using OpenAmp.Application.Common;

namespace OpenAmp.Application.Reservations;

public sealed record NovaStavkaRezervacije(int? OpremaId, int? ArtikalId, int Kolicina);

public sealed record StavkaRezervacijeDto(
    int Id,
    int? OpremaId,
    int? ArtikalId,
    string Naziv,
    int Kolicina,
    decimal JedinicnaCijena,
    decimal BrojSati,
    decimal UkupnaCijena);

public sealed record RezervacijaDto(
    int Id,
    int SalaId,
    string Sala,
    int BendId,
    string Bend,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    decimal UkupnaCijena,
    string Status,
    string? Napomena,
    string RowVersion,
    IReadOnlyCollection<StavkaRezervacijeDto> Stavke);

public sealed record SlobodanTerminDto(DateTime TerminOdUtc, DateTime TerminDoUtc);

public sealed record OtkazivanjeRezultatDto(
    RezervacijaDto Rezervacija,
    decimal RefundiraniIznos,
    string? StripeRefundId);

public sealed record KreirajRezervacijuCommand(
    int KorisnikId,
    int SalaId,
    int BendId,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    string? Napomena,
    IReadOnlyCollection<NovaStavkaRezervacije> Stavke) : ICommand<RezervacijaDto>;

public sealed record IzmijeniRezervacijuCommand(
    int RezervacijaId,
    int KorisnikId,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    string RowVersion) : ICommand<RezervacijaDto>;

public sealed record OtkaziRezervacijuCommand(
    int RezervacijaId,
    int KorisnikId,
    string RowVersion,
    string? Razlog) : ICommand<OtkazivanjeRezultatDto>;

public sealed record DohvatiRezervacijuQuery(int RezervacijaId, int KorisnikId)
    : IQuery<RezervacijaDto>;

public sealed record DohvatiSlobodneTermineQuery(
    int SalaId,
    DateOnly Datum,
    int TrajanjeMinuta,
    int KorakMinuta = 30) : IQuery<IReadOnlyCollection<SlobodanTerminDto>>;

public interface IRezervacijaService
{
    Task<RezervacijaDto> KreirajAsync(
        KreirajRezervacijuCommand zahtjev,
        CancellationToken cancellationToken = default);

    Task<RezervacijaDto> PromijeniTerminAsync(
        IzmijeniRezervacijuCommand zahtjev,
        CancellationToken cancellationToken = default);

    Task<OtkazivanjeRezultatDto> OtkaziAsync(
        OtkaziRezervacijuCommand zahtjev,
        CancellationToken cancellationToken = default);

    Task<RezervacijaDto> DohvatiAsync(
        int rezervacijaId,
        int korisnikId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SlobodanTerminDto>> DohvatiSlobodneTermineAsync(
        DohvatiSlobodneTermineQuery upit,
        CancellationToken cancellationToken = default);
}

public sealed class TerminNijeDostupanException(string message) : InvalidOperationException(message);
public sealed class KonfliktKonkurentnostiException(string message, Exception? innerException = null)
    : InvalidOperationException(message, innerException);
public sealed class EntitetNijePronadjenException(string message) : InvalidOperationException(message);
public sealed class NedozvoljenaOperacijaException(string message) : InvalidOperationException(message);
