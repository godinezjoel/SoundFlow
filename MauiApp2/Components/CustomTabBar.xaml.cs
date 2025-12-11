namespace MauiApp2.Components;

public partial class CustomTabBar : ContentView
{
    public CustomTabBar()
    {
        InitializeComponent();
    }

    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async void OnLikedClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LikedSongsPage");
    }

    private async void OnStatsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//StatsPage");
    }

    private async void OnAccountClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AccountPage");
    }
}