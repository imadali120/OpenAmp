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
        await DodajAkoNedostajeAsync(
            "admin",
            "Admin",
            "OpenAmp",
            "admin@openamp.local",
            "ADMIN",
            sada,
            cancellationToken);
        await DodajAkoNedostajeAsync(
            "zaposlenik",
            "Studio",
            "Zaposlenik",
            "zaposlenik@openamp.local",
            "ZAPOSLENIK",
            sada,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DodajAkoNedostajeAsync(
        string username,
        string ime,
        string prezime,
        string email,
        string ulogaKod,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Korisnici.AnyAsync(
            x => x.Username == username || x.Email == email,
            cancellationToken))
        {
            return;
        }
        var uloga = await dbContext.Uloge.SingleAsync(x => x.Kod == ulogaKod, cancellationToken);
        dbContext.Korisnici.Add(new Korisnik
        {
            Username = username,
            Ime = ime,
            Prezime = prezime,
            Email = email,
            PasswordHash = passwordHasher.Hash("OpenAmp1!"),
            Aktivan = true,
            KreiranUtc = sada,
            UlogaId = uloga.Id
        });
    }
}
