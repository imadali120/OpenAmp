namespace OpenAmp.Application.Desktop;

public sealed record SacuvajSifarnikDto(string Kod, string Naziv);

public sealed record DesktopStudioDto(
    int Id,
    string Naziv,
    string? Opis,
    string Adresa,
    string Grad,
    string? Telefon,
    string? Email,
    bool Aktivan,
    string VremenskaZona,
    TimeOnly RadnoVrijemeOd,
    TimeOnly RadnoVrijemeDo,
    int PuniPovratDoSati,
    int DjelimicniPovratDoSati,
    int DjelimicniPovratPostotak);

public sealed record SacuvajStudioDto(
    string Naziv,
    string? Opis,
    string Adresa,
    string Grad,
    string? Telefon,
    string? Email,
    bool Aktivan,
    string VremenskaZona,
    TimeOnly RadnoVrijemeOd,
    TimeOnly RadnoVrijemeDo,
    int PuniPovratDoSati,
    int DjelimicniPovratDoSati,
    int DjelimicniPovratPostotak);

public interface IReferenceDataService
{
    IReadOnlyCollection<string> PodrzaniTipovi { get; }

    Task<IReadOnlyCollection<DesktopSifarnikDto>> DohvatiAsync(
        string tip,
        string? tekst,
        CancellationToken cancellationToken = default);

    Task<DesktopSifarnikDto> SacuvajAsync(
        string tip,
        int? id,
        SacuvajSifarnikDto dto,
        CancellationToken cancellationToken = default);

    Task ObrisiAsync(
        string tip,
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DesktopStudioDto>> DohvatiStudijeAsync(
        string? tekst,
        CancellationToken cancellationToken = default);

    Task<DesktopStudioDto> SacuvajStudioAsync(
        int? id,
        SacuvajStudioDto dto,
        CancellationToken cancellationToken = default);

    Task ObrisiStudioAsync(
        int id,
        CancellationToken cancellationToken = default);
}
