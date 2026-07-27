using OpenAmp.Application.Mobile;

namespace OpenAmp.Application.Recommendations;

public sealed record SalaPreporukaDto(
    SalaCardDto Sala,
    double Score,
    double ContentScore,
    double CollaborativeScore,
    double Alpha,
    string Razlog);

public sealed record RecommendationFilter(
    int KorisnikId,
    int BendId,
    int Limit,
    DateTime? TerminOdUtc,
    DateTime? TerminDoUtc,
    string? ZanrKod,
    int? MinimalniKapacitet,
    string? KategorijaOpremeKod);

public interface IRecommendationService
{
    Task<IReadOnlyCollection<SalaPreporukaDto>> PreporuciSaleAsync(
        RecommendationFilter filter,
        CancellationToken cancellationToken = default);
}
