using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Domain.Rules;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Reservations;

public sealed class RezervacijaService(OpenAmpDbContext dbContext) : IRezervacijaService
{
    private static readonly string[] AktivniStatusi = ["NA_CEKANJU", "PLACENA"];

    public async Task<int> KreirajAsync(
        NovaRezervacija zahtjev,
        CancellationToken cancellationToken = default)
    {
        RezervacijaPravila.ProvjeriTermin(zahtjev.TerminOdUtc, zahtjev.TerminDoUtc);
        ProvjeriStavke(zahtjev.Stavke);

        try
        {
            await using var transakcija = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var sala = await dbContext.Sale
                .SingleOrDefaultAsync(x => x.Id == zahtjev.SalaId, cancellationToken)
                ?? throw new EntitetNijePronadjenException($"Sala {zahtjev.SalaId} nije pronađena.");

            if (!await dbContext.Bendovi.AnyAsync(x => x.Id == zahtjev.BendId, cancellationToken))
            {
                throw new EntitetNijePronadjenException($"Bend {zahtjev.BendId} nije pronađen.");
            }

            if (!await dbContext.Korisnici.AnyAsync(x => x.Id == zahtjev.KreiraoKorisnikId, cancellationToken))
            {
                throw new EntitetNijePronadjenException($"Korisnik {zahtjev.KreiraoKorisnikId} nije pronađen.");
            }

            await OsigurajSlobodanTerminAsync(
                zahtjev.SalaId,
                zahtjev.TerminOdUtc,
                zahtjev.TerminDoUtc,
                null,
                cancellationToken);

            var statusNaCekanjuId = await dbContext.StatusiRezervacija
                .Where(x => x.Kod == "NA_CEKANJU")
                .Select(x => x.Id)
                .SingleAsync(cancellationToken);

            var trajanjeSati = TrajanjeUSatima(zahtjev.TerminOdUtc, zahtjev.TerminDoUtc);
            var stavke = await KreirajStavkeAsync(
                zahtjev,
                trajanjeSati,
                cancellationToken);
            var sadaUtc = DateTime.UtcNow;
            var rezervacija = new Rezervacija
            {
                SalaId = zahtjev.SalaId,
                BendId = zahtjev.BendId,
                KreiraoKorisnikId = zahtjev.KreiraoKorisnikId,
                TerminOdUtc = zahtjev.TerminOdUtc,
                TerminDoUtc = zahtjev.TerminDoUtc,
                StatusRezervacijeId = statusNaCekanjuId,
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

            return rezervacija.Id;
        }
        catch (Exception exception) when (JeSqlServerDeadlock(exception))
        {
            throw new KonfliktKonkurentnostiException(
                "Druga rezervacija je istovremeno zauzela isti termin. Osvježite dostupnost i pokušajte ponovo.",
                exception);
        }
    }

    public async Task PromijeniTerminAsync(
        int rezervacijaId,
        DateTime terminOdUtc,
        DateTime terminDoUtc,
        byte[] ocekivaniRowVersion,
        CancellationToken cancellationToken = default)
    {
        RezervacijaPravila.ProvjeriTermin(terminOdUtc, terminDoUtc);
        if (ocekivaniRowVersion.Length == 0)
        {
            throw new ArgumentException("RowVersion je obavezan.", nameof(ocekivaniRowVersion));
        }

        try
        {
            await using var transakcija = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var rezervacija = await dbContext.Rezervacije
                .Include(x => x.Sala)
                .Include(x => x.Stavke)
                .SingleOrDefaultAsync(x => x.Id == rezervacijaId, cancellationToken)
                ?? throw new EntitetNijePronadjenException($"Rezervacija {rezervacijaId} nije pronađena.");

            dbContext.Entry(rezervacija)
                .Property(x => x.RowVersion)
                .OriginalValue = ocekivaniRowVersion;

            await OsigurajSlobodanTerminAsync(
                rezervacija.SalaId,
                terminOdUtc,
                terminDoUtc,
                rezervacijaId,
                cancellationToken);

            await OsigurajDostupnostOpremeAsync(
                rezervacija.Stavke
                    .Where(x => x.OpremaId.HasValue)
                    .Select(x => x.OpremaId!.Value)
                    .ToArray(),
                terminOdUtc,
                terminDoUtc,
                rezervacijaId,
                cancellationToken);

            var trajanjeSati = TrajanjeUSatima(terminOdUtc, terminDoUtc);
            foreach (var stavka in rezervacija.Stavke.Where(x => x.OpremaId.HasValue))
            {
                stavka.BrojSati = trajanjeSati;
                stavka.UkupnaCijena = decimal.Round(
                    stavka.JedinicnaCijena * stavka.Kolicina * trajanjeSati,
                    2,
                    MidpointRounding.AwayFromZero);
            }

            rezervacija.TerminOdUtc = terminOdUtc;
            rezervacija.TerminDoUtc = terminDoUtc;
            rezervacija.AzuriranaUtc = DateTime.UtcNow;
            rezervacija.UkupnaCijena = decimal.Round(
                (rezervacija.Sala.CijenaPoSatu * trajanjeSati) + rezervacija.Stavke.Sum(x => x.UkupnaCijena),
                2,
                MidpointRounding.AwayFromZero);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await transakcija.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await transakcija.RollbackAsync(cancellationToken);
                throw new KonfliktKonkurentnostiException(
                    "Rezervaciju je u međuvremenu izmijenio drugi korisnik. Osvježite podatke i pokušajte ponovo.",
                    exception);
            }
        }
        catch (Exception exception) when (JeSqlServerDeadlock(exception))
        {
            throw new KonfliktKonkurentnostiException(
                "Druga rezervacija je istovremeno promijenila zauzetost termina. Osvježite podatke i pokušajte ponovo.",
                exception);
        }
    }

    private async Task OsigurajSlobodanTerminAsync(
        int salaId,
        DateTime terminOdUtc,
        DateTime terminDoUtc,
        int? izuzmiRezervacijuId,
        CancellationToken cancellationToken)
    {
        var preklapanjePostoji = await dbContext.Rezervacije.AnyAsync(
            x => x.SalaId == salaId
                 && (!izuzmiRezervacijuId.HasValue || x.Id != izuzmiRezervacijuId.Value)
                 && AktivniStatusi.Contains(x.Status.Kod)
                 && x.TerminOdUtc < terminDoUtc
                 && terminOdUtc < x.TerminDoUtc,
            cancellationToken);

        if (preklapanjePostoji)
        {
            throw new TerminNijeDostupanException("Odabrana sala je već rezervisana u dijelu traženog termina.");
        }
    }

    private async Task<List<StavkaRezervacije>> KreirajStavkeAsync(
        NovaRezervacija zahtjev,
        decimal trajanjeSati,
        CancellationToken cancellationToken)
    {
        var opremaIds = zahtjev.Stavke
            .Where(x => x.OpremaId.HasValue)
            .Select(x => x.OpremaId!.Value)
            .Distinct()
            .ToArray();
        var artikalIds = zahtjev.Stavke
            .Where(x => x.ArtikalId.HasValue)
            .Select(x => x.ArtikalId!.Value)
            .Distinct()
            .ToArray();

        var oprema = await dbContext.Oprema
            .Include(x => x.Status)
            .Where(x => opremaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var artikli = await dbContext.Artikli
            .Include(x => x.Status)
            .Where(x => artikalIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

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
        foreach (var trazenaStavka in zahtjev.Stavke)
        {
            if (trazenaStavka.OpremaId is int opremaId)
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
                    Kolicina = trazenaStavka.Kolicina,
                    JedinicnaCijena = entitet.CijenaNajmaPoSatu,
                    BrojSati = trajanjeSati,
                    UkupnaCijena = decimal.Round(
                        entitet.CijenaNajmaPoSatu * trazenaStavka.Kolicina * trajanjeSati,
                        2,
                        MidpointRounding.AwayFromZero)
                });
            }
            else if (trazenaStavka.ArtikalId is int artikalId)
            {
                var entitet = artikli[artikalId];
                if (entitet.Status.Kod != "AKTIVAN" || entitet.KolicinaNaStanju < trazenaStavka.Kolicina)
                {
                    throw new TerminNijeDostupanException($"Artikal '{entitet.Naziv}' nema dovoljnu dostupnu zalihu.");
                }

                rezultat.Add(new StavkaRezervacije
                {
                    ArtikalId = entitet.Id,
                    Naziv = entitet.Naziv,
                    Kolicina = trazenaStavka.Kolicina,
                    JedinicnaCijena = entitet.CijenaKupovine,
                    BrojSati = 0,
                    UkupnaCijena = decimal.Round(
                        entitet.CijenaKupovine * trazenaStavka.Kolicina,
                        2,
                        MidpointRounding.AwayFromZero)
                });
            }
        }

        return rezultat;
    }

    private static void ProvjeriStavke(IReadOnlyCollection<NovaStavkaRezervacije> stavke)
    {
        foreach (var stavka in stavke)
        {
            if (stavka.Kolicina <= 0)
            {
                throw new ArgumentException("Količina stavke mora biti veća od nule.");
            }

            if (stavka.OpremaId.HasValue == stavka.ArtikalId.HasValue)
            {
                throw new ArgumentException("Stavka mora sadržati ili opremu ili artikal, ali ne oboje.");
            }

            if (stavka.OpremaId.HasValue && stavka.Kolicina != 1)
            {
                throw new ArgumentException("Inventarski komad opreme može se dodati samo jednom.");
            }
        }
    }

    private static decimal TrajanjeUSatima(DateTime terminOdUtc, DateTime terminDoUtc) =>
        decimal.Round((decimal)(terminDoUtc - terminOdUtc).TotalHours, 2, MidpointRounding.AwayFromZero);

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

        var zauzetaOprema = await dbContext.StavkeRezervacija
            .Where(x => x.OpremaId.HasValue
                        && opremaIds.Contains(x.OpremaId.Value)
                        && (!izuzmiRezervacijuId.HasValue || x.RezervacijaId != izuzmiRezervacijuId.Value)
                        && AktivniStatusi.Contains(x.Rezervacija.Status.Kod)
                        && x.Rezervacija.TerminOdUtc < terminDoUtc
                        && terminOdUtc < x.Rezervacija.TerminDoUtc)
            .Select(x => x.OpremaId!.Value)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        if (zauzetaOprema.Length > 0)
        {
            throw new TerminNijeDostupanException(
                $"Oprema nije dostupna u traženom terminu: {string.Join(", ", zauzetaOprema)}.");
        }
    }

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
