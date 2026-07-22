using OpenAmp.Domain.Rules;

namespace OpenAmp.Infrastructure.Tests.Domain;

public sealed class RezervacijaPravilaTests
{
    [Theory]
    [InlineData("2026-08-01T10:00:00Z", "2026-08-01T12:00:00Z", "2026-08-01T11:00:00Z", "2026-08-01T13:00:00Z", true)]
    [InlineData("2026-08-01T10:00:00Z", "2026-08-01T12:00:00Z", "2026-08-01T12:00:00Z", "2026-08-01T14:00:00Z", false)]
    [InlineData("2026-08-01T10:00:00Z", "2026-08-01T12:00:00Z", "2026-08-01T08:00:00Z", "2026-08-01T10:00:00Z", false)]
    public void PreklapajuSeVracaOcekivaniRezultat(
        string prviOd,
        string prviDo,
        string drugiOd,
        string drugiDo,
        bool ocekivano)
    {
        var rezultat = RezervacijaPravila.PreklapajuSe(
            DateTime.Parse(prviOd, System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime(),
            DateTime.Parse(prviDo, System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime(),
            DateTime.Parse(drugiOd, System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime(),
            DateTime.Parse(drugiDo, System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime());

        Assert.Equal(ocekivano, rezultat);
    }

    [Fact]
    public void ProvjeriTerminBacaGreskuKadaKrajNijeNakonPocetka()
    {
        var termin = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() => RezervacijaPravila.ProvjeriTermin(termin, termin));
    }

    [Fact]
    public void ProvjeriTerminBacaGreskuZaLokalnoVrijeme()
    {
        var pocetak = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Local);
        var kraj = pocetak.AddHours(2);

        Assert.Throws<ArgumentException>(() => RezervacijaPravila.ProvjeriTermin(pocetak, kraj));
    }
}
