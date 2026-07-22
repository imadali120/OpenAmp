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

    private static OpenAmpDbContext KreirajSqlServerContext()
    {
        var options = new DbContextOptionsBuilder<OpenAmpDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=OpenAmpModelTests;Trusted_Connection=True")
            .Options;

        return new OpenAmpDbContext(options);
    }
}
