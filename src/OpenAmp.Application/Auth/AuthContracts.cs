using OpenAmp.Application.Common;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Application.Auth;

public sealed record KorisnikDto(
    int Id,
    string Username,
    string Ime,
    string Prezime,
    string Email,
    string? Telefon,
    string Uloga,
    string? FotografijaUrl,
    IReadOnlyCollection<int> InstrumentIds);

public sealed record AuthResponseDto(
    string AccessToken,
    DateTime AccessTokenIsticeUtc,
    string RefreshToken,
    DateTime RefreshTokenIsticeUtc,
    KorisnikDto Korisnik);

public sealed record RegisterCommand(
    string Username,
    string Ime,
    string Prezime,
    string Email,
    string Password,
    string? Telefon,
    string? IpAdresa) : ICommand<AuthResponseDto>;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAdresa) : ICommand<AuthResponseDto>;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAdresa) : ICommand<AuthResponseDto>;

public sealed record DohvatiKorisnikaQuery(int KorisnikId) : IQuery<KorisnikDto>;

public sealed record AzurirajProfilCommand(
    int KorisnikId,
    string Username,
    string Ime,
    string Prezime,
    string? Telefon,
    string? FotografijaUrl,
    IReadOnlyCollection<int> InstrumentIds) : ICommand<KorisnikDto>;

public sealed record PromijeniLozinkuCommand(
    int KorisnikId,
    string TrenutnaLozinka,
    string NovaLozinka) : ICommand<bool>;

public sealed record AccessTokenResult(string Token, DateTime IsticeUtc);

public sealed record GenerisaniRefreshToken(string Vrijednost, string Hash, DateTime IsticeUtc);

public interface IKorisnikRepository
{
    Task<bool> EmailPostojiAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernamePostojiAsync(
        string username,
        int? osimKorisnikaId = null,
        CancellationToken cancellationToken = default);
    Task<Korisnik?> DohvatiPoEmailuIliUsernameuAsync(
        string identifikator,
        CancellationToken cancellationToken = default);
    Task<Korisnik?> DohvatiPoIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Korisnik?> DohvatiPoRefreshTokenHashuAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<Uloga> DohvatiUloguMuzicaraAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Instrument>> DohvatiInstrumenteAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);
    Task DodajAsync(Korisnik korisnik, CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface IAccessTokenService
{
    AccessTokenResult Kreiraj(Korisnik korisnik);
}

public interface IRefreshTokenService
{
    GenerisaniRefreshToken Kreiraj(DateTime sadaUtc);
    string Hash(string token);
}

public sealed class NeispravniPodaciZaPrijavuException()
    : InvalidOperationException("Email/username ili lozinka nisu ispravni.");

public sealed class EmailJeZauzetException()
    : InvalidOperationException("Korisnik sa navedenim emailom već postoji.");

public sealed class UsernameJeZauzetException()
    : InvalidOperationException("Navedeni username je zauzet.");

public sealed class NeispravanRefreshTokenException()
    : InvalidOperationException("Refresh token nije važeći.");

public sealed class KorisnikNijePronadjenException()
    : InvalidOperationException("Korisnik nije pronađen.");
