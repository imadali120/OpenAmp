namespace OpenAmp.Domain.Rules;

public static class RezervacijaPravila
{
    public static void ProvjeriTermin(DateTime terminOdUtc, DateTime terminDoUtc)
    {
        if (terminOdUtc.Kind != DateTimeKind.Utc || terminDoUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Termini rezervacije moraju biti zadani u UTC vremenu.");
        }

        if (terminDoUtc <= terminOdUtc)
        {
            throw new ArgumentException("Kraj termina mora biti nakon početka termina.");
        }
    }

    public static bool PreklapajuSe(
        DateTime prviOdUtc,
        DateTime prviDoUtc,
        DateTime drugiOdUtc,
        DateTime drugiDoUtc) =>
        prviOdUtc < drugiDoUtc && drugiOdUtc < prviDoUtc;
}
