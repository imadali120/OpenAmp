using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Application.Desktop;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/desktop/reference-data")]
public sealed class ReferenceDataController(IReferenceDataService service) : ControllerBase
{
    [HttpGet("types")]
    public IReadOnlyCollection<string> Types() => service.PodrzaniTipovi;

    [HttpGet("{type}")]
    public Task<IReadOnlyCollection<DesktopSifarnikDto>> Get(
        string type,
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        service.DohvatiAsync(type, search, cancellationToken);

    [HttpPost("{type}")]
    public Task<DesktopSifarnikDto> Create(
        string type,
        SacuvajSifarnikDto request,
        CancellationToken cancellationToken) =>
        service.SacuvajAsync(type, null, request, cancellationToken);

    [HttpPut("{type}/{id:int}")]
    public Task<DesktopSifarnikDto> Update(
        string type,
        int id,
        SacuvajSifarnikDto request,
        CancellationToken cancellationToken) =>
        service.SacuvajAsync(type, id, request, cancellationToken);

    [HttpDelete("{type}/{id:int}")]
    public async Task<IActionResult> Delete(
        string type,
        int id,
        CancellationToken cancellationToken)
    {
        await service.ObrisiAsync(type, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("studios/all")]
    public Task<IReadOnlyCollection<DesktopStudioDto>> Studios(
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        service.DohvatiStudijeAsync(search, cancellationToken);

    [HttpPost("studios/all")]
    public Task<DesktopStudioDto> CreateStudio(
        SacuvajStudioDto request,
        CancellationToken cancellationToken) =>
        service.SacuvajStudioAsync(null, request, cancellationToken);

    [HttpPut("studios/all/{id:int}")]
    public Task<DesktopStudioDto> UpdateStudio(
        int id,
        SacuvajStudioDto request,
        CancellationToken cancellationToken) =>
        service.SacuvajStudioAsync(id, request, cancellationToken);

    [HttpDelete("studios/all/{id:int}")]
    public async Task<IActionResult> DeleteStudio(
        int id,
        CancellationToken cancellationToken)
    {
        await service.ObrisiStudioAsync(id, cancellationToken);
        return NoContent();
    }
}
