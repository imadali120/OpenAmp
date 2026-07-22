namespace OpenAmp.Domain.Rules;

public static class OtkazivanjePravila
{
    public static decimal IzracunajPovrat(
        decimal ukupnaCijena,
        DateTime terminOdUtc,
        DateTime sadaUtc,
        int puniPovratDoSati,
        int djelimicniPovratDoSati,
        int djelimicniPovratPostotak)
    {
        if (ukupnaCijena < 0
            || puniPovratDoSati < djelimicniPovratDoSati
            || djelimicniPovratDoSati < 0
            || djelimicniPovratPostotak is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(ukupnaCijena), "Politika povrata nije validna.");
        }

        var sati = (terminOdUtc - sadaUtc).TotalHours;
        var postotak = sati >= puniPovratDoSati
            ? 100
            : sati >= djelimicniPovratDoSati
                ? djelimicniPovratPostotak
                : 0;
        return decimal.Round(ukupnaCijena * postotak / 100m, 2, MidpointRounding.AwayFromZero);
    }
}
