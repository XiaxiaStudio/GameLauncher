using Microsoft.UI.Xaml.Data;
using Windows.Media.Core;

namespace GameLauncher.Converters;

public class StringToMediaSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string path && !string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try
            {
                return MediaSource.CreateFromUri(new Uri(path));
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
