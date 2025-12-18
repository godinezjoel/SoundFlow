﻿namespace MauiApp2.Components;

public partial class CustomTabBar : ContentView
{
    private readonly Color _activeColor = Color.FromArgb("#2A7FE6");
    private readonly Color _inactiveColor = Colors.White;

    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(CustomTabBar), "HomePage", propertyChanged: OnCurrentPageChanged);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public CustomTabBar()
    {
        InitializeComponent();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        UpdateActiveTab();
    }

    private static void OnCurrentPageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomTabBar tabBar)
        {
            tabBar.UpdateActiveTab();
        }
    }

    private void UpdateActiveTab()
    {
        // Reset all icons to inactive
        HomeIcon.Stroke = _inactiveColor;
        HomeIcon.Fill = null;
        
        LikedIcon.Stroke = _inactiveColor;
        LikedIcon.Fill = null;
        
        StatsIcon.Stroke = _inactiveColor;
        StatsIcon.Fill = null;
        
        AccountIcon.Fill = _inactiveColor;

        // Set active icon
        switch (CurrentPage)
        {
            case "HomePage":
                HomeIcon.Stroke = _activeColor;
                break;
            case "LikedSongsPage":
                LikedIcon.Stroke = _activeColor;
                break;
            case "StatsPage":
                StatsIcon.Stroke = _activeColor;
                break;
            case "AccountPage":
                AccountIcon.Fill = _activeColor;
                break;
        }
    }

    private async void OnHomeClicked(object? sender, TappedEventArgs e)
    {
        CurrentPage = "HomePage";
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async void OnLikedClicked(object? sender, TappedEventArgs e)
    {
        CurrentPage = "LikedSongsPage";
        await Shell.Current.GoToAsync("//LikedSongsPage");
    }

    private async void OnStatsClicked(object? sender, TappedEventArgs e)
    {
        CurrentPage = "StatsPage";
        await Shell.Current.GoToAsync("//StatsPage");
    }

    private async void OnAccountClicked(object? sender, TappedEventArgs e)
    {
        CurrentPage = "AccountPage";
        await Shell.Current.GoToAsync("//AccountPage");
    }
}