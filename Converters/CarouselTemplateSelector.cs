using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GameLauncher.Converters;

public class CarouselTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ImageTemplate { get; set; }
    public DataTemplate? VideoTemplate { get; set; }

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v"
    };

    public static bool IsVideoFile(string path)
    {
        var ext = Path.GetExtension(path);
        return !string.IsNullOrEmpty(ext) && VideoExtensions.Contains(ext);
    }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is string path && IsVideoFile(path))
            return VideoTemplate ?? ImageTemplate;
        return ImageTemplate;
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
