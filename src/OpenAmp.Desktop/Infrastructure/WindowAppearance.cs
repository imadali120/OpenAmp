using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace OpenAmp.Desktop.Infrastructure;

public static class WindowAppearance
{
    private const int UseImmersiveDarkMode = 20;
    private const int BorderColor = 34;
    private const int CaptionColor = 35;
    private const int TextColor = 36;

    public static void UseOpenAmpChrome(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            var handle = new WindowInteropHelper(window).Handle;
            var enabled = 1;
            var border = 0x00312A27;
            var caption = 0x00120F0E;
            var text = 0x00EFF3F5;

            _ = DwmSetWindowAttribute(handle, UseImmersiveDarkMode, ref enabled, sizeof(int));
            _ = DwmSetWindowAttribute(handle, BorderColor, ref border, sizeof(int));
            _ = DwmSetWindowAttribute(handle, CaptionColor, ref caption, sizeof(int));
            _ = DwmSetWindowAttribute(handle, TextColor, ref text, sizeof(int));
        };
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int attributeValue,
        int attributeSize);
}
