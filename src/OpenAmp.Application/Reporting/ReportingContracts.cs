namespace OpenAmp.Application.Reporting;

public sealed record PrihodPoSaliDto(
    int SalaId,
    string Sala,
    string Studio,
    decimal Prihod,
    int BrojRezervacija,
    decimal Postotak);

public sealed record RezervacijePoZanruDto(
    int ZanrId,
    string Zanr,
    int BrojRezervacija,
    decimal Postotak);

public sealed record PoslovniIzvjestajDto(
    DateTime PeriodOdUtc,
    DateTime PeriodDoUtc,
    int? SalaId,
    int? ZanrId,
    decimal UkupanPrihod,
    int UkupnoRezervacija,
    decimal ProsjecnaVrijednostRezervacije,
    decimal UkupnoSati,
    IReadOnlyCollection<PrihodPoSaliDto> PrihodPoSalama,
    IReadOnlyCollection<RezervacijePoZanruDto> RezervacijePoZanrovima);

public interface IReportingService
{
    Task<PoslovniIzvjestajDto> GenerisiAsync(
        DateTime periodOdUtc,
        DateTime periodDoUtc,
        int? salaId,
        int? zanrId,
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerisiPdfAsync(
        DateTime periodOdUtc,
        DateTime periodDoUtc,
        int? salaId,
        int? zanrId,
        CancellationToken cancellationToken = default);
}
