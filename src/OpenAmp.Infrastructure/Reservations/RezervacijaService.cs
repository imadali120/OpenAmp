using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Payments;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Domain.Rules;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Reservations;

public sealed class RezervacijaService(
    OpenAmpDbContext dbContext,
    IStripeGateway stripeGateway,
    TimeProvider timeProvider) : IRezervacijaService
{
    private static readonly string[] AktivniStatusi = ["NA_CEKANJU", "PLACENA"];

    public async Task<RezervacijaDto> KreirajAsync(
        KreirajRezervacijuCommand zahtjev,
        CancellationToken cancellationToken = default)
    {
        RezervacijaPravila.ProvjeriTermin(zahtjev.TerminOdUtc, zahtjev.TerminDoUtc);
        ProvjeriStavke(zahtjev.Stavke);
        if (zahtjev.TerminOdUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new ArgumentException("Nije moguće rezervisati termin u prošlosti.");
        }

        try
        {
            await using var transakcija = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var sala = await dbContext.Sale
                .Include(x => x.Status)
                .Include(x => x.Studio)
                .SingleOrDefaultAsync(x => x.Id == zahtjev.SalaId, cancellationToken)
                ?? throw new EntitetNijePronadjenException($"Sala {zahtjev.SalaId} nije pronađena.");
            if (sala.Status.Kod != "AKTIVNA" || !sala.Studio.Aktivan)
            {
                throw new NedozvoljenaOperacijaException("Odabrana sala trenutno nije aktivna.");
            }

            OsigurajUnutarRadnogVremena(sala.Studio, zahtjev.TerminOdUtc, zahtjev.TerminDoUtc);

            var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Id == zahtjev.BendId, cancellationToken)
                ?? throw new EntitetNijePronadjenException($"Bend {zahtjev.BendId} nije pronađen.");
            var pripadaBendu = bend.OsnivacId == zahtjev.KorisnikId
                || await dbContext.ClanoviBenda.AnyAsync(
                    x => x.BendId == zahtjev.BendId && x.KorisnikId == zahtjev.KorisnikId && x.Aktivan,
                    cancellationToken);
            if (!pripadaBendu)
            {
                throw new NedozvoljenaOperacijaException("Rezervaciju može kreirati samo član odabranog benda.");
            }

            await OsigurajSlobodanTerminAsync(
                zahtjev.SalaId,
                zahtjev.TerminOdUtc,
                zahtjev.TerminDoUtc,
                null,
                cancellationToken);

            var status = await dbContext.StatusiRezervacija
                .SingleAsync(x => x.Kod == "NA_CEKANJU", cancellationToken);
            var trajanjeSati = TrajanjeUSatima(zahtjev.TerminOdUtc, zahtjev.TerminDoUtc);
            var stavke = await KreirajStavkeAsync(zahtjev, trajanjeSati, cancellationToken);
            var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
            var rezervacija = new Rezervacija
            {
                SalaId = zahtjev.SalaId,
                Sala = sala,
                BendId = zahtjev.BendId,
                Bend = bend,
                KreiraoKorisnikId = zahtjev.KorisnikId,
                TerminOdUtc = zahtjev.TerminOdUtc,
                TerminDoUtc = zahtjev.TerminDoUtc,
                StatusRezervacijeId = status.Id,
                Status = status,
                Napomena = zahtjev.Napomena,
                KreiranaUtc = sadaUtc,
                AzuriranaUtc = sadaUtc,
                Stavke = stavke,
                UkupnaCijena = decimal.Round(
                    (sala.CijenaPoSatu * trajanjeSati) + stavke.Sum(x => x.UkupnaCijena),
                    2,
                    MidpointRounding.AwayFromZero)
            };

            dbContext.Rezervacije.Add(rezervacija);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transakcija.CommitAsync(cancellationToken);
            return Mapiraj(rezervacija);
        }
        catch (Exception exception) when (JeSqlServerDeadlock(exception))
        {
            throw new KonfliktKonkurentnostiException(
                "Druga rezervacija je istovremeno zauzela isti termin. Osvježite dostupnost i pokušajte ponovo.",
                exception);
        }
    }

    public async Task<RezervacijaDto> PromijeniTerminAsync(
        IzmijeniRezervacijuCommand zahtjev,
        CancellationToken cancellationToken = default)
    {
        RezervacijaPravila.ProvjeriTermin(zahtjev.TerminOdUtc, zahtjev.TerminDoUtc);
        if (zahtjev.TerminOdUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new ArgumentException("Novi termin mora biti u budućnosti.");
        }

        var rowVersion = DekodirajRowVersion(zahtjev.RowVersion);
        try
        {
            await using var transakcija = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var rezervacija = await UpitRezervacije()
                .SingleOrDefaultAsync(x => x.Id == zahtjev.RezervacijaId, cancellationToken)
                ?? throw new EntitetNijePronadjenException($"Rezervacija {zahtjev.RezervacijaId} nije pronađena.");
            OsigurajVlasnistvo(rezervacija, zahtjev.KorisnikId);
            if (rezervacija.Status.Kod != "NA_CEKANJU")
            {
                throw new NedozvoljenaOperacijaException("Mijenjati se može samo rezervacija koja čeka plaćanje.");
            }


            OsigurajUnutarRadnogVremena(
                rezervacija.Sala.Studio,
                zahtjev.TerminOdUtc,
                zahtjev.TerminDoUtc);

            dbContext.Entry(rezervacija).Property(x => x.RowVersion).OriginalValue = rowVersion;
            await OsigurajSlobodanTerminAsync(
                rezervacija.SalaId,
                zahtjev.TerminOdUtc,
                zahtjev.TerminDoUtc,
                rezervacija.Id,
                cancellationToken);
            await OsigurajDostupnostOpremeAsync(
                rezervacija.Stavke.Where(x => x.OpremaId.HasValue).Select(x => x.OpremaId!.Value).ToArray(),
                zahtjev.TerminOdUtc,
                zahtjev.TerminDoUtc,
                rezervacija.Id,
                cancellationToken);

            var trajanjeSati = TrajanjeUSatima(zahtjev.TerminOdUtc, zahtjev.TerminDoUtc);
            foreach (var stavka in rezervacija.Stavke.Where(x => x.OpremaId.HasValue))
            {
                stavka.BrojSati = trajanjeSati;
                stavka.UkupnaCijena = decimal.Round(
                    stavka.JedinicnaCijena * stavka.Kolicina * trajanjeSati,
                    2,
                    MidpointRounding.AwayFromZero);
            }

            rezervacija.TerminOdUtc = zahtjev.TerminOdUtc;
            rezervacija.TerminDoUtc = zahtjev.TerminDoUtc;
            rezervacija.AzuriranaUtc = timeProvider.GetUtcNow().UtcDateTime;
            rezervacija.UkupnaCijena = decimal.Round(
                (rezervacija.Sala.CijenaPoSatu * trajanjeSati) + rezervacija.Stavke.Sum(x => x.UkupnaCijena),
                2,
                MidpointRounding.AwayFromZero);

            await SacuvajSaConcurrencyProvjeromAsync(transakcija, cancellationToken);
            return Mapiraj(rezervacija);
        }
        catch (Exception exception) when (JeSqlServerDeadlock(exception))
        {
            throw new KonfliktKonkurentnostiException(
                "Druga rezervacija je istovremeno promijenila zauzetost termina.",
                exception);
        }
    }

    public async Task<OtkazivanjeRezultatDto> OtkaziAsync(
        OtkaziRezervacijuCommand zahtjev,
        CancellationToken cancellationToken = default)
    {
        var rezervacija = await UpitRezervacije()
            .SingleOrDefaultAsync(x => x.Id == zahtjev.RezervacijaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException($"Rezervacija {zahtjev.RezervacijaId} nije pronađena.");
        OsigurajVlasnistvo(rezervacija, zahtjev.KorisnikId);
        if (!AktivniStatusi.Contains(rezervacija.Status.Kod))
        {
            throw new NedozvoljenaOperacijaException("Rezervaciju u trenutnom statusu nije moguće otkazati.");
        }

        var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (rezervacija.TerminOdUtc <= sadaUtc)
        {
            throw new NedozvoljenaOperacijaException("Nije moguće otkazati rezervaciju čiji je termin počeo.");
        }

        dbContext.Entry(rezervacija).Property(x => x.RowVersion).OriginalValue = DekodirajRowVersion(zahtjev.RowVersion);
        var refundIznos = IzracunajRefund(rezervacija, sadaUtc);
        string? refundId = null;
        if (rezervacija.StripePaymentIntentId is not null)
        {
            if (rezervacija.Status.Kod == "PLACENA" && refundIznos > 0)
            {
                var refund = await stripeGateway.RefundirajAsync(
                    rezervacija.StripePaymentIntentId,
                    refundIznos,
                    stripeGateway.Valuta,
                    $"openamp-rezervacija-{rezervacija.Id}-refund-v1",
                    cancellationToken);
                refundId = refund.Id;
                rezervacija.StripeRefundId = refund.Id;
                rezervacija.RefundiraniIznos = refund.Iznos;
                rezervacija.RefundiranUtc = sadaUtc;
            }
            else if (rezervacija.Status.Kod == "NA_CEKANJU")
            {
                await stripeGateway.OtkaziPaymentIntentAsync(rezervacija.StripePaymentIntentId, cancellationToken);
            }
        }

        foreach (var stavka in rezervacija.Stavke.Where(x => x.ArtikalId.HasValue && x.Artikal is not null))
        {
            stavka.Artikal!.KolicinaNaStanju += stavka.Kolicina;
        }

        rezervacija.Status = await dbContext.StatusiRezervacija
            .SingleAsync(x => x.Kod == "OTKAZANA", cancellationToken);
        rezervacija.StatusRezervacijeId = rezervacija.Status.Id;
        rezervacija.OtkazanaUtc = sadaUtc;
        rezervacija.RazlogOtkazivanja = zahtjev.Razlog?.Trim();
        rezervacija.AzuriranaUtc = sadaUtc;
        await SacuvajSaConcurrencyProvjeromAsync(null, cancellationToken);
        return new OtkazivanjeRezultatDto(Mapiraj(rezervacija), refundIznos, refundId);
    }

    public async Task<RezervacijaDto> DohvatiAsync(
        int rezervacijaId,
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        var rezervacija = await UpitRezervacije()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == rezervacijaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException($"Rezervacija {rezervacijaId} nije pronađena.");
        await OsigurajPristupAsync(rezervacija, korisnikId, cancellationToken);
        return Mapiraj(rezervacija);
    }

    public async Task<IReadOnlyCollection<SlobodanTerminDto>> DohvatiSlobodneTermineAsync(
        DohvatiSlobodneTermineQuery upit,
        CancellationToken cancellationToken = default)
    {
        if (upit.TrajanjeMinuta is < 30 or > 720 || upit.KorakMinuta is < 15 or > 120)
        {
            throw new ArgumentException("Trajanje ili korak termina nisu u dozvoljenom rasponu.");
        }

        var sala = await dbContext.Sale.AsNoTracking()
            .Include(x => x.Studio)
            .Include(x => x.Status)
            .SingleOrDefaultAsync(x => x.Id == upit.SalaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException($"Sala {upit.SalaId} nije pronađena.");
        if (sala.Status.Kod != "AKTIVNA")
        {
            return [];
        }

        var zona = TimeZoneInfo.FindSystemTimeZoneById(sala.Studio.VremenskaZona);
        var lokalniOd = upit.Datum.ToDateTime(sala.Studio.RadnoVrijemeOd, DateTimeKind.Unspecified);
        var lokalniDo = upit.Datum.ToDateTime(sala.Studio.RadnoVrijemeDo, DateTimeKind.Unspecified);
        var danOdUtc = TimeZoneInfo.ConvertTimeToUtc(lokalniOd, zona);
        var danDoUtc = TimeZoneInfo.ConvertTimeToUtc(lokalniDo, zona);
        var zauzeti = await dbContext.Rezervacije.AsNoTracking()
            .Where(x => x.SalaId == upit.SalaId
                        && AktivniStatusi.Contains(x.Status.Kod)
                        && x.TerminOdUtc < danDoUtc
                        && danOdUtc < x.TerminDoUtc)
            .Select(x => new { x.TerminOdUtc, x.TerminDoUtc })
            .ToArrayAsync(cancellationToken);

        var rezultat = new List<SlobodanTerminDto>();
        var trajanje = TimeSpan.FromMinutes(upit.TrajanjeMinuta);
        var korak = TimeSpan.FromMinutes(upit.KorakMinuta);
        var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
        for (var pocetak = danOdUtc; pocetak.Add(trajanje) <= danDoUtc; pocetak = pocetak.Add(korak))
        {
            var kraj = pocetak.Add(trajanje);
            if (pocetak > sadaUtc && !zauzeti.Any(x => pocetak < x.TerminDoUtc && x.TerminOdUtc < kraj))
            {
                rezultat.Add(new SlobodanTerminDto(pocetak, kraj));
            }
        }

        return rezultat;
    }

    public async Task<OtkazivanjePregledDto> DohvatiOtkazivanjePregledAsync(
        int rezervacijaId,
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        var rezervacija = await UpitRezervacije().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == rezervacijaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException($"Rezervacija {rezervacijaId} nije pronađena.");
        OsigurajVlasnistvo(rezervacija, korisnikId);
        var studio = rezervacija.Sala.Studio;
        var povrat = AktivniStatusi.Contains(rezervacija.Status.Kod)
            ? IzracunajRefund(rezervacija, timeProvider.GetUtcNow().UtcDateTime)
            : 0;
        return new OtkazivanjePregledDto(
            povrat,
            studio.PuniPovratDoSati,
            studio.DjelimicniPovratDoSati,
            studio.DjelimicniPovratPostotak);
    }

    private IQueryable<Rezervacija> UpitRezervacije() => dbContext.Rezervacije
        .Include(x => x.Sala).ThenInclude(x => x.Studio)
        .Include(x => x.Bend)
        .Include(x => x.Status)
        .Include(x => x.Stavke).ThenInclude(x => x.Artikal);

    private async Task OsigurajSlobodanTerminAsync(
        int salaId,
        DateTime terminOdUtc,
        DateTime terminDoUtc,
        int? izuzmiRezervacijuId,
        CancellationToken cancellationToken)
    {
        var preklapanje = await dbContext.Rezervacije.AnyAsync(
            x => x.SalaId == salaId
                 && (!izuzmiRezervacijuId.HasValue || x.Id != izuzmiRezervacijuId.Value)
                 && AktivniStatusi.Contains(x.Status.Kod)
                 && x.TerminOdUtc < terminDoUtc
                 && terminOdUtc < x.TerminDoUtc,
            cancellationToken);
        if (preklapanje)
        {
            throw new TerminNijeDostupanException("Odabrana sala je već rezervisana u dijelu traženog termina.");
        }
    }

    private async Task<List<StavkaRezervacije>> KreirajStavkeAsync(
        KreirajRezervacijuCommand zahtjev,
        decimal trajanjeSati,
        CancellationToken cancellationToken)
    {
        var opremaIds = zahtjev.Stavke.Where(x => x.OpremaId.HasValue).Select(x => x.OpremaId!.Value).ToArray();
        var artikalIds = zahtjev.Stavke.Where(x => x.ArtikalId.HasValue).Select(x => x.ArtikalId!.Value).ToArray();
        var oprema = await dbContext.Oprema.Include(x => x.Status)
            .Where(x => opremaIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        var artikli = await dbContext.Artikli.Include(x => x.Status)
            .Where(x => artikalIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        if (oprema.Count != opremaIds.Length || artikli.Count != artikalIds.Length)
        {
            throw new EntitetNijePronadjenException("Jedna ili više odabranih stavki ne postoje.");
        }

        await OsigurajDostupnostOpremeAsync(
            opremaIds,
            zahtjev.TerminOdUtc,
            zahtjev.TerminDoUtc,
            null,
            cancellationToken);
        var rezultat = new List<StavkaRezervacije>();
        foreach (var trazena in zahtjev.Stavke)
        {
            if (trazena.OpremaId is int opremaId)
            {
                var entitet = oprema[opremaId];
                if (entitet.Status.Kod != "DOSTUPNA")
                {
                    throw new TerminNijeDostupanException($"Oprema '{entitet.Naziv}' trenutno nije dostupna.");
                }

                rezultat.Add(new StavkaRezervacije
                {
                    OpremaId = entitet.Id,
                    Naziv = entitet.Naziv,
                    Kolicina = 1,
                    JedinicnaCijena = entitet.CijenaNajmaPoSatu,
                    BrojSati = trajanjeSati,
                    UkupnaCijena = decimal.Round(entitet.CijenaNajmaPoSatu * trajanjeSati, 2)
                });
            }
            else if (trazena.ArtikalId is int artikalId)
            {
                var entitet = artikli[artikalId];
                if (entitet.Status.Kod != "AKTIVAN" || entitet.KolicinaNaStanju < trazena.Kolicina)
                {
                    throw new TerminNijeDostupanException($"Artikal '{entitet.Naziv}' nema dovoljnu zalihu.");
                }

                entitet.KolicinaNaStanju -= trazena.Kolicina;
                rezultat.Add(new StavkaRezervacije
                {
                    ArtikalId = entitet.Id,
                    Artikal = entitet,
                    Naziv = entitet.Naziv,
                    Kolicina = trazena.Kolicina,
                    JedinicnaCijena = entitet.CijenaKupovine,
                    BrojSati = 0,
                    UkupnaCijena = decimal.Round(entitet.CijenaKupovine * trazena.Kolicina, 2)
                });
            }
        }

        return rezultat;
    }

    private async Task OsigurajDostupnostOpremeAsync(
        int[] opremaIds,
        DateTime terminOdUtc,
        DateTime terminDoUtc,
        int? izuzmiRezervacijuId,
        CancellationToken cancellationToken)
    {
        if (opremaIds.Length == 0)
        {
            return;
        }

        var zauzeta = await dbContext.StavkeRezervacija
            .Where(x => x.OpremaId.HasValue
                        && opremaIds.Contains(x.OpremaId.Value)
                        && (!izuzmiRezervacijuId.HasValue || x.RezervacijaId != izuzmiRezervacijuId.Value)
                        && AktivniStatusi.Contains(x.Rezervacija.Status.Kod)
                        && x.Rezervacija.TerminOdUtc < terminDoUtc
                        && terminOdUtc < x.Rezervacija.TerminDoUtc)
            .Select(x => x.OpremaId!.Value).Distinct().ToArrayAsync(cancellationToken);
        if (zauzeta.Length > 0)
        {
            throw new TerminNijeDostupanException($"Oprema nije dostupna: {string.Join(", ", zauzeta)}.");
        }
    }

    private async Task SacuvajSaConcurrencyProvjeromAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transakcija,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            if (transakcija is not null)
            {
                await transakcija.CommitAsync(cancellationToken);
            }
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transakcija is not null)
            {
                await transakcija.RollbackAsync(cancellationToken);
            }

            throw new KonfliktKonkurentnostiException(
                "Rezervaciju je u međuvremenu izmijenio drugi korisnik.", exception);
        }
    }

    private static decimal IzracunajRefund(Rezervacija rezervacija, DateTime sadaUtc)
    {
        if (rezervacija.Status.Kod != "PLACENA")
        {
            return 0;
        }

        var studio = rezervacija.Sala.Studio;
        return OtkazivanjePravila.IzracunajPovrat(
            rezervacija.UkupnaCijena,
            rezervacija.TerminOdUtc,
            sadaUtc,
            studio.PuniPovratDoSati,
            studio.DjelimicniPovratDoSati,
            studio.DjelimicniPovratPostotak);
    }

    private static void ProvjeriStavke(IReadOnlyCollection<NovaStavkaRezervacije> stavke)
    {
        foreach (var stavka in stavke)
        {
            if (stavka.Kolicina <= 0 || stavka.OpremaId.HasValue == stavka.ArtikalId.HasValue)
            {
                throw new ArgumentException("Svaka stavka mora imati pozitivan iznos i tačno jedan tip.");
            }

            if (stavka.OpremaId.HasValue && stavka.Kolicina != 1)
            {
                throw new ArgumentException("Inventarski komad opreme može se dodati samo jednom.");
            }
        }

        if (stavke.Where(x => x.OpremaId.HasValue).GroupBy(x => x.OpremaId).Any(x => x.Count() > 1)
            || stavke.Where(x => x.ArtikalId.HasValue).GroupBy(x => x.ArtikalId).Any(x => x.Count() > 1))
        {
            throw new ArgumentException("Ista oprema ili artikal ne smiju biti navedeni više puta.");
        }
    }

    private static void OsigurajVlasnistvo(Rezervacija rezervacija, int korisnikId)
    {
        if (rezervacija.KreiraoKorisnikId != korisnikId)
        {
            throw new NedozvoljenaOperacijaException("Nemate pristup ovoj rezervaciji.");
        }
    }

    private async Task OsigurajPristupAsync(
        Rezervacija rezervacija,
        int korisnikId,
        CancellationToken cancellationToken)
    {
        if (rezervacija.KreiraoKorisnikId == korisnikId)
        {
            return;
        }

        var clanBenda = await dbContext.ClanoviBenda.AnyAsync(
            x => x.BendId == rezervacija.BendId && x.KorisnikId == korisnikId && x.Aktivan,
            cancellationToken);
        if (!clanBenda)
        {
            throw new NedozvoljenaOperacijaException("Nemate pristup ovoj rezervaciji.");
        }
    }

    private static void OsigurajUnutarRadnogVremena(
        Studio studio,
        DateTime terminOdUtc,
        DateTime terminDoUtc)
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(studio.VremenskaZona);
        var lokalniOd = TimeZoneInfo.ConvertTimeFromUtc(terminOdUtc, zona);
        var lokalniDo = TimeZoneInfo.ConvertTimeFromUtc(terminDoUtc, zona);
        if (DateOnly.FromDateTime(lokalniOd) != DateOnly.FromDateTime(lokalniDo)
            || TimeOnly.FromDateTime(lokalniOd) < studio.RadnoVrijemeOd
            || TimeOnly.FromDateTime(lokalniDo) > studio.RadnoVrijemeDo)
        {
            throw new NedozvoljenaOperacijaException("Termin mora biti unutar radnog vremena studija.");
        }
    }

    private static byte[] DekodirajRowVersion(string rowVersion)
    {
        try
        {
            var vrijednost = Convert.FromBase64String(rowVersion);
            return vrijednost.Length == 0 ? throw new FormatException() : vrijednost;
        }
        catch (FormatException)
        {
            throw new ArgumentException("RowVersion mora biti validan Base64 string.", nameof(rowVersion));
        }
    }

    private static RezervacijaDto Mapiraj(Rezervacija rezervacija) => new(
        rezervacija.Id,
        rezervacija.SalaId,
        rezervacija.Sala.Naziv,
        rezervacija.BendId,
        rezervacija.Bend.Naziv,
        rezervacija.TerminOdUtc,
        rezervacija.TerminDoUtc,
        rezervacija.UkupnaCijena,
        rezervacija.Status.Naziv,
        rezervacija.Status.Kod,
        rezervacija.Napomena,
        Convert.ToBase64String(rezervacija.RowVersion),
        rezervacija.Stavke.Select(x => new StavkaRezervacijeDto(
            x.Id, x.OpremaId, x.ArtikalId, x.Naziv, x.Kolicina,
            x.JedinicnaCijena, x.BrojSati, x.UkupnaCijena)).ToArray());

    private static decimal TrajanjeUSatima(DateTime odUtc, DateTime doUtc) =>
        decimal.Round((decimal)(doUtc - odUtc).TotalHours, 2, MidpointRounding.AwayFromZero);

    private static bool JeSqlServerDeadlock(Exception exception)
    {
        for (var trenutni = exception; trenutni is not null; trenutni = trenutni.InnerException)
        {
            if (trenutni is SqlException { Number: 1205 })
            {
                return true;
            }
        }

        return false;
    }
}
