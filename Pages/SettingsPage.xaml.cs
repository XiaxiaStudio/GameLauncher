using GameLauncher.Models;
using GameLauncher.Services;
using GameLauncher.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GameLauncher.Pages;

public sealed partial class SettingsPage : Page
{
    private MainViewModel ViewModel => App.ViewModel;
    private GameItem? _selectedGame;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        AiEndpointBox.Text = ViewModel.AiSettings.Endpoint;
        AiKeyBox.Password = ViewModel.AiSettings.ApiKey;
        AiModelBox.Text = ViewModel.AiSettings.Model;

        var themeIndex = ViewModel.Theme switch
        {
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };
        ThemeComboBox.SelectedIndex = themeIndex;

        var backdropIndex = ViewModel.Backdrop switch
        {
            "Acrylic" => 1,
            _ => 0
        };
        BackdropComboBox.SelectedIndex = backdropIndex;
    }

    private void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedGame = GamesListView.SelectedItem as GameItem;
        ViewModel.SelectedGame = _selectedGame;

        if (_selectedGame != null)
        {
            EditPanel.Visibility = Visibility.Visible;
            GameNameBox.Text = _selectedGame.Name;
            GameExePathBox.Text = _selectedGame.ExePath;
            SteamAppIdBox.Text = _selectedGame.SteamAppId;
            GameIconPathBox.Text = _selectedGame.IconPath;
            GameDescBox.Text = _selectedGame.Description;
            CarouselListView.ItemsSource = _selectedGame.CarouselImages;
            ScreenshotsListView.ItemsSource = _selectedGame.Screenshots;
            CarouselPreview.Visibility = Visibility.Collapsed;
            UpdateSimplifyBtn();
        }
        else
        {
            EditPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void GameDescBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSimplifyBtn();
    }

    private void UpdateSimplifyBtn()
    {
        var text = GameDescBox.Text ?? "";
        var hasAi = !string.IsNullOrWhiteSpace(ViewModel.AiSettings.ApiKey);
        SimplifyBtn.Visibility = (text.Length > 15 && hasAi) ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void SimplifyDesc_Click(object sender, RoutedEventArgs e)
    {
        var desc = GameDescBox.Text;
        if (string.IsNullOrWhiteSpace(desc) || desc.Length <= 15) return;

        SimplifyBtn.IsEnabled = false;
        SimplifyBtn.Content = "AI 简化中...";

        var (result, error) = await AiService.SimplifyDescriptionAsync(ViewModel.AiSettings, desc);

        SimplifyBtn.IsEnabled = true;
        SimplifyBtn.Content = "AI 简化";

        if (!string.IsNullOrEmpty(result))
        {
            GameDescBox.Text = result;
        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "AI 简化失败",
                Content = error ?? "未知错误",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private void AddGame_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddGameCommand.Execute(null);
    }

    private void DeleteGame_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteGameCommand.Execute(null);
    }

    private void FavoriteIcon_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is FontIcon icon && icon.Tag is GameItem game)
        {
            ViewModel.ToggleFavoriteCommand.Execute(game);
            // 强制刷新列表，让星星状态立即更新
            GamesListView.ItemsSource = null;
            GamesListView.ItemsSource = ViewModel.Games;
        }
        e.Handled = true;
    }

    public static Visibility BoolToVis(bool value)
        => value ? Visibility.Visible : Visibility.Collapsed;

    private async void ImportGames_Click(object sender, RoutedEventArgs e)
    {
        var steamGames = GameDetector.DetectSteamGames(ViewModel.SteamLibraryPaths);
        var epicGames = GameDetector.DetectEpicGames();
        var allDetected = steamGames.Concat(epicGames).ToList();

        if (allDetected.Count == 0)
        {
            var dialog = new ContentDialog
            {
                Title = "未检测到游戏",
                Content = "未在 Steam 或 Epic 默认路径中检测到已安装的游戏",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        var existingExePaths = new HashSet<string>(
            ViewModel.Games.Select(g => g.ExePath.ToLowerInvariant()));

        var newGames = allDetected
            .Where(g => !existingExePaths.Contains(g.ExePath.ToLowerInvariant()))
            .ToList();

        if (newGames.Count == 0)
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = "检测到的游戏已全部在列表中",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        foreach (var game in newGames)
            ViewModel.Games.Add(game);

        ViewModel.SaveData();

        var result = new ContentDialog
        {
            Title = "导入完成",
            Content = $"成功导入 {newGames.Count} 个游戏（Steam: {steamGames.Count}, Epic: {epicGames.Count}）",
            CloseButtonText = "确定",
            XamlRoot = this.XamlRoot
        };
        await result.ShowAsync();
    }

    private void SaveGame_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame != null)
        {
            _selectedGame.Name = GameNameBox.Text;
            _selectedGame.ExePath = GameExePathBox.Text;
            _selectedGame.SteamAppId = SteamAppIdBox.Text?.Trim() ?? "";
            _selectedGame.IconPath = GameIconPathBox.Text;
            _selectedGame.Description = GameDescBox.Text;
            ViewModel.SaveData();

            GamesListView.ItemsSource = null;
            GamesListView.ItemsSource = ViewModel.Games;
        }
    }

    private void SaveAiSettings_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AiSettings.Endpoint = AiEndpointBox.Text?.Trim() ?? "";
        ViewModel.AiSettings.ApiKey = AiKeyBox.Password?.Trim() ?? "";
        ViewModel.AiSettings.Model = AiModelBox.Text?.Trim() ?? "";
        ViewModel.SaveData();
        UpdateSimplifyBtn();
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string theme)
        {
            ViewModel.Theme = theme;
        }
    }

    private void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BackdropComboBox.SelectedItem is ComboBoxItem item && item.Tag is string backdrop)
        {
            ViewModel.Backdrop = backdrop;
        }
    }

    private async void FetchModels_Click(object sender, RoutedEventArgs e)
    {
        var endpoint = AiEndpointBox.Text?.Trim() ?? "";
        var apiKey = AiKeyBox.Password?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = "请先填写 API 地址和 API Key",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        FetchModelsBtn.IsEnabled = false;
        FetchModelsBtn.Content = "获取中...";

        var tempSettings = new AiSettings { Endpoint = endpoint, ApiKey = apiKey };
        var models = await AiService.FetchModelsAsync(tempSettings);

        FetchModelsBtn.IsEnabled = true;
        FetchModelsBtn.Content = "获取模型";

        if (models.Count > 0)
        {
            var currentModel = AiModelBox.Text;
            AiModelBox.ItemsSource = models;
            if (!string.IsNullOrEmpty(currentModel) && models.Contains(currentModel))
                AiModelBox.SelectedItem = currentModel;
            else if (models.Count > 0)
                AiModelBox.SelectedIndex = 0;
        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "获取失败",
                Content = "无法获取模型列表，请检查 API 地址和 Key 是否正确",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private async void BrowseExe_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".exe");
        picker.FileTypeFilter.Add(".lnk");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            GameExePathBox.Text = file.Path;
            if (string.IsNullOrEmpty(GameNameBox.Text) || GameNameBox.Text == "新游戏")
                GameNameBox.Text = System.IO.Path.GetFileNameWithoutExtension(file.Path);
        }
    }

    private async void BrowseIcon_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".ico");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            var copied = await ImageService.CopyToImageFolderAsync(file.Path);
            GameIconPathBox.Text = copied ?? file.Path;
        }
    }

    private async void AddCarouselImage_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null) return;

        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".mp4");
        picker.FileTypeFilter.Add(".avi");
        picker.FileTypeFilter.Add(".mkv");
        picker.FileTypeFilter.Add(".mov");
        picker.FileTypeFilter.Add(".wmv");
        picker.FileTypeFilter.Add(".flv");
        picker.FileTypeFilter.Add(".webm");
        picker.FileTypeFilter.Add(".m4v");

        var files = await picker.PickMultipleFilesAsync();
        foreach (var file in files)
        {
            var copied = await ImageService.CopyToImageFolderAsync(file.Path);
            _selectedGame.CarouselImages.Add(copied ?? file.Path);
        }

        if (files.Count > 0)
            ViewModel.SaveData();

        CarouselListView.ItemsSource = null;
        CarouselListView.ItemsSource = _selectedGame.CarouselImages;
    }

    private void RemoveCarouselImage_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null) return;

        var selected = CarouselListView.SelectedItem as string;
        if (selected != null)
        {
            _selectedGame.CarouselImages.Remove(selected);
            ViewModel.SaveData();

            CarouselListView.ItemsSource = null;
            CarouselListView.ItemsSource = _selectedGame.CarouselImages;
            CarouselPreview.Visibility = Visibility.Collapsed;
        }
    }

    private async void CropImage_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null) return;

        var button = sender as Button;
        var imagePath = button?.Tag as string;
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath)) return;

        var dialog = new CropDialog(imagePath)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.CroppedImagePath != null)
        {
            var index = _selectedGame.CarouselImages.IndexOf(imagePath);
            if (index >= 0)
            {
                _selectedGame.CarouselImages[index] = dialog.CroppedImagePath;
                ViewModel.SaveData();

                CarouselListView.ItemsSource = null;
                CarouselListView.ItemsSource = _selectedGame.CarouselImages;
            }
        }
    }

    private async void AddScreenshot_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null) return;

        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");

        var files = await picker.PickMultipleFilesAsync();
        foreach (var file in files)
        {
            var copied = await ImageService.CopyToImageFolderAsync(file.Path);
            _selectedGame.Screenshots.Add(copied ?? file.Path);
        }

        if (files.Count > 0)
            ViewModel.SaveData();

        ScreenshotsListView.ItemsSource = null;
        ScreenshotsListView.ItemsSource = _selectedGame.Screenshots;
    }

    private void RemoveScreenshot_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null) return;

        var selected = ScreenshotsListView.SelectedItem as string;
        if (selected != null)
        {
            _selectedGame.Screenshots.Remove(selected);
            ViewModel.SaveData();

            ScreenshotsListView.ItemsSource = null;
            ScreenshotsListView.ItemsSource = _selectedGame.Screenshots;
        }
    }

    private async void CropScreenshot_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null) return;

        var button = sender as Button;
        var imagePath = button?.Tag as string;
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath)) return;

        var dialog = new CropDialog(imagePath)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.CroppedImagePath != null)
        {
            var index = _selectedGame.Screenshots.IndexOf(imagePath);
            if (index >= 0)
            {
                _selectedGame.Screenshots[index] = dialog.CroppedImagePath;
                ViewModel.SaveData();

                ScreenshotsListView.ItemsSource = null;
                ScreenshotsListView.ItemsSource = _selectedGame.Screenshots;
            }
        }
    }

    private async void AddSteamPath_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null && !ViewModel.SteamLibraryPaths.Contains(folder.Path))
        {
            ViewModel.SteamLibraryPaths.Add(folder.Path);
            ViewModel.SaveData();
        }
    }

    private void RemoveSteamPath_Click(object sender, RoutedEventArgs e)
    {
        var selected = SteamPathsListView.SelectedItem as string;
        if (selected != null)
        {
            ViewModel.SteamLibraryPaths.Remove(selected);
            ViewModel.SaveData();
        }
    }
}
