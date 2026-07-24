using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenAmp.Desktop.Infrastructure;

public sealed class StatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value?.ToString()?.ToUpperInvariant() ?? "";
        return text switch
        {
            "AKTIVNA" or "DOSTUPNA" or "AKTIVAN" or "PLACENA" or "IZVRSENA" => Brush("#38B77A"),
            "ODRZAVANJE" or "SERVIS" or "NA_CEKANJU" => Brush("#E8A43A"),
            "NEAKTIVNA" or "POKVARENA" or "OTKAZANA" or "UKINUT" => Brush("#F05A65"),
            _ => Brush("#8C8991")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static SolidColorBrush Brush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));
}

public sealed class GenreBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value?.ToString()?.ToUpperInvariant() ?? "";
        var color = text switch
        {
            "ROCK" => "#FF5A36",
            "METAL" => "#706C78",
            "JAZZ" => "#E8A43A",
            "POP" => "#E45A8D",
            "FUNK" => "#35B58A",
            _ => "#4D9FE6"
        };
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
