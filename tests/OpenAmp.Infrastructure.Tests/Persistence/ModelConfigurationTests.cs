using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Tests.Persistence;

public sealed class ModelConfigurationTests
{
    [Fact]
    public void RezervacijaRowVersionJeConcurrencyToken()
    {
        using var context = KreirajSqlServerContext();
        var entity = context.Model.FindEntityType(typeof(Rezervacija));
        var property = entity?.FindProperty(nameof(Rezervacija.RowVersion));

        Assert.NotNull(property);
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }

    [Fact]
    public void RezervacijaImaIndeksZaProvjeruPreklapanjaTermina()
    {
        using var context = KreirajSqlServerContext();
        var entity = context.Model.FindEntityType(typeof(Rezervacija));
        var index = entity?.GetIndexes().SingleOrDefault(x =>
            x.Properties.Select(p => p.Name).SequenceEqual(
            [nameof(Rezervacija.SalaId), nameof(Rezervacija.TerminOdUtc), nameof(Rezervacija.TerminDoUtc)]));

        Assert.NotNull(index);
        Assert.Equal("IX_Rezervacije_Sala_Termin", index.GetDatabaseName());
    }

    [Fact]
    public void ClanBendaImaKompozitniPrimarniKljuc()
    {
        using var context = KreirajSqlServerContext();
        var key = context.Model.FindEntityType(typeof(ClanBenda))?.FindPrimaryKey();

        Assert.Equal(
            [nameof(ClanBenda.BendId), nameof(ClanBenda.KorisnikId)],
            key?.Properties.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void RefreshTokenHashJeJedinstven()
    {
        using var context = KreirajSqlServerContext();
        var entity = context.Model.FindEntityType(typeof(RefreshToken));
        var index = entity?.GetIndexes().SingleOrDefault(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(RefreshToken.TokenHash)]));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void StripeWebhookDogadjajKoristiStripeIdKaoPrimarniKljuc()
    {
        using var context = KreirajSqlServerContext();
        var key = context.Model.FindEntityType(typeof(StripeWebhookDogadjaj))?.FindPrimaryKey();

        Assert.Equal([nameof(StripeWebhookDogadjaj.Id)], key?.Properties.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void OmiljenaSalaImaKompozitniPrimarniKljuc()
    {
        using var context = KreirajSqlServerContext();
        var key = context.Model.FindEntityType(typeof(OmiljenaSala))?.FindPrimaryKey();

        Assert.Equal(
            [nameof(OmiljenaSala.KorisnikId), nameof(OmiljenaSala.SalaId)],
            key?.Properties.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void PostavkeKorisnikaSuJedanNaJedanSaKorisnikom()
    {
        using var context = KreirajSqlServerContext();
        var entity = context.Model.FindEntityType(typeof(PostavkeKorisnika));
        var foreignKey = entity?.GetForeignKeys().Single();

        Assert.True(foreignKey?.IsUnique);
        Assert.Equal(nameof(PostavkeKorisnika.KorisnikId), foreignKey?.Properties.Single().Name);
    }

    [Fact]
    public void StripeCustomerIdJeJedinstvenKadaPostoji()
    {
        using var context = KreirajSqlServerContext();
        var entity = context.Model.FindEntityType(typeof(Korisnik));
        var index = entity?.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(Korisnik.StripeCustomerId)]));

        Assert.True(index?.IsUnique);
        Assert.Equal("[StripeCustomerId] IS NOT NULL", index?.GetFilter());
    }

    [Fact]
    public void UsernameJeObavezanIJedinstven()
    {
        using var context = KreirajSqlServerContext();
        var entity = context.Model.FindEntityType(typeof(Korisnik));
        var property = entity?.FindProperty(nameof(Korisnik.Username));
        var index = entity?.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(Korisnik.Username)]));

        Assert.False(property?.IsNullable);
        Assert.Equal(30, property?.GetMaxLength());
        Assert.True(index?.IsUnique);
    }

    [Fact]
    public void MedijskeDatotekeSeCuvajuOdvojenoOdProfila()
    {
        using var context = KreirajSqlServerContext();
        var media = context.Model.FindEntityType(typeof(MedijskaDatoteka));
        var user = context.Model.FindEntityType(typeof(Korisnik));

        Assert.Equal("MedijskeDatoteke", media?.GetTableName());
        var content = media?.FindProperty(nameof(MedijskaDatoteka.Sadrzaj));
        Assert.Equal(typeof(byte[]), content?.ClrType);
        Assert.False(content?.IsNullable);
        Assert.NotNull(user?.FindProperty(nameof(Korisnik.ProfilnaSlikaId)));
    }

    private static OpenAmpDbContext KreirajSqlServerContext()
    {
        var options = new DbContextOptionsBuilder<OpenAmpDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=OpenAmpModelTests;Trusted_Connection=True")
            .Options;

        return new OpenAmpDbContext(options);
    }
}
