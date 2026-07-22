using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Api.Models;
using OpenAmp.Application.Common;
using OpenAmp.Application.Mobile;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bands")]
public sealed class BandsController(
    IQueryHandler<DohvatiMojeBendoveQuery, IReadOnlyCollection<BendDto>> listHandler,
    ICommandHandler<KreirajBendCommand, BendDto> createHandler,
    ICommandHandler<PosaljiPozivnicuBendaCommand, PozivnicaBendaDto> inviteHandler) : ControllerBase
{
    [HttpGet("mine")]
    public Task<IReadOnlyCollection<BendDto>> Mine(CancellationToken cancellationToken) =>
        listHandler.HandleAsync(new DohvatiMojeBendoveQuery(User.KorisnikId()), cancellationToken);

    [HttpPost]
    [ProducesResponseType<BendDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<BendDto>> Create(
        CreateBandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.HandleAsync(
            new KreirajBendCommand(User.KorisnikId(), request.Naziv, request.ZanrId, request.Opis),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("{id:int}/invitations")]
    [ProducesResponseType<PozivnicaBendaDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<PozivnicaBendaDto>> Invite(
        int id,
        InviteBandMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await inviteHandler.HandleAsync(
            new PosaljiPozivnicuBendaCommand(User.KorisnikId(), id, request.Email),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
