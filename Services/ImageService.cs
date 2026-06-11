namespace GameLauncher.Services;

public static class ImageService
{
    private static string? _imageDir;

    public static string GetImageDirectory()
    {
        if (_imageDir != null) return _imageDir;

        _imageDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameLauncher", "Images");
        Directory.CreateDirectory(_imageDir);
        return _imageDir;
    }

    public static async Task<string?> CopyToImageFolderAsync(string sourcePath)
    {
        if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
            return null;

        try
        {
            var dir = GetImageDirectory();
            var ext = Path.GetExtension(sourcePath);
            var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
            var destPath = Path.Combine(dir, fileName);

            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(destStream);

            return destPath;
        }
        catch
        {
            return null;
        }
    }
}
