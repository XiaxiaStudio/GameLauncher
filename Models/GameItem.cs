using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameLauncher.Models;

public class GameItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _exePath = string.Empty;
    private string _iconPath = string.Empty;
    private string _description = string.Empty;
    private string _steamAppId = string.Empty;
    private bool _isFavorite;
    private DateTime? _lastPlayed;
    private int _playCount;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string ExePath
    {
        get => _exePath;
        set { _exePath = value; OnPropertyChanged(); }
    }

    public string IconPath
    {
        get => _iconPath;
        set { _iconPath = value; OnPropertyChanged(); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public string SteamAppId
    {
        get => _steamAppId;
        set { _steamAppId = value; OnPropertyChanged(); }
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set { _isFavorite = value; OnPropertyChanged(); }
    }

    public DateTime? LastPlayed
    {
        get => _lastPlayed;
        set { _lastPlayed = value; OnPropertyChanged(); OnPropertyChanged(nameof(LastPlayedText)); }
    }

    public int PlayCount
    {
        get => _playCount;
        set { _playCount = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> CarouselImages { get; set; } = new();
    public ObservableCollection<string> Screenshots { get; set; } = new();

    public string LastPlayedText
    {
        get
        {
            if (LastPlayed == null) return "从未游玩";
            var span = DateTime.Now - LastPlayed.Value;
            if (span.TotalMinutes < 1) return "刚刚";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} 分钟前";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours} 小时前";
            return $"{(int)span.TotalDays} 天前";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
