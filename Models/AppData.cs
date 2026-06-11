using System.Collections.ObjectModel;

namespace GameLauncher.Models;

public class AppData
{
    public ObservableCollection<GameItem> Games { get; set; } = new();
    public AiSettings Ai { get; set; } = new();
    public ObservableCollection<string> SteamLibraryPaths { get; set; } = new();
    public string Theme { get; set; } = "Default";
}

public class AiSettings
{
    public string Endpoint { get; set; } = "https://opencode.ai/zen/go/v1/chat/completions";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "deepseek-v4-flash";
}
