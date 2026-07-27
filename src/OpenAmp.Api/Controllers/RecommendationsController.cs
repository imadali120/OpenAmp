using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Application.Recommendations;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recommendations")]
public sealed class RecommendationsController(IRecommendationService service) : ControllerBase
{
    [HttpGet("bands/{bandId:int}/halls")]
    public Task<IReadOnlyCollection<SalaPreporukaDto>> Halls(
        int bandId,
        [FromQuery] int limit = 5,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] string? genre = null,
        [FromQuery] int? minimumCapacity = null,
        [FromQuery] string? equipmentCategory = null,
        CancellationToken cancellationToken = default) =>
        service.PreporuciSaleAsync(
            new RecommendationFilter(
                User.KorisnikId(),
                bandId,
                limit,
                fromUtc,
                toUtc,
                genre,
                minimumCapacity,
                equipmentCategory),
            cancellationToken);
}
