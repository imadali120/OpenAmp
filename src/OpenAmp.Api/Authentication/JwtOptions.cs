namespace OpenAmp.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "OpenAmp.Api";
    public string Audience { get; set; } = "OpenAmp.Clients";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
}
