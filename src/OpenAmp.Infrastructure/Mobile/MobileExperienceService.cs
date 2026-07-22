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

    public async Task<IReadOnlyCollection<MobileRezervacijaDto>> DohvatiRezervacijeAsync(
        int korisnikId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Rezervacije
            .AsNoTracking()
            .Where(x => x.KreiraoKorisnikId == korisnikId
                || x.Bend.Clanovi.Any(c => c.KorisnikId == korisnikId && c.Aktivan))
            .OrderByDescending(x => x.TerminOdUtc)
            .Select(x => new MobileRezervacijaDto(
                x.Id,
                x.Sala.Naziv,
                x.Sala.Studio.Naziv,
                x.Bend.Naziv,
                x.TerminOdUtc,
                x.TerminDoUtc,
                x.UkupnaCijena,
                x.Status.Naziv,
                Convert.ToBase64String(x.RowVersion),
                x.Sala.Galerija.OrderBy(slika => slika.Redoslijed).Select(slika => slika.Url).FirstOrDefault()))
            .ToArrayAsync(cancellationToken);

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
        var omiljenaSala = rezervacije
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
}
