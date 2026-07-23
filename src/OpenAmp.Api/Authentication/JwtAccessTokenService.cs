using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenAmp.Application.Auth;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Api.Authentication;

public sealed class JwtAccessTokenService(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : IAccessTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessTokenResult Kreiraj(Korisnik korisnik)
    {
        var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
        var isticeUtc = sadaUtc.AddMinutes(_options.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, korisnik.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Email, korisnik.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, korisnik.Username),
            new Claim(JwtRegisteredClaimNames.GivenName, korisnik.Ime),
            new Claim(JwtRegisteredClaimNames.FamilyName, korisnik.Prezime),
            new Claim(ClaimTypes.Role, korisnik.Uloga.Kod),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            sadaUtc,
            isticeUtc,
            credentials);
        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), isticeUtc);
    }
}
