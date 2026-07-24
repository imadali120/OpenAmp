using OpenAmp.Desktop.Infrastructure;
using OpenAmp.Desktop.Models;
using OpenAmp.Desktop.Services;

namespace OpenAmp.Desktop.ViewModels;

public sealed class LoginViewModel(OpenAmpApiClient api) : ObservableObject
{
    private string _identifier = "admin";
    private string _error = "";
    private bool _isBusy;

    public string Identifier
    {
        get => _identifier;
        set => SetProperty(ref _identifier, value);
    }

    public string Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public AuthSession? Session { get; private set; }

    public async Task<bool> LoginAsync(string password)
    {
        Error = "";
        if (string.IsNullOrWhiteSpace(Identifier) || string.IsNullOrEmpty(password))
        {
            Error = "Unesi username/email i lozinku.";
            return false;
        }
        IsBusy = true;
        try
        {
            Session = await api.LoginAsync(Identifier.Trim(), password);
            if (Session.Korisnik.Uloga is not ("Administrator" or "Zaposlenik"))
            {
                Session = null;
                Error = "Desktop aplikacija je dostupna administratorima i zaposlenicima.";
                return false;
            }
            api.SetSession(Session);
            return true;
        }
        catch (Exception exception)
        {
            Error = exception.Message;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
