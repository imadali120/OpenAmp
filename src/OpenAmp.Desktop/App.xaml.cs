using System.Windows;
using OpenAmp.Desktop.Services;
using OpenAmp.Desktop.ViewModels;
using OpenAmp.Desktop.Views;

namespace OpenAmp.Desktop;

public partial class App : Application, IDisposable
{
    private OpenAmpApiClient? _api;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var baseUrl = Environment.GetEnvironmentVariable("OPENAMP_API_URL") ?? "http://localhost:5264";
        _api = new OpenAmpApiClient(baseUrl);
        var login = new LoginWindow
        {
            DataContext = new LoginViewModel(_api)
        };
        if (login.ShowDialog() != true || login.DataContext is not LoginViewModel loginVm || loginVm.Session is null)
        {
            Shutdown();
            return;
        }
        var main = new MainWindow
        {
            DataContext = new MainViewModel(_api, loginVm.Session)
        };
        MainWindow = main;
        main.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();
        base.OnExit(e);
    }

    public void Dispose()
    {
        _api?.Dispose();
        _api = null;
        GC.SuppressFinalize(this);
    }
}
