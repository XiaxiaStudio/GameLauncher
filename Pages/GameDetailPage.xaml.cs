using System.Diagnostics;
using GameLauncher.Converters;
using GameLauncher.Models;
using GameLauncher.Services;
using GameLauncher.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GameLauncher.Pages;

public sealed partial class GameDetailPage : Page
{
    private GameItem? _game;
    private DispatcherTimer? _carouselTimer;
    private MainViewModel ViewModel => App.ViewModel;

    public GameDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is GameItem game)
        {
            _game = game;
            BindGame(game);
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        _carouselTimer?.Stop();
        base.OnNavigatingFrom(e);
    }

    private void BindGame(GameItem game)
    {
        GameNameText.Text = game.Name;
        GameDescText.Text = string.IsNullOrEmpty(game.Description) ? "暂无描述" : game.Description;
        ExePathText.Text = string.IsNullOrEmpty(game.ExePath) ? "未设置" : game.ExePath;
        LaunchButton.IsEnabled = !string.IsNullOrEmpty(game.ExePath) || !string.IsNullOrEmpty(game.SteamAppId);

        if (!string.IsNullOrEmpty(game.IconPath) && File.Exists(game.IconPath))
        {
            GameIcon.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(game.IconPath));
            GameIcon.Visibility = Visibility.Visible;
            DefaultIcon.Visibility = Visibility.Collapsed;
        }
        else
        {
            GameIcon.Visibility = Visibility.Collapsed;
            DefaultIcon.Visibility = Visibility.Visible;
        }

        var hasAi = !string.IsNullOrWhiteSpace(ViewModel.AiSettings.ApiKey);
        SimplifyBtn.Visibility = (game.Description?.Length > 15 && hasAi) ? Visibility.Visible : Visibility.Collapsed;

        _carouselTimer?.Stop();

        if (game.CarouselImages.Count > 0)
        {
            CarouselFlipView.ItemsSource = game.CarouselImages.ToList();
            CarouselFlipView.SelectedIndex = 0;
            CarouselFlipView.Visibility = Visibility.Visible;
            NoCarouselPanel.Visibility = Visibility.Collapsed;

            _carouselTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            _carouselTimer.Tick += (s, args) =>
            {
                var count = game.CarouselImages.Count;
                if (count > 0)
                    CarouselFlipView.SelectedIndex = (CarouselFlipView.SelectedIndex + 1) % count;
            };

            CarouselFlipView.SelectionChanged += (s, args) =>
            {
                var selectedItem = CarouselFlipView.SelectedItem as string;
                if (selectedItem != null && CarouselTemplateSelector.IsVideoFile(selectedItem))
                    _carouselTimer?.Stop();
                else
                    _carouselTimer?.Start();
            };

            var firstItem = game.CarouselImages[0];
            if (!CarouselTemplateSelector.IsVideoFile(firstItem))
                _carouselTimer.Start();
        }
        else
        {
            CarouselFlipView.Visibility = Visibility.Collapsed;
            NoCarouselPanel.Visibility = Visibility.Visible;
        }
    }

    private async void SimplifyDesc_Click(object sender, RoutedEventArgs e)
    {
        if (_game == null) return;

        var desc = _game.Description;
        if (string.IsNullOrWhiteSpace(desc) || desc.Length <= 15) return;

        SimplifyBtn.IsEnabled = false;
        SimplifyBtn.Content = "AI 简化中...";

        var (result, error) = await AiService.SimplifyDescriptionAsync(ViewModel.AiSettings, desc);

        SimplifyBtn.IsEnabled = true;
        SimplifyBtn.Content = "AI 简化";

        if (!string.IsNullOrEmpty(result))
        {
            _game.Description = result;
            GameDescText.Text = result;
            ViewModel.SaveData();
        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "AI 简化失败",
                Content = error ?? "未知错误",
                CloseButtonText = "确定",
                XamlRoot = Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
            Frame.GoBack();
    }

    private void Launch_Click(object sender, RoutedEventArgs e)
    {
        if (_game == null) return;

        try
        {
            ProcessStartInfo psi;
            if (!string.IsNullOrWhiteSpace(_game.SteamAppId))
            {
                psi = new ProcessStartInfo
                {
                    FileName = $"steam://rungameid/{_game.SteamAppId}",
                    UseShellExecute = true
                };
            }
            else if (!string.IsNullOrWhiteSpace(_game.ExePath))
            {
                psi = new ProcessStartInfo
                {
                    FileName = _game.ExePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(_game.ExePath)
                };
            }
            else
            {
                return;
            }

            Process.Start(psi);

            _game.PlayCount++;
            _game.LastPlayed = DateTime.Now;
            ViewModel.SaveData();
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "启动失败",
                Content = $"无法启动游戏: {ex.Message}",
                CloseButtonText = "确定",
                XamlRoot = Content.XamlRoot
            };
            _ = dialog.ShowAsync();
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (_game == null || string.IsNullOrWhiteSpace(_game.ExePath)) return;

        var dir = Path.GetDirectoryName(_game.ExePath);
        if (dir != null && Directory.Exists(dir))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = dir,
                UseShellExecute = true
            });
        }
    }
}
