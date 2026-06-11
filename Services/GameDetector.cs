using System.Text.Json;
using GameLauncher.Models;

namespace GameLauncher.Services;

public static class GameDetector
{
    public static List<GameItem> DetectSteamGames(IEnumerable<string>? customPaths = null)
    {
        var games = new List<GameItem>();
        var steamPaths = GetSteamLibraryFolders();

        if (customPaths != null)
        {
            foreach (var p in customPaths)
            {
                if (!string.IsNullOrWhiteSpace(p) && Directory.Exists(p) && !steamPaths.Contains(p))
                    steamPaths.Add(p);
            }
        }

        foreach (var steamPath in steamPaths)
        {
            var appsPath = Path.Combine(steamPath, "steamapps");
            if (!Directory.Exists(appsPath)) continue;

            foreach (var manifestFile in Directory.GetFiles(appsPath, "appmanifest_*.acf"))
            {
                try
                {
                    var content = File.ReadAllText(manifestFile);
                    var name = ExtractValue(content, "name");
                    var installDir = ExtractValue(content, "installdir");

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(installDir)) continue;

                    var gamePath = Path.Combine(appsPath, "common", installDir);
                    if (!Directory.Exists(gamePath)) continue;

                    var exeFile = FindGameExe(gamePath);
                    if (exeFile == null) continue;

                    var appId = Path.GetFileNameWithoutExtension(manifestFile).Replace("appmanifest_", "");

                    games.Add(new GameItem
                    {
                        Name = name,
                        ExePath = exeFile,
                        Description = "Steam 游戏",
                        SteamAppId = appId
                    });
                }
                catch { }
            }
        }

        return games;
    }

    public static List<GameItem> DetectEpicGames()
    {
        var games = new List<GameItem>();
        var epicManifestsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests");

        if (!Directory.Exists(epicManifestsPath)) return games;

        foreach (var manifestFile in Directory.GetFiles(epicManifestsPath, "*.item"))
        {
            try
            {
                var json = File.ReadAllText(manifestFile);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var displayName = root.GetProperty("DisplayName").GetString();
                var installLocation = root.GetProperty("InstallLocation").GetString();
                var launchExecutable = root.TryGetProperty("LaunchExecutable", out var le) ? le.GetString() : null;

                if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(installLocation)) continue;

                var exePath = !string.IsNullOrEmpty(launchExecutable)
                    ? Path.Combine(installLocation, launchExecutable)
                    : FindGameExe(installLocation);

                if (exePath == null || !File.Exists(exePath)) continue;

                games.Add(new GameItem
                {
                    Name = displayName,
                    ExePath = exePath,
                    Description = "Epic 游戏"
                });
            }
            catch { }
        }

        return games;
    }

    private static List<string> GetSteamLibraryFolders()
    {
        var paths = new List<string>();
        var defaultPath = @"C:\Program Files (x86)\Steam";
        if (Directory.Exists(defaultPath))
            paths.Add(defaultPath);

        var libraryFoldersFile = Path.Combine(defaultPath, "steamapps", "libraryfolders.vdf");
        if (File.Exists(libraryFoldersFile))
        {
            var content = File.ReadAllText(libraryFoldersFile);
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("\"path\""))
                {
                    var start = trimmed.IndexOf('"', trimmed.IndexOf("\"path\"") + 6);
                    if (start < 0) continue;
                    var end = trimmed.IndexOf('"', start + 1);
                    if (end < 0) continue;
                    var path = trimmed.Substring(start + 1, end - start - 1).Replace("\\\\", "\\");
                    if (Directory.Exists(path) && !paths.Contains(path))
                        paths.Add(path);
                }
            }
        }

        return paths;
    }

    private static string? FindGameExe(string gamePath)
    {
        var exes = Directory.GetFiles(gamePath, "*.exe", SearchOption.TopDirectoryOnly);
        if (exes.Length == 0) return null;

        var gameName = Path.GetFileName(gamePath).ToLower();
        var bestMatch = exes.FirstOrDefault(e =>
            Path.GetFileNameWithoutExtension(e).ToLower().Contains(gameName) ||
            gameName.Contains(Path.GetFileNameWithoutExtension(e).ToLower()));

        return bestMatch ?? exes.FirstOrDefault();
    }

    private static string? ExtractValue(string content, string key)
    {
        var pattern = $"\"{key}\"\t\t\"";
        var start = content.IndexOf(pattern);
        if (start < 0) return null;
        start += pattern.Length;
        var end = content.IndexOf('"', start);
        if (end < 0) return null;
        return content.Substring(start, end - start);
    }
}
