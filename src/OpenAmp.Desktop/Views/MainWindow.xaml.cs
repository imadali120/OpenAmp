using System.Windows;

namespace OpenAmp.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closed += (_, _) => Application.Current.Shutdown();
    }
}
