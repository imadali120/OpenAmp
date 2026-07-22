using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Application.Common;
using OpenAmp.Application.Mobile;

namespace OpenAmp.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api")]
public sealed class CatalogController(
    IQueryHandler<PretraziSaleQuery, IReadOnlyCollection<SalaCardDto>> searchHandler,
    IQueryHandler<DohvatiSaluQuery, SalaDetaljiDto> detailsHandler,
    IQueryHandler<DohvatiMobileSifarnikeQuery, MobileLookupsDto> lookupsHandler) : ControllerBase
{
    [HttpGet("salas")]
    public Task<IReadOnlyCollection<SalaCardDto>> Search(
        [FromQuery] string? search,
        [FromQuery] string? genre,
        [FromQuery] int? capacity,
        [FromQuery] string? equipmentCategory,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken) =>
        searchHandler.HandleAsync(
            new PretraziSaleQuery(search, genre, capacity, equipmentCategory, fromUtc, toUtc),
            cancellationToken);

    [HttpGet("salas/{id:int}")]
    public Task<SalaDetaljiDto> Details(int id, CancellationToken cancellationToken) =>
        detailsHandler.HandleAsync(new DohvatiSaluQuery(id), cancellationToken);

    [HttpGet("mobile/lookups")]
    public Task<MobileLookupsDto> Lookups(CancellationToken cancellationToken) =>
        lookupsHandler.HandleAsync(new DohvatiMobileSifarnikeQuery(), cancellationToken);
}
