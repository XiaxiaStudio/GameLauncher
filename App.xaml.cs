using GameLauncher.Services;
using GameLauncher.ViewModels;
using Microsoft.UI.Xaml;

namespace GameLauncher;

public partial class App : Application
{
    private Window? _window;

    public static MainViewModel ViewModel { get; private set; } = null!;
    public static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();

        var dataService = new DataService();
        ViewModel = new MainViewModel(dataService);
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainWindow = _window;
        _window.Activate();
    }
}
