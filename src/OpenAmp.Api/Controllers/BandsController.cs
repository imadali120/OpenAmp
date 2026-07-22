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
    IQueryHandler<DohvatiPrimljenePozivniceQuery, IReadOnlyCollection<PrimljenaPozivnicaBendaDto>> receivedHandler,
    ICommandHandler<KreirajBendCommand, BendDto> createHandler,
    ICommandHandler<PosaljiPozivnicuBendaCommand, PozivnicaBendaDto> inviteHandler,
    ICommandHandler<OdgovoriNaPozivnicuBendaCommand, PrimljenaPozivnicaBendaDto> respondHandler,
    ICommandHandler<AzurirajBendCommand, BendDto> updateHandler,
    ICommandHandler<AzurirajClanaBendaCommand, BendDto> updateMemberHandler,
    ICommandHandler<UkloniClanaBendaCommand, BendDto> removeMemberHandler) : ControllerBase
{
    [HttpGet("mine")]
    public Task<IReadOnlyCollection<BendDto>> Mine(CancellationToken cancellationToken) =>
        listHandler.HandleAsync(new DohvatiMojeBendoveQuery(User.KorisnikId()), cancellationToken);

    [HttpGet("invitations/received")]
    public Task<IReadOnlyCollection<PrimljenaPozivnicaBendaDto>> ReceivedInvitations(
        CancellationToken cancellationToken) =>
        receivedHandler.HandleAsync(new DohvatiPrimljenePozivniceQuery(User.KorisnikId()), cancellationToken);

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

    [HttpPost("invitations/{invitationId:int}/respond")]
    public Task<PrimljenaPozivnicaBendaDto> Respond(
        int invitationId,
        RespondBandInvitationRequest request,
        CancellationToken cancellationToken) =>
        respondHandler.HandleAsync(
            new OdgovoriNaPozivnicuBendaCommand(
                User.KorisnikId(), invitationId, request.Prihvati, request.InstrumentId),
            cancellationToken);

    [HttpPut("{id:int}")]
    public Task<BendDto> Update(
        int id,
        UpdateBandRequest request,
        CancellationToken cancellationToken) =>
        updateHandler.HandleAsync(
            new AzurirajBendCommand(User.KorisnikId(), id, request.Naziv, request.ZanrId, request.Opis),
            cancellationToken);

    [HttpPut("{id:int}/members/{memberUserId:int}")]
    public Task<BendDto> UpdateMember(
        int id,
        int memberUserId,
        UpdateBandMemberRequest request,
        CancellationToken cancellationToken) =>
        updateMemberHandler.HandleAsync(
            new AzurirajClanaBendaCommand(
                User.KorisnikId(), id, memberUserId, request.InstrumentId, request.Uloga),
            cancellationToken);

    [HttpDelete("{id:int}/members/{memberUserId:int}")]
    public Task<BendDto> RemoveMember(
        int id,
        int memberUserId,
        CancellationToken cancellationToken) =>
        removeMemberHandler.HandleAsync(
            new UkloniClanaBendaCommand(User.KorisnikId(), id, memberUserId),
            cancellationToken);
}
