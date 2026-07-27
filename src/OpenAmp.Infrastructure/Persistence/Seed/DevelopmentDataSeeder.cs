using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Auth;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Infrastructure.Persistence.Seed;

public sealed class DevelopmentDataSeeder(
    OpenAmpDbContext dbContext,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        var admin = await DodajIliAzurirajAsync(
            "admin",
            "Admin",
            "OpenAmp",
            "admin@openamp.local",
            "ADMIN",
            sada,
            cancellationToken);
        _ = admin;
        var zaposlenik = await DodajIliAzurirajAsync(
            "zaposlenik",
            "Studio",
            "Zaposlenik",
            "zaposlenik@openamp.local",
            "ZAPOSLENIK",
            sada,
            cancellationToken);
        _ = zaposlenik;
        var muzicar = await DodajIliAzurirajAsync(
            "muzicar",
            "Demo",
            "Muzičar",
            "muzicar@openamp.local",
            "MUZICAR",
            sada,
            cancellationToken);
        var jazz = await DodajIliAzurirajAsync(
            "jazz",
            "Jasmin",
            "Jazz",
            "jazz@openamp.local",
            "MUZICAR",
            sada,
            cancellationToken);
        var metal = await DodajIliAzurirajAsync(
            "metal",
            "Milan",
            "Metal",
            "metal@openamp.local",
            "MUZICAR",
            sada,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await DodajDemoPodatkeAsync(muzicar, jazz, metal, sada, cancellationToken);
    }

    private async Task<Korisnik> DodajIliAzurirajAsync(
        string username,
        string ime,
        string prezime,
        string email,
        string ulogaKod,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var korisnik = await dbContext.Korisnici.SingleOrDefaultAsync(
            x => x.Username == username || x.Email == email,
            cancellationToken);
        if (korisnik is not null)
        {
            korisnik.PasswordHash = passwordHasher.Hash("test");
            korisnik.Aktivan = true;
            return korisnik;
        }
        var uloga = await dbContext.Uloge.SingleAsync(x => x.Kod == ulogaKod, cancellationToken);
        korisnik = new Korisnik
        {
            Username = username,
            Ime = ime,
            Prezime = prezime,
            Email = email,
            PasswordHash = passwordHasher.Hash("test"),
            Aktivan = true,
            KreiranUtc = sada,
            UlogaId = uloga.Id
        };
        dbContext.Korisnici.Add(korisnik);
        return korisnik;
    }

    private async Task DodajDemoPodatkeAsync(
        Korisnik muzicar,
        Korisnik jazz,
        Korisnik metal,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var gitara = await dbContext.Instrumenti.SingleAsync(
            x => x.Kod == "GITARA",
            cancellationToken);
        var klavijature = await dbContext.Instrumenti.SingleAsync(
            x => x.Kod == "KLAVIJATURE",
            cancellationToken);
        var bubnjevi = await dbContext.Instrumenti.SingleAsync(
            x => x.Kod == "BUBNJEVI",
            cancellationToken);
        await DodajInstrumentAsync(muzicar.Id, gitara.Id, cancellationToken);
        await DodajInstrumentAsync(jazz.Id, klavijature.Id, cancellationToken);
        await DodajInstrumentAsync(metal.Id, bubnjevi.Id, cancellationToken);
        await DodajPostavkeAsync(muzicar.Id, sada, cancellationToken);
        await DodajPostavkeAsync(jazz.Id, sada, cancellationToken);
        await DodajPostavkeAsync(metal.Id, sada, cancellationToken);

        var rockId = await dbContext.Zanrovi
            .Where(x => x.Kod == "ROCK")
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);
        var jazzId = await dbContext.Zanrovi
            .Where(x => x.Kod == "JAZZ")
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);
        var metalId = await dbContext.Zanrovi
            .Where(x => x.Kod == "METAL")
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);
        var demoRock = await DodajBendAsync(
            "Demo Rock Bend",
            muzicar,
            rockId,
            gitara.Id,
            sada,
            cancellationToken);
        var demoJazz = await DodajBendAsync(
            "Blue Notes",
            jazz,
            jazzId,
            klavijature.Id,
            sada,
            cancellationToken);
        var demoMetal = await DodajBendAsync(
            "Metal Forge",
            metal,
            metalId,
            bubnjevi.Id,
            sada,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (await dbContext.Rezervacije.AnyAsync(
            x => x.Napomena == "OPENAMP_DEMO_SEED",
            cancellationToken))
        {
            return;
        }

        var izvrsenaId = await dbContext.StatusiRezervacija
            .Where(x => x.Kod == "IZVRSENA")
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);
        var placenaId = await dbContext.StatusiRezervacija
            .Where(x => x.Kod == "PLACENA")
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);
        var demoReservations = new[]
        {
            Rezervacija(demoRock, muzicar, 1, sada.AddDays(-28), 60, izvrsenaId, 30m),
            Rezervacija(demoRock, muzicar, 2, sada.AddDays(-20), 120, izvrsenaId, 48m),
            Rezervacija(demoJazz, jazz, 1, sada.AddDays(-18), 60, izvrsenaId, 30m),
            Rezervacija(demoJazz, jazz, 2, sada.AddDays(-12), 60, izvrsenaId, 24m),
            Rezervacija(demoMetal, metal, 1, sada.AddDays(-10), 120, izvrsenaId, 60m),
            Rezervacija(demoMetal, metal, 2, sada.AddDays(-6), 60, izvrsenaId, 24m),
            Rezervacija(demoJazz, jazz, 1, DanasnjiTermin(sada), 90, placenaId, 45m),
            Rezervacija(demoRock, muzicar, 3, SljedeciTermin(sada), 120, placenaId, 70m)
        };
        dbContext.Rezervacije.AddRange(demoReservations);
        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.Recenzije.AddRange(
            Recenzija(demoReservations[0], muzicar.Id, 1, 5, sada.AddDays(-27)),
            Recenzija(demoReservations[1], muzicar.Id, 2, 4, sada.AddDays(-19)),
            Recenzija(demoReservations[2], jazz.Id, 1, 4, sada.AddDays(-17)),
            Recenzija(demoReservations[3], jazz.Id, 2, 5, sada.AddDays(-11)),
            Recenzija(demoReservations[4], metal.Id, 1, 5, sada.AddDays(-9)),
            Recenzija(demoReservations[5], metal.Id, 2, 3, sada.AddDays(-5)));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DodajInstrumentAsync(
        int korisnikId,
        int instrumentId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.KorisnikInstrumenti.AnyAsync(
            x => x.KorisnikId == korisnikId && x.InstrumentId == instrumentId,
            cancellationToken))
        {
            dbContext.KorisnikInstrumenti.Add(new KorisnikInstrument
            {
                KorisnikId = korisnikId,
                InstrumentId = instrumentId,
                Primarni = true
            });
        }
    }

    private async Task DodajPostavkeAsync(
        int korisnikId,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.PostavkeKorisnika.AnyAsync(
            x => x.KorisnikId == korisnikId,
            cancellationToken))
        {
            dbContext.PostavkeKorisnika.Add(new PostavkeKorisnika
            {
                KorisnikId = korisnikId,
                PushNotifikacije = true,
                EmailNotifikacije = true,
                Jezik = "bs",
                ProfilJavan = true,
                AzuriraneUtc = sada
            });
        }
    }

    private async Task<Bend> DodajBendAsync(
        string naziv,
        Korisnik osnivac,
        int zanrId,
        int instrumentId,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(
            x => x.Naziv == naziv,
            cancellationToken);
        if (bend is not null)
        {
            return bend;
        }
        bend = new Bend
        {
            Naziv = naziv,
            Opis = "Demonstracijski bend za testiranje rezervacija i preporuka.",
            KreiranUtc = sada.AddMonths(-3),
            OsnivacId = osnivac.Id,
            ZanrId = zanrId
        };
        dbContext.Bendovi.Add(bend);
        dbContext.ClanoviBenda.Add(new ClanBenda
        {
            Bend = bend,
            KorisnikId = osnivac.Id,
            InstrumentId = instrumentId,
            UlogaUBendu = "Osnivač",
            DatumPridruzivanjaUtc = sada.AddMonths(-3),
            Aktivan = true
        });
        return bend;
    }

    private static Rezervacija Rezervacija(
        Bend bend,
        Korisnik korisnik,
        int salaId,
        DateTime termin,
        int minute,
        int statusId,
        decimal cijena) => new()
        {
            SalaId = salaId,
            BendId = bend.Id,
            KreiraoKorisnikId = korisnik.Id,
            TerminOdUtc = termin,
            TerminDoUtc = termin.AddMinutes(minute),
            UkupnaCijena = cijena,
            StatusRezervacijeId = statusId,
            Napomena = "OPENAMP_DEMO_SEED",
            KreiranaUtc = termin.AddDays(-3),
            AzuriranaUtc = termin.AddDays(-2)
        };

    private static Recenzija Recenzija(
        Rezervacija rezervacija,
        int korisnikId,
        int salaId,
        int ocjena,
        DateTime kreirana) => new()
        {
            RezervacijaId = rezervacija.Id,
            KorisnikId = korisnikId,
            SalaId = salaId,
            Ocjena = ocjena,
            Komentar = "Demo recenzija za evaluaciju aplikacije.",
            KreiranaUtc = kreirana,
            Vidljiva = true
        };

    private static DateTime SljedeciTermin(DateTime sada)
    {
        var termin = sada.Date.AddDays(2).AddHours(18);
        return DateTime.SpecifyKind(termin, DateTimeKind.Utc);
    }

    private static DateTime DanasnjiTermin(DateTime sada)
    {
        var termin = sada.Date.AddHours(18);
        return DateTime.SpecifyKind(termin, DateTimeKind.Utc);
    }
}
