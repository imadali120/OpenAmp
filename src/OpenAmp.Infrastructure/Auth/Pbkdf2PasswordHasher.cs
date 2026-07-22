using System.Security.Cryptography;
using OpenAmp.Application.Auth;

namespace OpenAmp.Infrastructure.Auth;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 64;
    private const string Prefix = "pbkdf2-sha512";

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var dijelovi = passwordHash.Split('$');
        if (dijelovi.Length != 4
            || dijelovi[0] != Prefix
            || !int.TryParse(dijelovi[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(dijelovi[2]);
            var ocekivani = Convert.FromBase64String(dijelovi[3]);
            var stvarni = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                ocekivani.Length);
            return CryptographicOperations.FixedTimeEquals(stvarni, ocekivani);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
