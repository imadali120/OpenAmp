using OpenAmp.Domain.Rules;

namespace OpenAmp.Infrastructure.Tests.Domain;

public sealed class OtkazivanjePravilaTests
{
    private static readonly DateTime SadaUtc = new(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(30, 100)]
    [InlineData(24, 100)]
    [InlineData(18, 50)]
    [InlineData(12, 50)]
    [InlineData(11, 0)]
    public void IzracunajPovratPostujeGranicePolitike(int satiDoTermina, int ocekivaniPostotak)
    {
        var rezultat = OtkazivanjePravila.IzracunajPovrat(
            80m,
            SadaUtc.AddHours(satiDoTermina),
            SadaUtc,
            24,
            12,
            50);

        Assert.Equal(80m * ocekivaniPostotak / 100m, rezultat);
    }
}
