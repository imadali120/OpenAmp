using OpenAmp.Infrastructure.Auth;

namespace OpenAmp.Infrastructure.Tests.Domain;

public sealed class AuthSecurityTests
{
    [Fact]
    public void PasswordHasherKoristiJedinstvenuSoIVerifikujeLozinku()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var prvi = hasher.Hash("TestPassword!123");
        var drugi = hasher.Hash("TestPassword!123");

        Assert.NotEqual(prvi, drugi);
        Assert.True(hasher.Verify("TestPassword!123", prvi));
        Assert.False(hasher.Verify("PogresnaPassword!123", prvi));
    }

    [Fact]
    public void RefreshTokenJeNasumicanIHashJeDeterministican()
    {
        var service = new RefreshTokenService();
        var sada = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
        var prvi = service.Kreiraj(sada);
        var drugi = service.Kreiraj(sada);

        Assert.NotEqual(prvi.Vrijednost, drugi.Vrijednost);
        Assert.Equal(prvi.Hash, service.Hash(prvi.Vrijednost));
        Assert.Equal(sada.AddDays(30), prvi.IsticeUtc);
    }
}
