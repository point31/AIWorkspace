using AIWorkspace.AI;
using System.Globalization;
using System.Windows.Data;

namespace AIWorkspace.Converters;

[ValueConversion(typeof(ProviderType), typeof(string))]
public class ProviderTypeNameConverter : IValueConverter
{
    public static readonly ProviderTypeNameConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is ProviderType pt ? pt switch
        {
            ProviderType.OpenAI => "OpenAI",
            ProviderType.Claude => "Claude (Anthropic)",
            ProviderType.Gemini => "Gemini (Google)",
            _                   => pt.ToString()
        } : "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
