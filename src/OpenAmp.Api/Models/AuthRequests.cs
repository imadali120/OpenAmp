using System.ComponentModel.DataAnnotations;

namespace OpenAmp.Api.Models;

public sealed record RegisterRequest(
    [param: Required, StringLength(100, MinimumLength = 2)] string Ime,
    [param: Required, StringLength(100, MinimumLength = 2)] string Prezime,
    [param: Required, EmailAddress, StringLength(320)] string Email,
    [param: Required, StringLength(128, MinimumLength = 10)] string Password,
    [param: Phone, StringLength(30)] string? Telefon);

public sealed record LoginRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password);

public sealed record RefreshTokenRequest(
    [param: Required] string RefreshToken);

public sealed record UpdateProfileRequest(
    [param: Required, StringLength(100, MinimumLength = 2)] string Ime,
    [param: Required, StringLength(100, MinimumLength = 2)] string Prezime,
    [param: Phone, StringLength(30)] string? Telefon,
    [param: Url, StringLength(2048)] string? FotografijaUrl,
    IReadOnlyCollection<int>? InstrumentIds);

public sealed record ChangePasswordRequest(
    [param: Required] string TrenutnaLozinka,
    [param: Required, StringLength(128, MinimumLength = 10)] string NovaLozinka);
