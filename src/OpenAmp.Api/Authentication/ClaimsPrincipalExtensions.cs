using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenAmp.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static int KorisnikId(this ClaimsPrincipal principal)
    {
        var vrijednost = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(vrijednost, out var id)
            ? id
            : throw new UnauthorizedAccessException("JWT ne sadrži validan korisnički identifikator.");
    }
}
