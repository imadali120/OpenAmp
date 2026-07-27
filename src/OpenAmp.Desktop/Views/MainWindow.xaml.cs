using System.Windows;
using OpenAmp.Desktop.Infrastructure;

namespace OpenAmp.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        WindowAppearance.UseOpenAmpChrome(this);
        InitializeComponent();
        Closed += (_, _) => Application.Current.Shutdown();
    }
}
