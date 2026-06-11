using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameLauncher.Models;
using GameLauncher.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GameLauncher.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly DataService _dataService;
    private GameItem? _selectedGame;

    public ObservableCollection<GameItem> Games { get; set; } = new();

    public int GamesCount => Games.Count;

    public AiSettings AiSettings { get; set; } = new();

    public ObservableCollection<string> SteamLibraryPaths { get; set; } = new();

    private string _theme = "Default";
    public string Theme
    {
        get => _theme;
        set
        {
            if (_theme != value)
            {
                _theme = value;
                OnPropertyChanged();
                ApplyTheme();
                SaveData();
            }
        }
    }

    public GameItem? SelectedGame
    {
        get => _selectedGame;
        set { _selectedGame = value; OnPropertyChanged(); }
    }

    public ICommand AddGameCommand { get; }
    public ICommand DeleteGameCommand { get; }
    public ICommand LaunchGameCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }

    public MainViewModel(DataService dataService)
    {
        _dataService = dataService;

        AddGameCommand = new RelayCommand(AddGame);
        DeleteGameCommand = new RelayCommand(DeleteGame, () => SelectedGame != null);
        LaunchGameCommand = new RelayCommand(LaunchGame, () => SelectedGame != null);
        ToggleFavoriteCommand = new RelayCommand<GameItem>(ToggleFavorite);

        LoadData();
    }

    public void LoadData()
    {
        var data = _dataService.Load();
        Games = data.Games;
        AiSettings = data.Ai;
        SteamLibraryPaths = data.SteamLibraryPaths;
        _theme = data.Theme;

        Games.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(GamesCount));
            SaveData();
        };

        OnPropertyChanged(nameof(Games));
        OnPropertyChanged(nameof(GamesCount));
        OnPropertyChanged(nameof(AiSettings));
        OnPropertyChanged(nameof(SteamLibraryPaths));
        OnPropertyChanged(nameof(Theme));

        ApplyTheme();
    }

    public void SaveData()
    {
        var data = new AppData();
        foreach (var g in Games) data.Games.Add(g);
        data.Ai = AiSettings;
        foreach (var p in SteamLibraryPaths) data.SteamLibraryPaths.Add(p);
        data.Theme = _theme;
        _dataService.Save(data);
    }

    private void ApplyTheme()
    {
        if (App.MainWindow?.Content is FrameworkElement root)
        {
            root.RequestedTheme = Theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }
    }

    private void AddGame()
    {
        var newGame = new GameItem { Name = "新游戏", Description = "游戏描述" };
        Games.Add(newGame);
        SelectedGame = newGame;
    }

    private void DeleteGame()
    {
        if (SelectedGame != null)
        {
            Games.Remove(SelectedGame);
            SelectedGame = null;
        }
    }

    private void ToggleFavorite(GameItem? game)
    {
        if (game != null)
        {
            game.IsFavorite = !game.IsFavorite;
            SaveData();
        }
    }

    private async void LaunchGame()
    {
        if (SelectedGame == null)
            return;

        try
        {
            ProcessStartInfo psi;
            if (!string.IsNullOrWhiteSpace(SelectedGame.SteamAppId))
            {
                psi = new ProcessStartInfo
                {
                    FileName = $"steam://rungameid/{SelectedGame.SteamAppId}",
                    UseShellExecute = true
                };
            }
            else if (!string.IsNullOrWhiteSpace(SelectedGame.ExePath))
            {
                psi = new ProcessStartInfo
                {
                    FileName = SelectedGame.ExePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(SelectedGame.ExePath)
                };
            }
            else
            {
                return;
            }

            Process.Start(psi);

            SelectedGame.PlayCount++;
            SelectedGame.LastPlayed = DateTime.Now;
            SaveData();
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "启动失败",
                Content = $"无法启动游戏: {ex.Message}",
                CloseButtonText = "确定",
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

    public void Execute(object? parameter) => _execute((T?)parameter);

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
