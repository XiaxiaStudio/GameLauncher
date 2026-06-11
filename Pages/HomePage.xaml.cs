using System.Collections.ObjectModel;
using GameLauncher.Models;
using GameLauncher.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace GameLauncher.Pages;

public sealed partial class HomePage : Page
{
    private MainViewModel ViewModel => App.ViewModel;
    public ObservableCollection<GameItem> FilteredGames { get; } = new();

    public HomePage()
    {
        InitializeComponent();
        Loaded += HomePage_Loaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        RefreshFilteredGames();
    }

    private void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshFilteredGames();
        ViewModel.Games.CollectionChanged += (s, args) => RefreshFilteredGames();
    }

    private void RefreshFilteredGames()
    {
        var searchText = SearchBox?.Text?.Trim().ToLower() ?? "";
        var allGames = ViewModel.Games;

        var filtered = string.IsNullOrEmpty(searchText)
            ? allGames.ToList()
            : allGames.Where(g => g.Name.ToLower().Contains(searchText) ||
                                  g.Description.ToLower().Contains(searchText)).ToList();

        filtered.Sort((a, b) =>
        {
            if (a.IsFavorite != b.IsFavorite)
                return a.IsFavorite ? -1 : 1;
            if (a.LastPlayed != null && b.LastPlayed != null)
                return b.LastPlayed.Value.CompareTo(a.LastPlayed.Value);
            if (a.LastPlayed != null) return -1;
            if (b.LastPlayed != null) return 1;
            return 0;
        });

        FilteredGames.Clear();
        foreach (var game in filtered)
            FilteredGames.Add(game);

        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        var hasGames = FilteredGames.Count > 0;
        GamesList.Visibility = hasGames ? Visibility.Visible : Visibility.Collapsed;
        EmptyPanel.Visibility = hasGames ? Visibility.Collapsed : Visibility.Visible;
        EmptyText.Text = string.IsNullOrEmpty(SearchBox?.Text) ? "还没有添加游戏" : "没有找到匹配的游戏";
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        RefreshFilteredGames();
    }

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            RefreshFilteredGames();
    }

    private void GamesList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is GameItem game)
        {
            ViewModel.SelectedGame = game;
            Frame.Navigate(typeof(GameDetailPage), game);
        }
    }

    private void GamesList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        var newOrder = FilteredGames.ToList();
        var mainGames = ViewModel.Games;

        mainGames.Clear();
        foreach (var game in newOrder)
            mainGames.Add(game);

        ViewModel.SaveData();
    }

    public static Visibility BoolToVis(bool value)
        => value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility CountToVis(int count)
        => count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility StringToVis(string? value)
        => string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility StringNotEmptyToVis(string? value)
        => !string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;
}
