using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Mobile;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Mobile;

public sealed class MobileExperienceService(
    OpenAmpDbContext dbContext,
    TimeProvider timeProvider) : IMobileExperienceService
{
    public async Task<IReadOnlyCollection<SalaCardDto>> PretraziSaleAsync(
        PretraziSaleQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.TerminOdUtc.HasValue != query.TerminDoUtc.HasValue
            || query.TerminOdUtc.HasValue && query.TerminDoUtc <= query.TerminOdUtc)
        {
            throw new ArgumentException("Za termin je potrebno poslati ispravan početak i kraj.");
        }

        var saleQuery = dbContext.Sale
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Studio)
            .Include(x => x.Status)
            .Include(x => x.Galerija)
            .Include(x => x.Oprema).ThenInclude(x => x.Kategorija)
            .Include(x => x.Oprema).ThenInclude(x => x.Status)
            .Include(x => x.Recenzije)
            .Include(x => x.Rezervacije).ThenInclude(x => x.Status)
            .Include(x => x.Rezervacije).ThenInclude(x => x.Bend).ThenInclude(x => x.Zanr)
            .Where(x => x.Studio.Aktivan && x.Status.Kod == "AKTIVNA");

        if (!string.IsNullOrWhiteSpace(query.Tekst))
        {
            var tekst = query.Tekst.Trim();
            saleQuery = saleQuery.Where(x =>
                x.Naziv.Contains(tekst)
                || x.Studio.Naziv.Contains(tekst)
                || x.Studio.Grad.Contains(tekst));
        }

        if (query.MinimalniKapacitet is > 0)
        {
            saleQuery = saleQuery.Where(x => x.Kapacitet >= query.MinimalniKapacitet);
        }

        if (!string.IsNullOrWhiteSpace(query.KategorijaOpremeKod))
        {
            var kod = query.KategorijaOpremeKod.Trim().ToUpperInvariant();
            saleQuery = saleQuery.Where(x =>
                x.Oprema.Any(o => o.Kategorija.Kod == kod && o.Status.Kod == "DOSTUPNA"));
        }

        if (!string.IsNullOrWhiteSpace(query.ZanrKod))
        {
            var zanr = query.ZanrKod.Trim().ToUpperInvariant();
            saleQuery = saleQuery.Where(x =>
                x.Rezervacije.Count == 0
                || x.Rezervacije.Any(r => r.Bend.Zanr.Kod == zanr));
        }

        var sale = await saleQuery.OrderBy(x => x.CijenaPoSatu).ToListAsync(cancellationToken);
        return sale.Select(x => UCardDto(
                x,
                !query.TerminOdUtc.HasValue || !x.Rezervacije.Any(r =>
                    r.Status.Kod != "OTKAZANA"
                    && r.TerminOdUtc < query.TerminDoUtc
                    && query.TerminOdUtc < r.TerminDoUtc)))
            .ToArray();
    }

    public async Task<SalaDetaljiDto> DohvatiSaluAsync(
        int salaId,
        CancellationToken cancellationToken = default)
    {
        var sala = await dbContext.Sale
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Studio).ThenInclude(x => x.Artikli).ThenInclude(x => x.Kategorija)
            .Include(x => x.Studio).ThenInclude(x => x.Artikli).ThenInclude(x => x.Status)
            .Include(x => x.Galerija)
            .Include(x => x.Oprema).ThenInclude(x => x.Kategorija)
            .Include(x => x.Oprema).ThenInclude(x => x.Status)
            .Include(x => x.Recenzije).ThenInclude(x => x.Korisnik)
            .SingleOrDefaultAsync(x => x.Id == salaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException($"Sala {salaId} nije pronađena.");

        var vidljiveRecenzije = sala.Recenzije.Where(x => x.Vidljiva).ToArray();
        return new SalaDetaljiDto(
            sala.Id,
            sala.Naziv,
            sala.Studio.Naziv,
            sala.Studio.Grad,
            sala.Studio.Adresa,
            sala.Kapacitet,
            sala.CijenaPoSatu,
            sala.Opis,
            sala.Akustika,
            sala.GeografskaSirina,
            sala.GeografskaDuzina,
            vidljiveRecenzije.Length == 0 ? 0 : decimal.Round((decimal)vidljiveRecenzije.Average(x => x.Ocjena), 1),
            vidljiveRecenzije.Length,
            sala.Galerija.OrderBy(x => x.Redoslijed).Select(x => x.Url).ToArray(),
            sala.Oprema
                .OrderBy(x => x.Kategorija.Naziv).ThenBy(x => x.Naziv)
                .Select(x => new OpremaZaNajamDto(
                    x.Id, x.Naziv, x.Kategorija.Naziv, x.Opis, x.CijenaNajmaPoSatu, x.Status.Kod == "DOSTUPNA"))
                .ToArray(),
            sala.Studio.Artikli
                .Where(x => x.Status.Kod == "AKTIVAN" && x.KolicinaNaStanju > 0)
                .OrderBy(x => x.Kategorija.Naziv).ThenBy(x => x.Naziv)
                .Select(x => new ArtikalZaKupovinuDto(
                    x.Id, x.Naziv, x.Kategorija.Naziv, x.Opis, x.CijenaKupovine, x.KolicinaNaStanju))
                .ToArray(),
            vidljiveRecenzije
                .OrderByDescending(x => x.KreiranaUtc)
                .Take(20)
                .Select(x => new RecenzijaSaleDto(
                    x.Id, x.Ocjena, x.Komentar, $"{x.Korisnik.Ime} {x.Korisnik.Prezime}", x.KreiranaUtc))
                .ToArray());
    }

    public async Task<MobileLookupsDto> DohvatiSifarnikeAsync(CancellationToken cancellationToken = default) =>
        new(
            await dbContext.Zanrovi.AsNoTracking().OrderBy(x => x.Naziv)
                .Select(x => new SifarnikDto(x.Id, x.Kod, x.Naziv)).ToArrayAsync(cancellationToken),
            await dbContext.KategorijeOpreme.AsNoTracking().OrderBy(x => x.Naziv)
                .Select(x => new SifarnikDto(x.Id, x.Kod, x.Naziv)).ToArrayAsync(cancellationToken),
            await dbContext.Instrumenti.AsNoTracking().OrderBy(x => x.Naziv)
                .Select(x => new SifarnikDto(x.Id, x.Kod, x.Naziv)).ToArrayAsync(cancellationToken));

    public async Task<IReadOnlyCollection<BendDto>> DohvatiBendoveAsync(
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        var bendovi = await dbContext.Bendovi
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Zanr)
            .Include(x => x.Clanovi).ThenInclude(x => x.Korisnik)
            .Include(x => x.Clanovi).ThenInclude(x => x.Instrument)
            .Include(x => x.Pozivnice).ThenInclude(x => x.Status)
            .Include(x => x.Rezervacije)
            .Where(x => x.OsnivacId == korisnikId || x.Clanovi.Any(c => c.KorisnikId == korisnikId && c.Aktivan))
            .OrderBy(x => x.Naziv)
            .ToListAsync(cancellationToken);
        return bendovi.Select(x => UBendDto(x, korisnikId)).ToArray();
    }

    public async Task<BendDto> KreirajBendAsync(
        KreirajBendCommand command,
        CancellationToken cancellationToken = default)
    {
        var naziv = command.Naziv.Trim();
        if (naziv.Length is < 2 or > 150)
        {
            throw new ArgumentException("Naziv benda mora imati između 2 i 150 znakova.");
        }

        if (!await dbContext.Korisnici.AnyAsync(x => x.Id == command.KorisnikId && x.Aktivan, cancellationToken))
        {
            throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        }

        var zanr = await dbContext.Zanrovi.SingleOrDefaultAsync(x => x.Id == command.ZanrId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Žanr nije pronađen.");
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        var bend = new Bend
        {
            Naziv = naziv,
            Opis = string.IsNullOrWhiteSpace(command.Opis) ? null : command.Opis.Trim(),
            OsnivacId = command.KorisnikId,
            ZanrId = zanr.Id,
            Zanr = zanr,
            KreiranUtc = sada,
            Clanovi =
            [
                new ClanBenda
                {
                    KorisnikId = command.KorisnikId,
                    DatumPridruzivanjaUtc = sada,
                    UlogaUBendu = "Osnivač",
                    Aktivan = true
                }
            ]
        };
        dbContext.Bendovi.Add(bend);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await DohvatiBendAsync(bend.Id, command.KorisnikId, cancellationToken);
    }

    public async Task<PozivnicaBendaDto> PosaljiPozivnicuAsync(
        PosaljiPozivnicuBendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Id == command.BendId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Bend nije pronađen.");
        if (bend.OsnivacId != command.KorisnikId)
        {
            throw new NedozvoljenaOperacijaException("Samo osnivač benda može slati pozivnice.");
        }

        var email = command.Email.Trim().ToLowerInvariant();
        if (email.Length is < 3 or > 320 || !email.Contains('@'))
        {
            throw new ArgumentException("Email adresa nije ispravna.");
        }

        var status = await dbContext.StatusiPozivnica.SingleAsync(x => x.Kod == "NA_CEKANJU", cancellationToken);
        var pozvani = await dbContext.Korisnici.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        var postojeca = await dbContext.PozivniceBenda.AnyAsync(
            x => x.BendId == bend.Id && x.Email == email && x.StatusPozivniceId == status.Id && x.IsticeUtc > sada,
            cancellationToken);
        if (postojeca)
        {
            throw new InvalidOperationException("Aktivna pozivnica za ovaj email već postoji.");
        }

        var pozivnica = new PozivnicaBenda
        {
            BendId = bend.Id,
            PozvaoKorisnikId = command.KorisnikId,
            PozvaniKorisnikId = pozvani?.Id,
            Email = email,
            Kod = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)),
            StatusPozivniceId = status.Id,
            Status = status,
            KreiranaUtc = sada,
            IsticeUtc = sada.AddDays(7)
        };
        dbContext.PozivniceBenda.Add(pozivnica);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new PozivnicaBendaDto(
            pozivnica.Id, pozivnica.Email, pozivnica.Kod, status.Naziv, pozivnica.IsticeUtc);
    }

    public async Task<IReadOnlyCollection<PrimljenaPozivnicaBendaDto>> DohvatiPrimljenePozivniceAsync(
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await dbContext.Korisnici.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == korisnikId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        var pozivnice = await dbContext.PozivniceBenda
            .Include(x => x.Bend).ThenInclude(x => x.Zanr)
            .Include(x => x.PozvaoKorisnik)
            .Include(x => x.Status)
            .Where(x => x.PozvaniKorisnikId == korisnikId || x.Email == korisnik.Email)
            .OrderByDescending(x => x.KreiranaUtc)
            .ToListAsync(cancellationToken);

        var istekle = pozivnice.Where(x => x.Status.Kod == "NA_CEKANJU" && x.IsticeUtc <= sada).ToArray();
        if (istekle.Length > 0)
        {
            var istekaoStatus = await dbContext.StatusiPozivnica.SingleAsync(x => x.Kod == "ISTEKLA", cancellationToken);
            foreach (var pozivnica in istekle)
            {
                pozivnica.Status = istekaoStatus;
                pozivnica.StatusPozivniceId = istekaoStatus.Id;
                pozivnica.OdgovorenaUtc = sada;
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return pozivnice.Select(UPrimljenuPozivnicuDto).ToArray();
    }

    public async Task<PrimljenaPozivnicaBendaDto> OdgovoriNaPozivnicuAsync(
        OdgovoriNaPozivnicuBendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await dbContext.Korisnici.SingleOrDefaultAsync(x => x.Id == command.KorisnikId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        var pozivnica = await dbContext.PozivniceBenda
            .Include(x => x.Bend).ThenInclude(x => x.Zanr)
            .Include(x => x.PozvaoKorisnik)
            .Include(x => x.Status)
            .SingleOrDefaultAsync(x => x.Id == command.PozivnicaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Pozivnica nije pronađena.");
        if (pozivnica.PozvaniKorisnikId != command.KorisnikId && pozivnica.Email != korisnik.Email)
        {
            throw new NedozvoljenaOperacijaException("Pozivnica nije namijenjena ovom korisniku.");
        }
        if (pozivnica.Status.Kod != "NA_CEKANJU")
        {
            throw new NedozvoljenaOperacijaException("Na ovu pozivnicu je već odgovoreno.");
        }

        var sada = timeProvider.GetUtcNow().UtcDateTime;
        if (pozivnica.IsticeUtc <= sada)
        {
            var istekao = await dbContext.StatusiPozivnica.SingleAsync(x => x.Kod == "ISTEKLA", cancellationToken);
            pozivnica.Status = istekao;
            pozivnica.StatusPozivniceId = istekao.Id;
            pozivnica.OdgovorenaUtc = sada;
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new NedozvoljenaOperacijaException("Pozivnica je istekla.");
        }

        if (command.InstrumentId.HasValue
            && !await dbContext.Instrumenti.AnyAsync(x => x.Id == command.InstrumentId.Value, cancellationToken))
        {
            throw new EntitetNijePronadjenException("Instrument nije pronađen.");
        }

        var statusKod = command.Prihvati ? "PRIHVACENA" : "ODBIJENA";
        var status = await dbContext.StatusiPozivnica.SingleAsync(x => x.Kod == statusKod, cancellationToken);
        pozivnica.PozvaniKorisnikId = korisnik.Id;
        pozivnica.Status = status;
        pozivnica.StatusPozivniceId = status.Id;
        pozivnica.OdgovorenaUtc = sada;
        if (command.Prihvati)
        {
            var clanstvo = await dbContext.ClanoviBenda.SingleOrDefaultAsync(
                x => x.BendId == pozivnica.BendId && x.KorisnikId == korisnik.Id,
                cancellationToken);
            if (clanstvo is null)
            {
                dbContext.ClanoviBenda.Add(new ClanBenda
                {
                    BendId = pozivnica.BendId,
                    KorisnikId = korisnik.Id,
                    InstrumentId = command.InstrumentId,
                    DatumPridruzivanjaUtc = sada,
                    UlogaUBendu = "Član",
                    Aktivan = true
                });
            }
            else
            {
                clanstvo.Aktivan = true;
                clanstvo.InstrumentId = command.InstrumentId;
                clanstvo.DatumPridruzivanjaUtc = sada;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return UPrimljenuPozivnicuDto(pozivnica);
    }

    public async Task<BendDto> AzurirajBendAsync(
        AzurirajBendCommand command,
        CancellationToken cancellationToken = default)
    {
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Id == command.BendId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Bend nije pronađen.");
        OsigurajOsnivaca(bend, command.KorisnikId);
        var naziv = command.Naziv.Trim();
        if (naziv.Length is < 2 or > 150)
        {
            throw new ArgumentException("Naziv benda mora imati između 2 i 150 znakova.");
        }
        if (!await dbContext.Zanrovi.AnyAsync(x => x.Id == command.ZanrId, cancellationToken))
        {
            throw new EntitetNijePronadjenException("Žanr nije pronađen.");
        }
        bend.Naziv = naziv;
        bend.ZanrId = command.ZanrId;
        bend.Opis = string.IsNullOrWhiteSpace(command.Opis) ? null : command.Opis.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return await DohvatiBendAsync(bend.Id, command.KorisnikId, cancellationToken);
    }

    public async Task<BendDto> AzurirajClanaAsync(
        AzurirajClanaBendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Id == command.BendId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Bend nije pronađen.");
        OsigurajOsnivaca(bend, command.KorisnikId);
        var clan = await dbContext.ClanoviBenda.SingleOrDefaultAsync(
            x => x.BendId == command.BendId && x.KorisnikId == command.ClanKorisnikId && x.Aktivan,
            cancellationToken) ?? throw new EntitetNijePronadjenException("Član benda nije pronađen.");
        if (command.InstrumentId.HasValue
            && !await dbContext.Instrumenti.AnyAsync(x => x.Id == command.InstrumentId.Value, cancellationToken))
        {
            throw new EntitetNijePronadjenException("Instrument nije pronađen.");
        }
        clan.InstrumentId = command.InstrumentId;
        clan.UlogaUBendu = string.IsNullOrWhiteSpace(command.Uloga) ? null : command.Uloga.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return await DohvatiBendAsync(bend.Id, command.KorisnikId, cancellationToken);
    }

    public async Task<BendDto> UkloniClanaAsync(
        UkloniClanaBendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Id == command.BendId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Bend nije pronađen.");
        var napustaSam = command.KorisnikId == command.ClanKorisnikId;
        if (!napustaSam)
        {
            OsigurajOsnivaca(bend, command.KorisnikId);
        }
        if (command.ClanKorisnikId == bend.OsnivacId)
        {
            throw new NedozvoljenaOperacijaException("Osnivač ne može napustiti bend niti biti uklonjen.");
        }
        var clan = await dbContext.ClanoviBenda.SingleOrDefaultAsync(
            x => x.BendId == command.BendId && x.KorisnikId == command.ClanKorisnikId && x.Aktivan,
            cancellationToken) ?? throw new EntitetNijePronadjenException("Član benda nije pronađen.");
        clan.Aktivan = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await DohvatiBendAsync(bend.Id, command.KorisnikId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MobileRezervacijaDto>> DohvatiRezervacijeAsync(
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        return await dbContext.Rezervacije
            .AsNoTracking()
            .Where(x => x.KreiraoKorisnikId == korisnikId
                || x.Bend.Clanovi.Any(c => c.KorisnikId == korisnikId && c.Aktivan))
            .OrderByDescending(x => x.TerminOdUtc)
            .Select(x => new MobileRezervacijaDto(
                x.Id,
                x.SalaId,
                x.Sala.Naziv,
                x.Sala.Studio.Naziv,
                x.BendId,
                x.Bend.Naziv,
                x.TerminOdUtc,
                x.TerminDoUtc,
                x.UkupnaCijena,
                x.Status.Naziv,
                x.Status.Kod,
                Convert.ToBase64String(x.RowVersion),
                x.Sala.Galerija.OrderBy(slika => slika.Redoslijed).Select(slika => slika.Url).FirstOrDefault(),
                x.KreiraoKorisnikId == korisnikId
                    && x.TerminOdUtc > sada
                    && (x.Status.Kod == "NA_CEKANJU" || x.Status.Kod == "PLACENA"),
                x.KreiraoKorisnikId == korisnikId
                    && x.TerminDoUtc <= sada
                    && (x.Status.Kod == "PLACENA" || x.Status.Kod == "IZVRSENA")
                    && x.Recenzija == null))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ProfilPregledDto> DohvatiProfilAsync(
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await dbContext.Korisnici
            .AsNoTracking()
            .Include(x => x.Instrumenti).ThenInclude(x => x.Instrument)
            .SingleOrDefaultAsync(x => x.Id == korisnikId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        var rezervacije = await dbContext.Rezervacije
            .AsNoTracking()
            .Include(x => x.Sala)
            .Include(x => x.Bend).ThenInclude(x => x.Zanr)
            .Where(x => x.KreiraoKorisnikId == korisnikId)
            .ToListAsync(cancellationToken);
        var brojBendova = await dbContext.ClanoviBenda.CountAsync(
            x => x.KorisnikId == korisnikId && x.Aktivan,
            cancellationToken);
        var brojRecenzija = await dbContext.Recenzije.CountAsync(x => x.KorisnikId == korisnikId, cancellationToken);
        var omiljenaSala = await dbContext.OmiljeneSale.AsNoTracking()
            .Where(x => x.KorisnikId == korisnikId)
            .OrderByDescending(x => x.KreiranaUtc)
            .Select(x => x.Sala.Naziv)
            .FirstOrDefaultAsync(cancellationToken);
        omiljenaSala ??= rezervacije
            .GroupBy(x => new { x.SalaId, x.Sala.Naziv })
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key.Naziv)
            .FirstOrDefault();
        var najcesciZanr = rezervacije
            .GroupBy(x => new { x.Bend.ZanrId, x.Bend.Zanr.Naziv })
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key.Naziv)
            .FirstOrDefault();
        var ukupnoSati = rezervacije
            .Where(x => x.StatusRezervacijeId != 4)
            .Sum(x => (decimal)(x.TerminDoUtc - x.TerminOdUtc).TotalHours);

        return new ProfilPregledDto(
            korisnik.Id,
            korisnik.Ime,
            korisnik.Prezime,
            korisnik.Email,
            korisnik.Telefon,
            korisnik.FotografijaUrl,
            korisnik.Instrumenti.OrderByDescending(x => x.Primarni).Select(x => x.Instrument.Naziv).ToArray(),
            brojBendova,
            rezervacije.Count,
            decimal.Round(ukupnoSati, 1),
            brojRecenzija,
            omiljenaSala,
            najcesciZanr);
    }

    public async Task<IReadOnlyCollection<int>> DohvatiOmiljeneSaleAsync(
        int korisnikId,
        CancellationToken cancellationToken = default) =>
        await dbContext.OmiljeneSale.AsNoTracking()
            .Where(x => x.KorisnikId == korisnikId)
            .OrderByDescending(x => x.KreiranaUtc)
            .Select(x => x.SalaId)
            .ToArrayAsync(cancellationToken);

    public async Task<bool> PostaviOmiljenuSaluAsync(
        PostaviOmiljenuSaluCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Korisnici.AnyAsync(x => x.Id == command.KorisnikId, cancellationToken)
            || !await dbContext.Sale.AnyAsync(x => x.Id == command.SalaId, cancellationToken))
        {
            throw new EntitetNijePronadjenException("Korisnik ili sala nisu pronađeni.");
        }
        var postojeca = await dbContext.OmiljeneSale.SingleOrDefaultAsync(
            x => x.KorisnikId == command.KorisnikId && x.SalaId == command.SalaId,
            cancellationToken);
        if (command.Sacuvana && postojeca is null)
        {
            dbContext.OmiljeneSale.Add(new OmiljenaSala
            {
                KorisnikId = command.KorisnikId,
                SalaId = command.SalaId,
                KreiranaUtc = timeProvider.GetUtcNow().UtcDateTime
            });
        }
        else if (!command.Sacuvana && postojeca is not null)
        {
            dbContext.OmiljeneSale.Remove(postojeca);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return command.Sacuvana;
    }

    public async Task<KorisnickePostavkeDto> DohvatiPostavkeAsync(
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Korisnici.AnyAsync(x => x.Id == korisnikId, cancellationToken))
        {
            throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        }
        var postavke = await dbContext.PostavkeKorisnika.AsNoTracking()
            .SingleOrDefaultAsync(x => x.KorisnikId == korisnikId, cancellationToken);
        return postavke is null
            ? new KorisnickePostavkeDto(true, true, "bs", true)
            : UPostavkeDto(postavke);
    }

    public async Task<KorisnickePostavkeDto> AzurirajPostavkeAsync(
        AzurirajKorisnickePostavkeCommand command,
        CancellationToken cancellationToken = default)
    {
        var jezik = command.Jezik.Trim().ToLowerInvariant();
        if (jezik is not ("bs" or "en"))
        {
            throw new ArgumentException("Podržani jezici su bs i en.");
        }
        if (!await dbContext.Korisnici.AnyAsync(x => x.Id == command.KorisnikId, cancellationToken))
        {
            throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        }
        var postavke = await dbContext.PostavkeKorisnika.SingleOrDefaultAsync(
            x => x.KorisnikId == command.KorisnikId,
            cancellationToken);
        if (postavke is null)
        {
            postavke = new PostavkeKorisnika { KorisnikId = command.KorisnikId };
            dbContext.PostavkeKorisnika.Add(postavke);
        }
        postavke.PushNotifikacije = command.PushNotifikacije;
        postavke.EmailNotifikacije = command.EmailNotifikacije;
        postavke.Jezik = jezik;
        postavke.ProfilJavan = command.ProfilJavan;
        postavke.AzuriraneUtc = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
        return UPostavkeDto(postavke);
    }

    public async Task<RecenzijaSaleDto> KreirajRecenzijuAsync(
        KreirajRecenzijuCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Ocjena is < 1 or > 5)
        {
            throw new ArgumentException("Ocjena mora biti između 1 i 5.");
        }
        var komentar = string.IsNullOrWhiteSpace(command.Komentar) ? null : command.Komentar.Trim();
        if (komentar?.Length > 2000)
        {
            throw new ArgumentException("Komentar može imati najviše 2000 znakova.");
        }
        var rezervacija = await dbContext.Rezervacije
            .Include(x => x.Status)
            .Include(x => x.KreiraoKorisnik)
            .Include(x => x.Recenzija)
            .SingleOrDefaultAsync(x => x.Id == command.RezervacijaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Rezervacija nije pronađena.");
        if (rezervacija.KreiraoKorisnikId != command.KorisnikId)
        {
            throw new NedozvoljenaOperacijaException("Samo korisnik koji je kreirao rezervaciju može ostaviti recenziju.");
        }
        if (rezervacija.TerminDoUtc > timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new NedozvoljenaOperacijaException("Recenziju je moguće ostaviti nakon završene probe.");
        }
        if (rezervacija.Status.Kod is not ("PLACENA" or "IZVRSENA"))
        {
            throw new NedozvoljenaOperacijaException("Recenzija je dostupna samo za plaćenu i završenu rezervaciju.");
        }
        if (rezervacija.Recenzija is not null)
        {
            throw new NedozvoljenaOperacijaException("Za ovu rezervaciju je već ostavljena recenzija.");
        }
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        var recenzija = new Recenzija
        {
            Ocjena = command.Ocjena,
            Komentar = komentar,
            KreiranaUtc = sada,
            Vidljiva = true,
            KorisnikId = command.KorisnikId,
            SalaId = rezervacija.SalaId,
            RezervacijaId = rezervacija.Id
        };
        dbContext.Recenzije.Add(recenzija);
        if (rezervacija.Status.Kod == "PLACENA")
        {
            var izvrsena = await dbContext.StatusiRezervacija.SingleAsync(x => x.Kod == "IZVRSENA", cancellationToken);
            rezervacija.Status = izvrsena;
            rezervacija.StatusRezervacijeId = izvrsena.Id;
            rezervacija.AzuriranaUtc = sada;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return new RecenzijaSaleDto(
            recenzija.Id,
            recenzija.Ocjena,
            recenzija.Komentar,
            $"{rezervacija.KreiraoKorisnik.Ime} {rezervacija.KreiraoKorisnik.Prezime}",
            recenzija.KreiranaUtc);
    }

    private async Task<BendDto> DohvatiBendAsync(
        int bendId,
        int korisnikId,
        CancellationToken cancellationToken)
    {
        var bend = await dbContext.Bendovi
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Zanr)
            .Include(x => x.Clanovi).ThenInclude(x => x.Korisnik)
            .Include(x => x.Clanovi).ThenInclude(x => x.Instrument)
            .Include(x => x.Pozivnice).ThenInclude(x => x.Status)
            .Include(x => x.Rezervacije)
            .SingleAsync(x => x.Id == bendId, cancellationToken);
        return UBendDto(bend, korisnikId);
    }

    private static SalaCardDto UCardDto(Sala sala, bool dostupna)
    {
        var recenzije = sala.Recenzije.Where(x => x.Vidljiva).ToArray();
        return new SalaCardDto(
            sala.Id,
            sala.Naziv,
            sala.Studio.Naziv,
            sala.Studio.Grad,
            sala.Kapacitet,
            sala.CijenaPoSatu,
            sala.Status.Naziv,
            sala.Galerija.OrderBy(x => x.Redoslijed).Select(x => x.Url).FirstOrDefault(),
            recenzije.Length == 0 ? 0 : decimal.Round((decimal)recenzije.Average(x => x.Ocjena), 1),
            recenzije.Length,
            sala.Oprema.Where(x => x.Status.Kod == "DOSTUPNA").Select(x => x.Naziv).Take(3).ToArray(),
            dostupna);
    }

    private static BendDto UBendDto(Bend bend, int korisnikId) =>
        new(
            bend.Id,
            bend.Naziv,
            bend.Zanr.Naziv,
            bend.Opis,
            bend.FotografijaUrl,
            bend.OsnivacId == korisnikId,
            bend.Rezervacije.Count,
            bend.Clanovi
                .Where(x => x.Aktivan)
                .OrderByDescending(x => x.KorisnikId == bend.OsnivacId)
                .ThenBy(x => x.Korisnik.Ime)
                .Select(x => new ClanBendaDto(
                    x.KorisnikId,
                    $"{x.Korisnik.Ime} {x.Korisnik.Prezime}",
                    x.Instrument == null ? null : x.Instrument.Naziv,
                    x.UlogaUBendu,
                    x.KorisnikId == bend.OsnivacId))
                .ToArray(),
            bend.Pozivnice
                .OrderByDescending(x => x.KreiranaUtc)
                .Select(x => new PozivnicaBendaDto(x.Id, x.Email, x.Kod, x.Status.Naziv, x.IsticeUtc))
                .ToArray());

    private static PrimljenaPozivnicaBendaDto UPrimljenuPozivnicuDto(PozivnicaBenda pozivnica) => new(
        pozivnica.Id,
        pozivnica.BendId,
        pozivnica.Bend.Naziv,
        pozivnica.Bend.Zanr.Naziv,
        $"{pozivnica.PozvaoKorisnik.Ime} {pozivnica.PozvaoKorisnik.Prezime}",
        pozivnica.Kod,
        pozivnica.Status.Naziv,
        pozivnica.KreiranaUtc,
        pozivnica.IsticeUtc);

    private static KorisnickePostavkeDto UPostavkeDto(PostavkeKorisnika postavke) => new(
        postavke.PushNotifikacije,
        postavke.EmailNotifikacije,
        postavke.Jezik,
        postavke.ProfilJavan);

    private static void OsigurajOsnivaca(Bend bend, int korisnikId)
    {
        if (bend.OsnivacId != korisnikId)
        {
            throw new NedozvoljenaOperacijaException("Samo osnivač može upravljati bendom.");
        }
    }
}
