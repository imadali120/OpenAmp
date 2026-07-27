using System.Windows;
using System.Windows.Input;
using OpenAmp.Desktop.Infrastructure;
using OpenAmp.Desktop.ViewModels;

namespace OpenAmp.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        WindowAppearance.UseOpenAmpChrome(this);
        InitializeComponent();
        Loaded += (_, _) =>
        {
            PasswordInput.Password = "OpenAmp1!";
            PasswordInput.Focus();
        };
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel && await viewModel.LoginAsync(PasswordInput.Password))
        {
            DialogResult = true;
        }
    }

    private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Login_Click(sender, e);
        }
    }
}
