using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Models;
using OpenAmp.Application.Auth;
using OpenAmp.Application.Common;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    ICommandHandler<RegisterCommand, AuthResponseDto> registerHandler,
    ICommandHandler<LoginCommand, AuthResponseDto> loginHandler,
    ICommandHandler<RefreshTokenCommand, AuthResponseDto> refreshHandler) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await registerHandler.HandleAsync(
            new RegisterCommand(
                request.Username,
                request.Ime,
                request.Prezime,
                request.Email,
                request.Password,
                request.Telefon,
                IpAdresa()),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public Task<AuthResponseDto> Login(LoginRequest request, CancellationToken cancellationToken) =>
        loginHandler.HandleAsync(new LoginCommand(request.Email, request.Password, IpAdresa()), cancellationToken);

    [AllowAnonymous]
    [HttpPost("refresh")]
    public Task<AuthResponseDto> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken) =>
        refreshHandler.HandleAsync(new RefreshTokenCommand(request.RefreshToken, IpAdresa()), cancellationToken);

    private string? IpAdresa() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
