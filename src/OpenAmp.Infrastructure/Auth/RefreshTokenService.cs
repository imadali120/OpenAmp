using System.Security.Cryptography;
using System.Text;
using OpenAmp.Application.Auth;

namespace OpenAmp.Infrastructure.Auth;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private static readonly TimeSpan Trajanje = TimeSpan.FromDays(30);

    public GenerisaniRefreshToken Kreiraj(DateTime sadaUtc)
    {
        var vrijednost = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new GenerisaniRefreshToken(vrijednost, Hash(vrijednost), sadaUtc.Add(Trajanje));
    }

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
