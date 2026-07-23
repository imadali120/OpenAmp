namespace OpenAmp.Application.Media;

public sealed record NovaSlikaDto(
    string NazivDatoteke,
    string ContentType,
    byte[] Sadrzaj);

public sealed record MedijskaDatotekaDto(
    int Id,
    string NazivDatoteke,
    string ContentType,
    byte[] Sadrzaj,
    DateTime KreiranaUtc);

public sealed record UploadSlikeDto(int Id, string Url);

public interface IMediaService
{
    Task<MedijskaDatotekaDto?> DohvatiAsync(int id, CancellationToken cancellationToken = default);
    Task<UploadSlikeDto> PostaviProfilnuSlikuAsync(
        int korisnikId,
        NovaSlikaDto slika,
        CancellationToken cancellationToken = default);
    Task<UploadSlikeDto> PostaviSlikuBendaAsync(
        int korisnikId,
        int bendId,
        NovaSlikaDto slika,
        CancellationToken cancellationToken = default);
    Task<UploadSlikeDto> PostaviSlikuStudijaAsync(
        int korisnikId,
        int studioId,
        NovaSlikaDto slika,
        CancellationToken cancellationToken = default);
    Task<UploadSlikeDto> DodajSlikuSaleAsync(
        int korisnikId,
        int salaId,
        NovaSlikaDto slika,
        string? alternativniTekst,
        CancellationToken cancellationToken = default);
}
