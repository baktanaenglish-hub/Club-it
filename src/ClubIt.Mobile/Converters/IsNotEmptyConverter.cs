using System.Globalization;

namespace ClubIt.Mobile.Converters;

/// <summary>True for a non-empty string (or any non-null object).</summary>
public class IsNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text ? !string.IsNullOrWhiteSpace(text) : value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
