using System.Text.RegularExpressions;

namespace OpenAmp.Application.Auth;

public static partial class CredentialPolicy
{
    public const int PasswordMinLength = 10;
    public const int PasswordMaxLength = 128;
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 30;

    public static string NormalizeUsername(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        if (!UsernameRegex().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Username mora imati 3–30 znakova i može sadržavati mala slova, brojeve, tačku i donju crtu.");
        }

        return normalized;
    }

    public static void ValidatePassword(string password)
    {
        if (password.Length is < PasswordMinLength or > PasswordMaxLength
            || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit)
            || !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            throw new ArgumentException(
                "Lozinka mora imati najmanje 10 znakova, veliko i malo slovo, broj i poseban znak.");
        }
    }

    [GeneratedRegex(@"^[a-z0-9](?:[a-z0-9._]{1,28}[a-z0-9])?$", RegexOptions.CultureInvariant)]
    private static partial Regex UsernameRegex();
}
