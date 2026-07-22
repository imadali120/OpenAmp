using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Api.Models;
using OpenAmp.Application.Common;
using OpenAmp.Application.Mobile;
using OpenAmp.Application.Reservations;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reservations")]
public sealed class ReservationsController(
    ICommandHandler<KreirajRezervacijuCommand, RezervacijaDto> createHandler,
    ICommandHandler<IzmijeniRezervacijuCommand, RezervacijaDto> updateHandler,
    ICommandHandler<OtkaziRezervacijuCommand, OtkazivanjeRezultatDto> cancelHandler,
    IQueryHandler<DohvatiRezervacijuQuery, RezervacijaDto> getHandler,
    IQueryHandler<DohvatiMojeRezervacijeQuery, IReadOnlyCollection<MobileRezervacijaDto>> historyHandler,
    IQueryHandler<DohvatiSlobodneTermineQuery, IReadOnlyCollection<SlobodanTerminDto>> availabilityHandler)
    : ControllerBase
{
    [HttpGet("mine")]
    public Task<IReadOnlyCollection<MobileRezervacijaDto>> Mine(CancellationToken cancellationToken) =>
        historyHandler.HandleAsync(new DohvatiMojeRezervacijeQuery(User.KorisnikId()), cancellationToken);

    [AllowAnonymous]
    [HttpGet("availability")]
    public Task<IReadOnlyCollection<SlobodanTerminDto>> Availability(
        [FromQuery] int salaId,
        [FromQuery] DateOnly date,
        [FromQuery] int durationMinutes = 60,
        [FromQuery] int stepMinutes = 30,
        CancellationToken cancellationToken = default) =>
        availabilityHandler.HandleAsync(
            new DohvatiSlobodneTermineQuery(salaId, date, durationMinutes, stepMinutes),
            cancellationToken);

    [HttpGet("{id:int}")]
    public Task<RezervacijaDto> Get(int id, CancellationToken cancellationToken) =>
        getHandler.HandleAsync(new DohvatiRezervacijuQuery(id, User.KorisnikId()), cancellationToken);

    [HttpPost]
    [ProducesResponseType<RezervacijaDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<RezervacijaDto>> Create(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createHandler.HandleAsync(
            new KreirajRezervacijuCommand(
                User.KorisnikId(),
                request.SalaId,
                request.BendId,
                request.TerminOdUtc,
                request.TerminDoUtc,
                request.Napomena,
                request.Stavke?.Select(x => new NovaStavkaRezervacije(x.OpremaId, x.ArtikalId, x.Kolicina)).ToArray()
                    ?? []),
            cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    public Task<RezervacijaDto> Update(
        int id,
        UpdateReservationRequest request,
        CancellationToken cancellationToken) =>
        updateHandler.HandleAsync(
            new IzmijeniRezervacijuCommand(
                id, User.KorisnikId(), request.TerminOdUtc, request.TerminDoUtc, request.RowVersion),
            cancellationToken);

    [HttpPost("{id:int}/cancel")]
    public Task<OtkazivanjeRezultatDto> Cancel(
        int id,
        CancelReservationRequest request,
        CancellationToken cancellationToken) =>
        cancelHandler.HandleAsync(
            new OtkaziRezervacijuCommand(id, User.KorisnikId(), request.RowVersion, request.Razlog),
            cancellationToken);
}
