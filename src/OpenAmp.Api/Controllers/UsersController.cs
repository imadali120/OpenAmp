using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Api.Models;
using OpenAmp.Application.Auth;
using OpenAmp.Application.Common;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(
    IQueryHandler<DohvatiKorisnikaQuery, KorisnikDto> getHandler,
    ICommandHandler<AzurirajProfilCommand, KorisnikDto> updateHandler) : ControllerBase
{
    [HttpGet("me")]
    public Task<KorisnikDto> Me(CancellationToken cancellationToken) =>
        getHandler.HandleAsync(new DohvatiKorisnikaQuery(User.KorisnikId()), cancellationToken);

    [HttpPut("me")]
    public Task<KorisnikDto> UpdateMe(UpdateProfileRequest request, CancellationToken cancellationToken) =>
        updateHandler.HandleAsync(
            new AzurirajProfilCommand(User.KorisnikId(), request.Ime, request.Prezime, request.Telefon),
            cancellationToken);
}
