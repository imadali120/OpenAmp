using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Api.Models;
using OpenAmp.Application.Auth;
using OpenAmp.Application.Common;
using OpenAmp.Application.Mobile;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(
    IQueryHandler<DohvatiKorisnikaQuery, KorisnikDto> getHandler,
    IQueryHandler<DohvatiProfilPregledQuery, ProfilPregledDto> overviewHandler,
    IQueryHandler<DohvatiOmiljeneSaleQuery, IReadOnlyCollection<int>> favoritesHandler,
    IQueryHandler<DohvatiKorisnickePostavkeQuery, KorisnickePostavkeDto> settingsHandler,
    ICommandHandler<AzurirajProfilCommand, KorisnikDto> updateHandler,
    ICommandHandler<PromijeniLozinkuCommand, bool> changePasswordHandler,
    ICommandHandler<PostaviOmiljenuSaluCommand, bool> setFavoriteHandler,
    ICommandHandler<AzurirajKorisnickePostavkeCommand, KorisnickePostavkeDto> updateSettingsHandler) : ControllerBase
{
    [HttpGet("me/overview")]
    public Task<ProfilPregledDto> Overview(CancellationToken cancellationToken) =>
        overviewHandler.HandleAsync(new DohvatiProfilPregledQuery(User.KorisnikId()), cancellationToken);

    [HttpGet("me")]
    public Task<KorisnikDto> Me(CancellationToken cancellationToken) =>
        getHandler.HandleAsync(new DohvatiKorisnikaQuery(User.KorisnikId()), cancellationToken);

    [HttpPut("me")]
    public Task<KorisnikDto> UpdateMe(UpdateProfileRequest request, CancellationToken cancellationToken) =>
        updateHandler.HandleAsync(
            new AzurirajProfilCommand(
                User.KorisnikId(),
                request.Ime,
                request.Prezime,
                request.Telefon,
                request.FotografijaUrl,
                request.InstrumentIds?.ToArray() ?? []),
            cancellationToken);

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await changePasswordHandler.HandleAsync(
            new PromijeniLozinkuCommand(User.KorisnikId(), request.TrenutnaLozinka, request.NovaLozinka),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("me/favorite-halls")]
    public Task<IReadOnlyCollection<int>> FavoriteHalls(CancellationToken cancellationToken) =>
        favoritesHandler.HandleAsync(new DohvatiOmiljeneSaleQuery(User.KorisnikId()), cancellationToken);

    [HttpPut("me/favorite-halls/{hallId:int}")]
    public Task<bool> SaveFavoriteHall(int hallId, CancellationToken cancellationToken) =>
        setFavoriteHandler.HandleAsync(
            new PostaviOmiljenuSaluCommand(User.KorisnikId(), hallId, true), cancellationToken);

    [HttpDelete("me/favorite-halls/{hallId:int}")]
    public Task<bool> RemoveFavoriteHall(int hallId, CancellationToken cancellationToken) =>
        setFavoriteHandler.HandleAsync(
            new PostaviOmiljenuSaluCommand(User.KorisnikId(), hallId, false), cancellationToken);

    [HttpGet("me/settings")]
    public Task<KorisnickePostavkeDto> Settings(CancellationToken cancellationToken) =>
        settingsHandler.HandleAsync(new DohvatiKorisnickePostavkeQuery(User.KorisnikId()), cancellationToken);

    [HttpPut("me/settings")]
    public Task<KorisnickePostavkeDto> UpdateSettings(
        UpdateUserSettingsRequest request,
        CancellationToken cancellationToken) =>
        updateSettingsHandler.HandleAsync(
            new AzurirajKorisnickePostavkeCommand(
                User.KorisnikId(),
                request.PushNotifikacije,
                request.EmailNotifikacije,
                request.Jezik,
                request.ProfilJavan),
            cancellationToken);
}
