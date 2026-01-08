using MauiApp2.Models;
using MauiApp2.Services;

namespace MauiApp2.Components;

public partial class CustomTabBar : ContentView
{
    private readonly Color _activeColor = Color.FromArgb("#2A7FE6");
    private readonly Color _inactiveColor = Colors.White;
    private readonly MusicService _musicService = MusicService.Instance;

    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(CustomTabBar), "HomePage", 
            propertyChanged: OnCurrentPageChanged,
            defaultBindingMode: BindingMode.OneWay);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public CustomTabBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        
        // Subscribe to profile changes
        _musicService.ProfileChanged += OnProfileChanged;
    }
    
    private void OnProfileChanged(object? sender, EventArgs e)
    {
        // Update profile image in tab bar when profile changes
        MainThread.BeginInvokeOnMainThread(LoadProfileImage);
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        // Update active tab when control is fully loaded
        UpdateActiveTab();
        LoadProfileImage();
    }
    
    private void LoadProfileImage()
    {
        var profile = _musicService.GetUserProfile();
        
        switch (profile.ImageType)
        {
            case ProfileImageType.PresetColor:
                ProfileImageBorder.BackgroundColor = Color.FromArgb(profile.ProfileColor ?? "#2A7FE6");
                ProfileCustomImage.IsVisible = false;
                break;
                
            case ProfileImageType.CustomImage:
                if (!string.IsNullOrEmpty(profile.ProfileImagePath))
                {
                    ProfileImageBorder.BackgroundColor = Colors.Transparent;
                    ProfileCustomImage.Source = ImageSource.FromFile(profile.ProfileImagePath);
                    ProfileCustomImage.IsVisible = true;
                }
                break;
                
            case ProfileImageType.Default:
            default:
                ProfileImageBorder.BackgroundColor = Color.FromArgb("#2A7FE6");
                ProfileCustomImage.IsVisible = false;
                break;
        }
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        // Delay the update to ensure bindings are set
        Dispatcher.Dispatch(() => UpdateActiveTab());
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
        // Reset all icons and labels to inactive
        HomeIcon.Stroke = _inactiveColor;
        HomeIcon.Fill = Brush.Transparent;
        HomeLabel.TextColor = _inactiveColor;
        
        LikedIcon.Stroke = _inactiveColor;
        LikedIcon.Fill = Brush.Transparent;
        LikedLabel.TextColor = _inactiveColor;
        
        StatsIcon.Stroke = _inactiveColor;
        StatsIcon.Fill = Brush.Transparent;
        StatsLabel.TextColor = _inactiveColor;
        
        ProfileImageBorder.Stroke = _inactiveColor;
        AccountLabel.TextColor = _inactiveColor;

        // Set active icon and label
        switch (CurrentPage)
        {
            case "HomePage":
                HomeIcon.Stroke = _activeColor;
                HomeLabel.TextColor = _activeColor;
                break;
            case "LikedSongsPage":
                LikedIcon.Stroke = _activeColor;
                LikedLabel.TextColor = _activeColor;
                break;
            case "StatsPage":
                StatsIcon.Stroke = _activeColor;
                StatsLabel.TextColor = _activeColor;
                break;
            case "AccountPage":
                ProfileImageBorder.Stroke = _activeColor;
                AccountLabel.TextColor = _activeColor;
                break;
        }
    }

    private async void OnHomeClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async void OnLikedClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//LikedSongsPage");
    }

    private async void OnStatsClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//StatsPage");
    }

    private async void OnAccountClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//AccountPage");
    }
}

