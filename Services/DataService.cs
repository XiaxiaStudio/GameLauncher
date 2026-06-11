using System.Text.Json;
using GameLauncher.Models;

namespace GameLauncher.Services;

public class DataService
{
    private readonly string _dataFilePath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public DataService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameLauncher");
        Directory.CreateDirectory(appDataPath);
        _dataFilePath = Path.Combine(appDataPath, "data.json");
    }

    public AppData Load()
    {
        if (!File.Exists(_dataFilePath))
            return new AppData();

        try
        {
            var json = File.ReadAllText(_dataFilePath);
            return JsonSerializer.Deserialize<AppData>(json, _jsonOptions) ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    public void Save(AppData data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(_dataFilePath, json);
    }
}
