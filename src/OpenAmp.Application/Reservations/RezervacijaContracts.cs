namespace OpenAmp.Application.Reservations;

public sealed record NovaRezervacija(
    int SalaId,
    int BendId,
    int KreiraoKorisnikId,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    string? Napomena,
    IReadOnlyCollection<NovaStavkaRezervacije> Stavke);

public sealed record NovaStavkaRezervacije(
    int? OpremaId,
    int? ArtikalId,
    int Kolicina);

public interface IRezervacijaService
{
    Task<int> KreirajAsync(NovaRezervacija zahtjev, CancellationToken cancellationToken = default);

    Task PromijeniTerminAsync(
        int rezervacijaId,
        DateTime terminOdUtc,
        DateTime terminDoUtc,
        byte[] ocekivaniRowVersion,
        CancellationToken cancellationToken = default);
}

public sealed class TerminNijeDostupanException(string message) : InvalidOperationException(message);

public sealed class KonfliktKonkurentnostiException(string message, Exception? innerException = null)
    : InvalidOperationException(message, innerException);

public sealed class EntitetNijePronadjenException(string message) : InvalidOperationException(message);
