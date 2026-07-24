using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Application.Desktop;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN,ZAPOSLENIK")]
[Route("api/desktop")]
public sealed class DesktopController(IDesktopAdminService service) : ControllerBase
{
    [HttpGet("lookups")]
    public Task<DesktopLookupsDto> Lookups(CancellationToken cancellationToken) =>
        service.DohvatiSifarnikeAsync(cancellationToken);

    [HttpGet("dashboard")]
    public Task<DesktopDashboardDto> Dashboard(
        [FromQuery] int? studioId,
        CancellationToken cancellationToken) =>
        service.DohvatiDashboardAsync(studioId, cancellationToken);

    [HttpGet("halls")]
    public Task<IReadOnlyCollection<DesktopSalaDto>> Halls(
        [FromQuery] string? search,
        [FromQuery] int? statusId,
        [FromQuery] int? minimumCapacity,
        CancellationToken cancellationToken) =>
        service.DohvatiSaleAsync(search, statusId, minimumCapacity, cancellationToken);

    [HttpPost("halls")]
    public async Task<ActionResult<DesktopSalaDto>> CreateHall(
        SacuvajSaluDto request,
        CancellationToken cancellationToken)
    {
        var result = await service.SacuvajSaluAsync(null, request, cancellationToken);
        return CreatedAtAction(nameof(Halls), new { id = result.Id }, result);
    }

    [HttpPut("halls/{id:int}")]
    public Task<DesktopSalaDto> UpdateHall(
        int id,
        SacuvajSaluDto request,
        CancellationToken cancellationToken) =>
        service.SacuvajSaluAsync(id, request, cancellationToken);

    [HttpDelete("halls/{id:int}")]
    public async Task<IActionResult> DeleteHall(int id, CancellationToken cancellationToken)
    {
        await service.ObrisiSaluAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("equipment")]
    public Task<IReadOnlyCollection<DesktopOpremaDto>> Equipment(
        [FromQuery] int? categoryId,
        [FromQuery] int? statusId,
        [FromQuery] int? hallId,
        CancellationToken cancellationToken) =>
        service.DohvatiOpremuAsync(categoryId, statusId, hallId, cancellationToken);

    [HttpPost("equipment")]
    public async Task<ActionResult<DesktopOpremaDto>> CreateEquipment(
        SacuvajOpremuDto request,
        CancellationToken cancellationToken)
    {
        var result = await service.SacuvajOpremuAsync(null, request, cancellationToken);
        return CreatedAtAction(nameof(Equipment), new { id = result.Id }, result);
    }

    [HttpPut("equipment/{id:int}")]
    public Task<DesktopOpremaDto> UpdateEquipment(
        int id,
        SacuvajOpremuDto request,
        CancellationToken cancellationToken) =>
        service.SacuvajOpremuAsync(id, request, cancellationToken);

    [HttpPost("equipment/{id:int}/services")]
    public Task<DesktopOpremaDto> ReportService(
        int id,
        PrijaviServisDto request,
        CancellationToken cancellationToken) =>
        service.PrijaviServisAsync(id, User.KorisnikId(), request, cancellationToken);

    [HttpPut("equipment/{id:int}/services/{serviceId:int}")]
    public Task<DesktopOpremaDto> CompleteService(
        int id,
        int serviceId,
        ZavrsiServisDto request,
        CancellationToken cancellationToken) =>
        service.ZavrsiServisAsync(id, serviceId, request, cancellationToken);

    [HttpGet("articles")]
    public Task<IReadOnlyCollection<DesktopArtikalDto>> Articles(
        [FromQuery] int? studioId,
        [FromQuery] bool lowStockOnly,
        CancellationToken cancellationToken) =>
        service.DohvatiArtikleAsync(studioId, lowStockOnly, cancellationToken);

    [HttpPost("articles")]
    public async Task<ActionResult<DesktopArtikalDto>> CreateArticle(
        SacuvajArtikalDto request,
        CancellationToken cancellationToken)
    {
        var result = await service.SacuvajArtikalAsync(null, request, cancellationToken);
        return CreatedAtAction(nameof(Articles), new { id = result.Id }, result);
    }

    [HttpPut("articles/{id:int}")]
    public Task<DesktopArtikalDto> UpdateArticle(
        int id,
        SacuvajArtikalDto request,
        CancellationToken cancellationToken) =>
        service.SacuvajArtikalAsync(id, request, cancellationToken);

    [HttpGet("reservations")]
    public Task<IReadOnlyCollection<DesktopRezervacijaDto>> Reservations(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int? hallId,
        CancellationToken cancellationToken) =>
        service.DohvatiRezervacijeAsync(fromUtc, toUtc, hallId, cancellationToken);

    [HttpPost("reservations")]
    public async Task<ActionResult<DesktopRezervacijaDto>> CreateReservation(
        SacuvajDesktopRezervacijuDto request,
        CancellationToken cancellationToken)
    {
        var result = await service.KreirajRezervacijuAsync(User.KorisnikId(), request, cancellationToken);
        return CreatedAtAction(nameof(Reservations), new { id = result.Id }, result);
    }

    [HttpPut("reservations/{id:int}")]
    public Task<DesktopRezervacijaDto> UpdateReservation(
        int id,
        IzmijeniDesktopRezervacijuDto request,
        CancellationToken cancellationToken) =>
        service.IzmijeniRezervacijuAsync(id, request, cancellationToken);

    [HttpGet("bands")]
    public Task<IReadOnlyCollection<DesktopBendDto>> Bands(
        [FromQuery] string? search,
        [FromQuery] int? genreId,
        CancellationToken cancellationToken) =>
        service.DohvatiBendoveAsync(search, genreId, cancellationToken);

    [HttpPut("bands/{id:int}")]
    public Task<DesktopBendDto> UpdateBand(
        int id,
        IzmijeniDesktopBendDto request,
        CancellationToken cancellationToken) =>
        service.IzmijeniBendAsync(id, request, cancellationToken);

    [HttpGet("users")]
    public Task<IReadOnlyCollection<DesktopKorisnikDto>> Users(
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        service.DohvatiKorisnikeAsync(search, cancellationToken);

    [Authorize(Roles = "ADMIN")]
    [HttpPut("users/{id:int}")]
    public Task<DesktopKorisnikDto> UpdateUser(
        int id,
        IzmijeniDesktopKorisnikDto request,
        CancellationToken cancellationToken) =>
        service.IzmijeniKorisnikaAsync(id, request, cancellationToken);
}
