using System.Data;
using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Desktop;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Desktop;

public sealed class DesktopAdminService(
    OpenAmpDbContext dbContext,
    TimeProvider timeProvider) : IDesktopAdminService
{
    private static readonly string[] AktivniStatusiRezervacije = ["NA_CEKANJU", "PLACENA"];

    public async Task<DesktopLookupsDto> DohvatiSifarnikeAsync(CancellationToken cancellationToken = default) =>
        new(
            await dbContext.Studiji.AsNoTracking().OrderBy(x => x.Naziv)
                .Select(x => new DesktopSifarnikDto(
                    x.Id,
                    x.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    x.Naziv)).ToArrayAsync(cancellationToken),
            await Sifarnik(dbContext.StatusiSala, cancellationToken),
            await Sifarnik(dbContext.KategorijeOpreme, cancellationToken),
            await Sifarnik(dbContext.StatusiOpreme, cancellationToken),
            await Sifarnik(dbContext.KategorijeArtikala, cancellationToken),
            await Sifarnik(dbContext.StatusiArtikala, cancellationToken),
            await Sifarnik(dbContext.StatusiRezervacija, cancellationToken),
            await Sifarnik(dbContext.Zanrovi, cancellationToken),
            await Sifarnik(dbContext.Uloge, cancellationToken));

    public async Task<DesktopDashboardDto> DohvatiDashboardAsync(
        int? studioId,
        CancellationToken cancellationToken = default)
    {
        var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
        var zona = TimeZoneInfo.FindSystemTimeZoneById("Europe/Sarajevo");
        var lokalno = TimeZoneInfo.ConvertTimeFromUtc(sadaUtc, zona);
        var danas = DateOnly.FromDateTime(lokalno);
        var pocetakSedmice = danas.AddDays(-((int)lokalno.DayOfWeek + 6) % 7);
        var sedmicaOdUtc = ULokalniUtc(pocetakSedmice, new TimeOnly(0, 0), zona);
        var sedmicaDoUtc = ULokalniUtc(pocetakSedmice.AddDays(7), new TimeOnly(0, 0), zona);
        var danasOdUtc = ULokalniUtc(danas, new TimeOnly(0, 0), zona);
        var danasDoUtc = danasOdUtc.AddDays(1);

        var rezervacije = await dbContext.Rezervacije.AsNoTracking()
            .Where(x => (!studioId.HasValue || x.Sala.StudioId == studioId)
                && x.TerminOdUtc < sedmicaDoUtc
                && sedmicaOdUtc < x.TerminDoUtc
                && x.Status.Kod != "OTKAZANA")
            .Select(x => new
            {
                x.Id,
                x.SalaId,
                Sala = x.Sala.Naziv,
                x.Sala.StudioId,
                x.Sala.Studio.RadnoVrijemeOd,
                x.Sala.Studio.RadnoVrijemeDo,
                Bend = x.Bend.Naziv,
                Zanr = x.Bend.Zanr.Naziv,
                x.TerminOdUtc,
                x.TerminDoUtc,
                Status = x.Status.Naziv
            })
            .ToArrayAsync(cancellationToken);
        var danasnje = rezervacije.Where(x => x.TerminOdUtc < danasDoUtc && danasOdUtc < x.TerminDoUtc).ToArray();
        var sale = await dbContext.Sale.AsNoTracking()
            .Where(x => (!studioId.HasValue || x.StudioId == studioId) && x.Status.Kod == "AKTIVNA")
            .Select(x => new { x.Id, x.Naziv, x.Studio.RadnoVrijemeOd, x.Studio.RadnoVrijemeDo })
            .ToArrayAsync(cancellationToken);
        var artikli = await dbContext.Artikli.AsNoTracking()
            .Where(x => (!studioId.HasValue || x.StudioId == studioId) && x.KolicinaNaStanju <= x.MinimalnaZaliha)
            .Include(x => x.Kategorija).Include(x => x.Status).Include(x => x.Studio)
            .OrderBy(x => x.KolicinaNaStanju)
            .ToArrayAsync(cancellationToken);
        var naNajmu = await dbContext.StavkeRezervacija.AsNoTracking()
            .Where(x => x.OpremaId.HasValue
                && (!studioId.HasValue || x.Rezervacija.Sala.StudioId == studioId)
                && AktivniStatusiRezervacije.Contains(x.Rezervacija.Status.Kod)
                && x.Rezervacija.TerminOdUtc <= sadaUtc
                && sadaUtc < x.Rezervacija.TerminDoUtc)
            .Select(x => x.OpremaId!.Value)
            .Distinct()
            .CountAsync(cancellationToken);

        var zauzetost = new List<ZauzetostSaleDto>();
        foreach (var sala in sale)
        {
            var dostupniSati = Math.Max(1, (sala.RadnoVrijemeDo - sala.RadnoVrijemeOd).TotalHours);
            for (var dan = 0; dan < 7; dan++)
            {
                var datum = pocetakSedmice.AddDays(dan);
                var danOd = ULokalniUtc(datum, sala.RadnoVrijemeOd, zona);
                var danDo = ULokalniUtc(datum, sala.RadnoVrijemeDo, zona);
                var zauzetiSati = rezervacije
                    .Where(x => x.SalaId == sala.Id && x.TerminOdUtc < danDo && danOd < x.TerminDoUtc)
                    .Sum(x => Math.Max(0, (Min(x.TerminDoUtc, danDo) - Max(x.TerminOdUtc, danOd)).TotalHours));
                zauzetost.Add(new ZauzetostSaleDto(
                    sala.Id,
                    sala.Naziv,
                    datum,
                    (int)Math.Clamp(Math.Round(zauzetiSati / dostupniSati * 100), 0, 100)));
            }
        }

        return new DesktopDashboardDto(
            danasnje.Length,
            sale.Length,
            naNajmu,
            artikli.Length,
            danasnje.OrderBy(x => x.TerminOdUtc).Select(x => new DashboardRezervacijaDto(
                x.Id,
                TimeZoneInfo.ConvertTimeFromUtc(x.TerminOdUtc, zona)
                    .ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                x.Bend,
                x.Sala,
                x.Zanr,
                x.Status)).ToArray(),
            zauzetost,
            artikli.Select(UArtikalDto).ToArray());
    }

    public async Task<IReadOnlyCollection<DesktopSalaDto>> DohvatiSaleAsync(
        string? tekst,
        int? statusId,
        int? minimalniKapacitet,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Sale.AsNoTracking().AsSplitQuery()
            .Include(x => x.Studio).Include(x => x.Status)
            .Include(x => x.Galerija).Include(x => x.Oprema).AsQueryable();
        if (!string.IsNullOrWhiteSpace(tekst))
        {
            var trazeno = tekst.Trim();
            query = query.Where(x => x.Naziv.Contains(trazeno) || x.Studio.Naziv.Contains(trazeno));
        }
        if (statusId.HasValue)
        {
            query = query.Where(x => x.StatusSaleId == statusId);
        }
        if (minimalniKapacitet.HasValue)
        {
            query = query.Where(x => x.Kapacitet >= minimalniKapacitet);
        }
        return (await query.OrderBy(x => x.Naziv).ToArrayAsync(cancellationToken)).Select(USalaDto).ToArray();
    }

    public async Task<DesktopSalaDto> SacuvajSaluAsync(
        int? id,
        SacuvajSaluDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidirajSalu(dto);
        await OsigurajPostojiAsync(dbContext.Studiji, dto.StudioId, "Studio", cancellationToken);
        await OsigurajPostojiAsync(dbContext.StatusiSala, dto.StatusId, "Status sale", cancellationToken);
        Sala sala;
        if (id.HasValue)
        {
            sala = await dbContext.Sale.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new EntitetNijePronadjenException("Sala nije pronađena.");
        }
        else
        {
            sala = new Sala();
            dbContext.Sale.Add(sala);
        }
        sala.StudioId = dto.StudioId;
        sala.Naziv = dto.Naziv.Trim();
        sala.Kapacitet = dto.Kapacitet;
        sala.CijenaPoSatu = dto.CijenaPoSatu;
        sala.StatusSaleId = dto.StatusId;
        sala.Opis = PraznoUNull(dto.Opis);
        sala.Akustika = PraznoUNull(dto.Akustika);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await UcitajSaluAsync(sala.Id, cancellationToken);
    }

    public async Task ObrisiSaluAsync(int id, CancellationToken cancellationToken = default)
    {
        var sala = await dbContext.Sale.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Sala nije pronađena.");
        var status = await dbContext.StatusiSala.SingleAsync(x => x.Kod == "NEAKTIVNA", cancellationToken);
        sala.StatusSaleId = status.Id;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DesktopOpremaDto>> DohvatiOpremuAsync(
        int? kategorijaId,
        int? statusId,
        int? salaId,
        CancellationToken cancellationToken = default)
    {
        var query = OpremaQuery().AsNoTracking();
        if (kategorijaId.HasValue)
        {
            query = query.Where(x => x.KategorijaOpremeId == kategorijaId);
        }
        if (statusId.HasValue)
        {
            query = query.Where(x => x.StatusOpremeId == statusId);
        }
        if (salaId.HasValue)
        {
            query = query.Where(x => x.SalaId == salaId);
        }
        return (await query.OrderBy(x => x.InventarskiBroj).ToArrayAsync(cancellationToken)).Select(UOpremaDto).ToArray();
    }

    public async Task<DesktopOpremaDto> SacuvajOpremuAsync(
        int? id,
        SacuvajOpremuDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.InventarskiBroj) || string.IsNullOrWhiteSpace(dto.Naziv)
            || dto.CijenaNajmaPoSatu < 0 || dto.Stanje is < 1 or > 5)
        {
            throw new ArgumentException("Podaci opreme nisu ispravni.");
        }
        await OsigurajPostojiAsync(dbContext.KategorijeOpreme, dto.KategorijaId, "Kategorija opreme", cancellationToken);
        await OsigurajPostojiAsync(dbContext.StatusiOpreme, dto.StatusId, "Status opreme", cancellationToken);
        if (dto.SalaId.HasValue)
        {
            await OsigurajPostojiAsync(dbContext.Sale, dto.SalaId.Value, "Sala", cancellationToken);
        }
        var broj = dto.InventarskiBroj.Trim().ToUpperInvariant();
        if (await dbContext.Oprema.AnyAsync(x => x.InventarskiBroj == broj && (!id.HasValue || x.Id != id), cancellationToken))
        {
            throw new ArgumentException("Inventarski broj opreme već postoji.");
        }
        Oprema oprema;
        if (id.HasValue)
        {
            oprema = await dbContext.Oprema.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new EntitetNijePronadjenException("Oprema nije pronađena.");
        }
        else
        {
            oprema = new Oprema();
            dbContext.Oprema.Add(oprema);
        }
        oprema.InventarskiBroj = broj;
        oprema.Naziv = dto.Naziv.Trim();
        oprema.Opis = PraznoUNull(dto.Opis);
        oprema.SerijskiBroj = PraznoUNull(dto.SerijskiBroj);
        oprema.CijenaNajmaPoSatu = dto.CijenaNajmaPoSatu;
        oprema.Stanje = dto.Stanje;
        oprema.DatumNabavke = dto.DatumNabavke;
        oprema.Napomena = PraznoUNull(dto.Napomena);
        oprema.KategorijaOpremeId = dto.KategorijaId;
        oprema.StatusOpremeId = dto.StatusId;
        oprema.SalaId = dto.SalaId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await UcitajOpremuAsync(oprema.Id, cancellationToken);
    }

    public async Task<DesktopOpremaDto> PrijaviServisAsync(
        int id,
        int korisnikId,
        PrijaviServisDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.OpisKvara))
        {
            throw new ArgumentException("Opis kvara je obavezan.");
        }
        var oprema = await dbContext.Oprema.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Oprema nije pronađena.");
        var servisStatus = await dbContext.StatusiOpreme.SingleAsync(x => x.Kod == "SERVIS", cancellationToken);
        oprema.StatusOpremeId = servisStatus.Id;
        oprema.ServisnaHistorija.Add(new ServisOpreme
        {
            PrijavljenUtc = timeProvider.GetUtcNow().UtcDateTime,
            OpisKvara = dto.OpisKvara.Trim(),
            PrijavioKorisnikId = korisnikId
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return await UcitajOpremuAsync(id, cancellationToken);
    }

    public async Task<DesktopOpremaDto> ZavrsiServisAsync(
        int id,
        int servisId,
        ZavrsiServisDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.IzvrseniRadovi) || dto.Trosak < 0 || dto.Stanje is < 1 or > 5)
        {
            throw new ArgumentException("Podaci završenog servisa nisu ispravni.");
        }
        await OsigurajPostojiAsync(dbContext.StatusiOpreme, dto.StatusId, "Status opreme", cancellationToken);
        var servis = await dbContext.ServisiOpreme.Include(x => x.Oprema)
            .SingleOrDefaultAsync(x => x.Id == servisId && x.OpremaId == id, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Servis nije pronađen.");
        if (servis.ZavrsenUtc.HasValue)
        {
            throw new ArgumentException("Servis je već završen.");
        }
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        servis.ZavrsenUtc = sada;
        servis.IzvrseniRadovi = dto.IzvrseniRadovi.Trim();
        servis.Trosak = dto.Trosak;
        servis.Oprema.DatumZadnjegServisa = DateOnly.FromDateTime(sada);
        servis.Oprema.Stanje = dto.Stanje;
        servis.Oprema.StatusOpremeId = dto.StatusId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await UcitajOpremuAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DesktopArtikalDto>> DohvatiArtikleAsync(
        int? studioId,
        bool samoNiskaZaliha,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Artikli.AsNoTracking()
            .Include(x => x.Kategorija).Include(x => x.Status).Include(x => x.Studio).AsQueryable();
        if (studioId.HasValue)
        {
            query = query.Where(x => x.StudioId == studioId);
        }
        if (samoNiskaZaliha)
        {
            query = query.Where(x => x.KolicinaNaStanju <= x.MinimalnaZaliha);
        }
        return (await query.OrderBy(x => x.Naziv).ToArrayAsync(cancellationToken)).Select(UArtikalDto).ToArray();
    }

    public async Task<DesktopArtikalDto> SacuvajArtikalAsync(
        int? id,
        SacuvajArtikalDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.InventarskiBroj) || string.IsNullOrWhiteSpace(dto.Naziv)
            || dto.KolicinaNaStanju < 0 || dto.MinimalnaZaliha < 0 || dto.Cijena < 0)
        {
            throw new ArgumentException("Podaci artikla nisu ispravni.");
        }
        await OsigurajPostojiAsync(dbContext.Studiji, dto.StudioId, "Studio", cancellationToken);
        await OsigurajPostojiAsync(dbContext.KategorijeArtikala, dto.KategorijaId, "Kategorija artikla", cancellationToken);
        await OsigurajPostojiAsync(dbContext.StatusiArtikala, dto.StatusId, "Status artikla", cancellationToken);
        var broj = dto.InventarskiBroj.Trim().ToUpperInvariant();
        if (await dbContext.Artikli.AnyAsync(x => x.InventarskiBroj == broj && (!id.HasValue || x.Id != id), cancellationToken))
        {
            throw new ArgumentException("Inventarski broj artikla već postoji.");
        }
        Artikal artikal;
        if (id.HasValue)
        {
            artikal = await dbContext.Artikli.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new EntitetNijePronadjenException("Artikal nije pronađen.");
        }
        else
        {
            artikal = new Artikal();
            dbContext.Artikli.Add(artikal);
        }
        artikal.InventarskiBroj = broj;
        artikal.Naziv = dto.Naziv.Trim();
        artikal.Opis = PraznoUNull(dto.Opis);
        artikal.KolicinaNaStanju = dto.KolicinaNaStanju;
        artikal.MinimalnaZaliha = dto.MinimalnaZaliha;
        artikal.CijenaKupovine = dto.Cijena;
        artikal.KategorijaArtiklaId = dto.KategorijaId;
        artikal.StatusArtiklaId = dto.StatusId;
        artikal.StudioId = dto.StudioId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await UcitajArtikalAsync(artikal.Id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DesktopRezervacijaDto>> DohvatiRezervacijeAsync(
        DateTime odUtc,
        DateTime doUtc,
        int? salaId,
        CancellationToken cancellationToken = default)
    {
        if (doUtc <= odUtc || doUtc - odUtc > TimeSpan.FromDays(62))
        {
            throw new ArgumentException("Period rezervacija nije ispravan.");
        }
        var query = RezervacijeQuery().AsNoTracking()
            .Where(x => x.TerminOdUtc < doUtc && odUtc < x.TerminDoUtc);
        if (salaId.HasValue)
        {
            query = query.Where(x => x.SalaId == salaId);
        }
        return (await query.OrderBy(x => x.TerminOdUtc).ToArrayAsync(cancellationToken))
            .Select(URezervacijaDto).ToArray();
    }

    public async Task<DesktopRezervacijaDto> KreirajRezervacijuAsync(
        int korisnikId,
        SacuvajDesktopRezervacijuDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidirajTermin(dto.TerminOdUtc, dto.TerminDoUtc);
        await using var transakcija = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var sala = await dbContext.Sale.Include(x => x.Status).Include(x => x.Studio)
            .SingleOrDefaultAsync(x => x.Id == dto.SalaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Sala nije pronađena.");
        if (sala.Status.Kod != "AKTIVNA")
        {
            throw new ArgumentException("Rezervacija se može kreirati samo za aktivnu salu.");
        }
        OsigurajUnutarRadnogVremena(sala.Studio, dto.TerminOdUtc, dto.TerminDoUtc);
        await OsigurajPostojiAsync(dbContext.Bendovi, dto.BendId, "Bend", cancellationToken);
        await OsigurajSlobodanTerminAsync(dto.SalaId, dto.TerminOdUtc, dto.TerminDoUtc, null, cancellationToken);
        var status = await dbContext.StatusiRezervacija.SingleAsync(x => x.Kod == "NA_CEKANJU", cancellationToken);
        var trajanje = (decimal)(dto.TerminDoUtc - dto.TerminOdUtc).TotalHours;
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        var rezervacija = new Rezervacija
        {
            SalaId = dto.SalaId,
            BendId = dto.BendId,
            KreiraoKorisnikId = korisnikId,
            TerminOdUtc = dto.TerminOdUtc,
            TerminDoUtc = dto.TerminDoUtc,
            UkupnaCijena = decimal.Round(sala.CijenaPoSatu * trajanje, 2),
            StatusRezervacijeId = status.Id,
            Napomena = PraznoUNull(dto.Napomena),
            KreiranaUtc = sada,
            AzuriranaUtc = sada
        };
        dbContext.Rezervacije.Add(rezervacija);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transakcija.CommitAsync(cancellationToken);
        return await UcitajRezervacijuAsync(rezervacija.Id, cancellationToken);
    }

    public async Task<DesktopRezervacijaDto> IzmijeniRezervacijuAsync(
        int id,
        IzmijeniDesktopRezervacijuDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidirajTermin(dto.TerminOdUtc, dto.TerminDoUtc);
        var rezervacija = await dbContext.Rezervacije.Include(x => x.Sala)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Rezervacija nije pronađena.");
        var rowVersion = DekodirajRowVersion(dto.RowVersion);
        if (!rezervacija.RowVersion.AsSpan().SequenceEqual(rowVersion))
        {
            throw new KonfliktKonkurentnostiException("Rezervaciju je u međuvremenu izmijenio drugi korisnik.");
        }
        var sala = await dbContext.Sale.Include(x => x.Status).Include(x => x.Studio)
            .SingleOrDefaultAsync(x => x.Id == dto.SalaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Sala nije pronađena.");
        if (sala.Status.Kod != "AKTIVNA")
        {
            throw new ArgumentException("Rezervacija se može vezati samo za aktivnu salu.");
        }
        OsigurajUnutarRadnogVremena(sala.Studio, dto.TerminOdUtc, dto.TerminDoUtc);
        await OsigurajPostojiAsync(dbContext.StatusiRezervacija, dto.StatusId, "Status rezervacije", cancellationToken);
        await OsigurajSlobodanTerminAsync(dto.SalaId, dto.TerminOdUtc, dto.TerminDoUtc, id, cancellationToken);
        dbContext.Entry(rezervacija).Property(x => x.RowVersion).OriginalValue = rowVersion;
        var dodatno = await dbContext.StavkeRezervacija.Where(x => x.RezervacijaId == id).SumAsync(
            x => x.OpremaId.HasValue ? x.JedinicnaCijena * x.Kolicina * (decimal)(dto.TerminDoUtc - dto.TerminOdUtc).TotalHours : x.UkupnaCijena,
            cancellationToken);
        rezervacija.SalaId = dto.SalaId;
        rezervacija.TerminOdUtc = dto.TerminOdUtc;
        rezervacija.TerminDoUtc = dto.TerminDoUtc;
        rezervacija.StatusRezervacijeId = dto.StatusId;
        rezervacija.Napomena = PraznoUNull(dto.Napomena);
        rezervacija.UkupnaCijena = decimal.Round(
            sala.CijenaPoSatu * (decimal)(dto.TerminDoUtc - dto.TerminOdUtc).TotalHours + dodatno, 2);
        rezervacija.AzuriranaUtc = timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new KonfliktKonkurentnostiException("Rezervaciju je u međuvremenu izmijenio drugi korisnik.", exception);
        }
        return await UcitajRezervacijuAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DesktopBendDto>> DohvatiBendoveAsync(
        string? tekst,
        int? zanrId,
        CancellationToken cancellationToken = default)
    {
        var query = BendoviQuery().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(tekst))
        {
            var trazeno = tekst.Trim();
            query = query.Where(x => x.Naziv.Contains(trazeno)
                || x.Clanovi.Any(c => c.Korisnik.Username.Contains(trazeno)
                    || c.Korisnik.Ime.Contains(trazeno)
                    || c.Korisnik.Prezime.Contains(trazeno)));
        }
        if (zanrId.HasValue)
        {
            query = query.Where(x => x.ZanrId == zanrId);
        }
        return (await query.OrderBy(x => x.Naziv).ToArrayAsync(cancellationToken)).Select(UBendDto).ToArray();
    }

    public async Task<DesktopBendDto> IzmijeniBendAsync(
        int id,
        IzmijeniDesktopBendDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Naziv))
        {
            throw new ArgumentException("Naziv benda je obavezan.");
        }
        await OsigurajPostojiAsync(dbContext.Zanrovi, dto.ZanrId, "Žanr", cancellationToken);
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Bend nije pronađen.");
        bend.Naziv = dto.Naziv.Trim();
        bend.ZanrId = dto.ZanrId;
        bend.Opis = PraznoUNull(dto.Opis);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await UcitajBendAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DesktopKorisnikDto>> DohvatiKorisnikeAsync(
        string? tekst,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Korisnici.AsNoTracking().Include(x => x.Uloga).AsQueryable();
        if (!string.IsNullOrWhiteSpace(tekst))
        {
            var trazeno = tekst.Trim();
            query = query.Where(x => x.Username.Contains(trazeno) || x.Email.Contains(trazeno)
                || x.Ime.Contains(trazeno) || x.Prezime.Contains(trazeno));
        }
        return (await query.OrderBy(x => x.Ime).ThenBy(x => x.Prezime).ToArrayAsync(cancellationToken))
            .Select(UKorisnikDto).ToArray();
    }

    public async Task<DesktopKorisnikDto> IzmijeniKorisnikaAsync(
        int id,
        IzmijeniDesktopKorisnikDto dto,
        CancellationToken cancellationToken = default)
    {
        await OsigurajPostojiAsync(dbContext.Uloge, dto.UlogaId, "Uloga", cancellationToken);
        var korisnik = await dbContext.Korisnici.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        korisnik.UlogaId = dto.UlogaId;
        korisnik.Aktivan = dto.Aktivan;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await dbContext.Korisnici.AsNoTracking().Include(x => x.Uloga)
            .Where(x => x.Id == id).Select(x => new DesktopKorisnikDto(
                x.Id, x.Username, x.Ime, x.Prezime, x.Email, x.Telefon,
                x.UlogaId, x.Uloga.Naziv, x.Uloga.Kod, x.Aktivan, x.KreiranUtc))
            .SingleAsync(cancellationToken);
    }

    private IQueryable<Oprema> OpremaQuery() => dbContext.Oprema.AsSplitQuery()
        .Include(x => x.Kategorija).Include(x => x.Status).Include(x => x.Sala)
        .Include(x => x.ServisnaHistorija).ThenInclude(x => x.PrijavioKorisnik);

    private IQueryable<Rezervacija> RezervacijeQuery() => dbContext.Rezervacije
        .Include(x => x.Sala).Include(x => x.Bend).ThenInclude(x => x.Zanr).Include(x => x.Status);

    private IQueryable<Bend> BendoviQuery() => dbContext.Bendovi.AsSplitQuery()
        .Include(x => x.Zanr).Include(x => x.Rezervacije)
        .Include(x => x.Clanovi).ThenInclude(x => x.Korisnik)
        .Include(x => x.Clanovi).ThenInclude(x => x.Instrument);

    private async Task<DesktopSalaDto> UcitajSaluAsync(int id, CancellationToken cancellationToken) =>
        USalaDto(await dbContext.Sale.AsNoTracking().AsSplitQuery()
            .Include(x => x.Studio).Include(x => x.Status).Include(x => x.Galerija).Include(x => x.Oprema)
            .SingleAsync(x => x.Id == id, cancellationToken));

    private async Task<DesktopOpremaDto> UcitajOpremuAsync(int id, CancellationToken cancellationToken) =>
        UOpremaDto(await OpremaQuery().AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken));

    private async Task<DesktopArtikalDto> UcitajArtikalAsync(int id, CancellationToken cancellationToken) =>
        UArtikalDto(await dbContext.Artikli.AsNoTracking()
            .Include(x => x.Kategorija).Include(x => x.Status).Include(x => x.Studio)
            .SingleAsync(x => x.Id == id, cancellationToken));

    private async Task<DesktopRezervacijaDto> UcitajRezervacijuAsync(int id, CancellationToken cancellationToken) =>
        URezervacijaDto(await RezervacijeQuery().AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken));

    private async Task<DesktopBendDto> UcitajBendAsync(int id, CancellationToken cancellationToken) =>
        UBendDto(await BendoviQuery().AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken));

    private async Task OsigurajSlobodanTerminAsync(
        int salaId,
        DateTime odUtc,
        DateTime doUtc,
        int? izuzmiId,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Rezervacije.AnyAsync(x => x.SalaId == salaId
            && (!izuzmiId.HasValue || x.Id != izuzmiId)
            && AktivniStatusiRezervacije.Contains(x.Status.Kod)
            && x.TerminOdUtc < doUtc && odUtc < x.TerminDoUtc, cancellationToken))
        {
            throw new TerminNijeDostupanException("Sala je već rezervisana u odabranom terminu.");
        }
    }

    private static async Task<IReadOnlyCollection<DesktopSifarnikDto>> Sifarnik<TEntity>(
        DbSet<TEntity> set,
        CancellationToken cancellationToken) where TEntity : class =>
        await set.AsNoTracking().OrderBy(x => EF.Property<string>(x, "Naziv"))
            .Select(x => new DesktopSifarnikDto(
                EF.Property<int>(x, "Id"),
                EF.Property<string>(x, "Kod"),
                EF.Property<string>(x, "Naziv")))
            .ToArrayAsync(cancellationToken);

    private static async Task OsigurajPostojiAsync<TEntity>(
        DbSet<TEntity> set,
        int id,
        string naziv,
        CancellationToken cancellationToken) where TEntity : class
    {
        if (!await set.AnyAsync(x => EF.Property<int>(x, "Id") == id, cancellationToken))
        {
            throw new EntitetNijePronadjenException($"{naziv} nije pronađen.");
        }
    }

    private static DesktopSalaDto USalaDto(Sala x) => new(
        x.Id, x.StudioId, x.Studio.Naziv, x.Naziv, x.Kapacitet, x.CijenaPoSatu,
        x.StatusSaleId, x.Status.Naziv, x.Status.Kod, x.Opis, x.Akustika,
        x.Galerija.OrderBy(s => s.Redoslijed).Select(s => s.Url).FirstOrDefault(),
        x.Oprema.OrderBy(o => o.Naziv).Select(o => o.Naziv).Take(5).ToArray());

    private static DesktopOpremaDto UOpremaDto(Oprema x) => new(
        x.Id, x.InventarskiBroj, x.Naziv, x.Opis, x.SerijskiBroj, x.CijenaNajmaPoSatu,
        x.Stanje, x.DatumNabavke, x.DatumZadnjegServisa, x.Napomena,
        x.KategorijaOpremeId, x.Kategorija.Naziv, x.StatusOpremeId, x.Status.Naziv, x.Status.Kod,
        x.SalaId, x.Sala?.Naziv,
        x.ServisnaHistorija.OrderByDescending(s => s.PrijavljenUtc).Select(s => new ServisOpremeDto(
            s.Id, s.PrijavljenUtc, s.ZavrsenUtc, s.OpisKvara, s.IzvrseniRadovi, s.Trosak,
            $"{s.PrijavioKorisnik.Ime} {s.PrijavioKorisnik.Prezime}")).ToArray());

    private static DesktopArtikalDto UArtikalDto(Artikal x) => new(
        x.Id, x.InventarskiBroj, x.Naziv, x.Opis, x.KolicinaNaStanju, x.MinimalnaZaliha,
        x.CijenaKupovine, x.KategorijaArtiklaId, x.Kategorija.Naziv, x.StatusArtiklaId,
        x.Status.Naziv, x.StudioId, x.Studio.Naziv, x.KolicinaNaStanju <= x.MinimalnaZaliha);

    private static DesktopRezervacijaDto URezervacijaDto(Rezervacija x) => new(
        x.Id, x.SalaId, x.Sala.Naziv, x.BendId, x.Bend.Naziv, x.Bend.Zanr.Naziv,
        x.TerminOdUtc, x.TerminDoUtc, x.UkupnaCijena, x.StatusRezervacijeId,
        x.Status.Naziv, x.Status.Kod, x.Napomena, Convert.ToBase64String(x.RowVersion));

    private static DesktopBendDto UBendDto(Bend x) => new(
        x.Id, x.Naziv, x.ZanrId, x.Zanr.Naziv, x.Opis, x.FotografijaUrl, x.Rezervacije.Count,
        x.Clanovi.Where(c => c.Aktivan).OrderByDescending(c => c.KorisnikId == x.OsnivacId)
            .ThenBy(c => c.Korisnik.Ime).Select(c => new DesktopClanBendaDto(
                c.KorisnikId, c.Korisnik.Username, $"{c.Korisnik.Ime} {c.Korisnik.Prezime}",
                c.Instrument == null ? null : c.Instrument.Naziv, c.UlogaUBendu,
                c.KorisnikId == x.OsnivacId)).ToArray());

    private static DesktopKorisnikDto UKorisnikDto(Korisnik x) => new(
        x.Id, x.Username, x.Ime, x.Prezime, x.Email, x.Telefon, x.UlogaId,
        x.Uloga.Naziv, x.Uloga.Kod, x.Aktivan, x.KreiranUtc);

    private static void ValidirajSalu(SacuvajSaluDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Naziv) || dto.Kapacitet <= 0 || dto.CijenaPoSatu < 0)
        {
            throw new ArgumentException("Podaci sale nisu ispravni.");
        }
    }

    private static void ValidirajTermin(DateTime odUtc, DateTime doUtc)
    {
        if (doUtc <= odUtc || doUtc - odUtc > TimeSpan.FromHours(12))
        {
            throw new ArgumentException("Termin rezervacije nije ispravan.");
        }
    }

    private static void OsigurajUnutarRadnogVremena(Studio studio, DateTime odUtc, DateTime doUtc)
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(studio.VremenskaZona);
        var lokalniOd = TimeZoneInfo.ConvertTimeFromUtc(odUtc, zona);
        var lokalniDo = TimeZoneInfo.ConvertTimeFromUtc(doUtc, zona);
        if (DateOnly.FromDateTime(lokalniOd) != DateOnly.FromDateTime(lokalniDo)
            || TimeOnly.FromDateTime(lokalniOd) < studio.RadnoVrijemeOd
            || TimeOnly.FromDateTime(lokalniDo) > studio.RadnoVrijemeDo)
        {
            throw new ArgumentException("Termin mora biti unutar radnog vremena studija.");
        }
    }

    private static byte[] DekodirajRowVersion(string rowVersion)
    {
        try
        {
            return Convert.FromBase64String(rowVersion);
        }
        catch (FormatException)
        {
            throw new ArgumentException("RowVersion nije ispravan.");
        }
    }

    private static DateTime ULokalniUtc(DateOnly datum, TimeOnly vrijeme, TimeZoneInfo zona) =>
        TimeZoneInfo.ConvertTimeToUtc(datum.ToDateTime(vrijeme, DateTimeKind.Unspecified), zona);

    private static string? PraznoUNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime Min(DateTime left, DateTime right) => left <= right ? left : right;
    private static DateTime Max(DateTime left, DateTime right) => left >= right ? left : right;
}
