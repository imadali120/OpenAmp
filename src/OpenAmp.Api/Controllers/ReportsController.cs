using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Application.Reporting;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN,ZAPOSLENIK")]
[Route("api/desktop/reports")]
public sealed class ReportsController(IReportingService service) : ControllerBase
{
    [HttpGet]
    public Task<PoslovniIzvjestajDto> Get(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int? hallId,
        [FromQuery] int? genreId,
        CancellationToken cancellationToken) =>
        service.GenerisiAsync(fromUtc, toUtc, hallId, genreId, cancellationToken);

    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int? hallId,
        [FromQuery] int? genreId,
        CancellationToken cancellationToken)
    {
        var content = await service.GenerisiPdfAsync(
            fromUtc,
            toUtc,
            hallId,
            genreId,
            cancellationToken);
        return File(
            content,
            "application/pdf",
            $"OpenAmp-izvjestaj-{fromUtc:yyyyMMdd}-{toUtc.AddTicks(-1):yyyyMMdd}.pdf");
    }
}
