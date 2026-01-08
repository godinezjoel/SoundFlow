using MauiApp2.Models;
using MauiApp2.Services;
using System.Text.RegularExpressions;

namespace MauiApp2.Pages;

public partial class AccountPage : ContentPage
{
    private readonly MusicService _musicService = MusicService.Instance;
    private bool _isValidUsername;
    
    // Temporary profile picture state for editing
    private string? _tempProfileImagePath;
    private string? _tempProfileColor;
    private ProfileImageType _tempImageType;

    public AccountPage()
    {
        InitializeComponent();
        LoadProfile();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProfile();
    }

    private void LoadProfile()
    {
        var profile = _musicService.GetUserProfile();
        UsernameLabel.Text = profile.Username;
        SpotifySwitch.IsToggled = profile.SpotifyConnected;
        SoundCloudSwitch.IsToggled = profile.SoundCloudConnected;
        AppleMusicSwitch.IsToggled = profile.AppleMusicConnected;
        
        // Load profile picture
        UpdateProfileImageDisplay(profile, ProfileImageBorder, ProfileIconImage, ProfileCustomImage);
    }
    
    private void UpdateProfileImageDisplay(UserProfile profile, Border border, Image iconImage, Image customImage)
    {
        switch (profile.ImageType)
        {
            case ProfileImageType.PresetColor:
                border.BackgroundColor = Color.FromArgb(profile.ProfileColor ?? "#2A7FE6");
                iconImage.IsVisible = true;
                customImage.IsVisible = false;
                break;
                
            case ProfileImageType.CustomImage:
                if (!string.IsNullOrEmpty(profile.ProfileImagePath))
                {
                    border.BackgroundColor = Colors.Transparent;
                    customImage.Source = ImageSource.FromFile(profile.ProfileImagePath);
                    iconImage.IsVisible = false;
                    customImage.IsVisible = true;
                }
                break;
                
            case ProfileImageType.Default:
            default:
                border.BackgroundColor = Color.FromArgb("#2A7FE6");
                iconImage.IsVisible = true;
                customImage.IsVisible = false;
                break;
        }
    }
    
    private void UpdateMainProfileImageDisplay()
    {
        switch (_tempImageType)
        {
            case ProfileImageType.PresetColor:
                ProfileImageBorder.BackgroundColor = Color.FromArgb(_tempProfileColor ?? "#2A7FE6");
                ProfileIconImage.IsVisible = true;
                ProfileCustomImage.IsVisible = false;
                break;
                
            case ProfileImageType.CustomImage:
                if (!string.IsNullOrEmpty(_tempProfileImagePath))
                {
                    ProfileImageBorder.BackgroundColor = Colors.Transparent;
                    ProfileCustomImage.Source = ImageSource.FromFile(_tempProfileImagePath);
                    ProfileIconImage.IsVisible = false;
                    ProfileCustomImage.IsVisible = true;
                }
                break;
                
            case ProfileImageType.Default:
            default:
                ProfileImageBorder.BackgroundColor = Color.FromArgb("#2A7FE6");
                ProfileIconImage.IsVisible = true;
                ProfileCustomImage.IsVisible = false;
                break;
        }
    }

    private void OnEditClicked(object? sender, TappedEventArgs e)
    {
        var profile = _musicService.GetUserProfile();
        UsernameEntry.Text = profile.Username;
        
        // Load current profile picture settings into temp state
        _tempProfileImagePath = profile.ProfileImagePath;
        _tempProfileColor = profile.ProfileColor;
        _tempImageType = profile.ImageType;
        
        // Show edit overlay on profile image
        ProfileEditOverlay.IsVisible = true;
        
        EditUsernameSection.IsVisible = true;
        ValidateUsername(profile.Username);
    }

    private void OnCancelEditClicked(object? sender, EventArgs e)
    {
        EditUsernameSection.IsVisible = false;
        ProfileEditOverlay.IsVisible = false;
        UsernameErrorLabel.IsVisible = false;
        
        // Reset temp values and reload original profile image
        var profile = _musicService.GetUserProfile();
        UpdateProfileImageDisplay(profile, ProfileImageBorder, ProfileIconImage, ProfileCustomImage);
    }

    private void OnUsernameTextChanged(object? sender, TextChangedEventArgs e)
    {
        ValidateUsername(e.NewTextValue);
    }

    private void ValidateUsername(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            ShowUsernameError("Username cannot be empty");
            _isValidUsername = false;
            SaveButton.IsEnabled = false;
            return;
        }

        if (username.Length < 3)
        {
            ShowUsernameError("Username must be at least 3 characters");
            _isValidUsername = false;
            SaveButton.IsEnabled = false;
            return;
        }

        if (username.Length > 20)
        {
            ShowUsernameError("Username cannot exceed 20 characters");
            _isValidUsername = false;
            SaveButton.IsEnabled = false;
            return;
        }

        // Check for special characters (only allow letters, numbers, and underscore)
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
        {
            ShowUsernameError("Username can only contain letters, numbers, and underscore");
            _isValidUsername = false;
            SaveButton.IsEnabled = false;
            return;
        }

        // Valid username
        UsernameErrorLabel.IsVisible = false;
        _isValidUsername = true;
        SaveButton.IsEnabled = true;
    }

    private void ShowUsernameError(string message)
    {
        UsernameErrorLabel.Text = message;
        UsernameErrorLabel.IsVisible = true;
    }

    private async void OnSaveUsernameClicked(object? sender, EventArgs e)
    {
        if (!_isValidUsername)
            return;

        var profile = _musicService.GetUserProfile();
        profile.Username = UsernameEntry.Text ?? "Username";
        
        // Save profile picture settings
        profile.ProfileImagePath = _tempProfileImagePath;
        profile.ProfileColor = _tempProfileColor;
        profile.ImageType = _tempImageType;
        
        _musicService.UpdateUserProfile(profile);

        UsernameLabel.Text = profile.Username;
        
        // Update main profile picture display
        UpdateProfileImageDisplay(profile, ProfileImageBorder, ProfileIconImage, ProfileCustomImage);
        
        EditUsernameSection.IsVisible = false;
        ProfileEditOverlay.IsVisible = false;

        await DisplayAlert("Success", "Profile updated successfully!", "OK");
    }

    private void OnMusicServiceToggled(object? sender, ToggledEventArgs e)
    {
        var profile = _musicService.GetUserProfile();
        
        if (sender == SpotifySwitch)
            profile.SpotifyConnected = e.Value;
        else if (sender == SoundCloudSwitch)
            profile.SoundCloudConnected = e.Value;
        else if (sender == AppleMusicSwitch)
            profile.AppleMusicConnected = e.Value;
        
        _musicService.UpdateUserProfile(profile);
    }

    private async void OnFaqClicked(object? sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(new FaqPage());
    }
    
    private async void OnProfileImageEditClicked(object? sender, TappedEventArgs e)
    {
        var action = await DisplayActionSheet(
            "Change Profile Picture",
            "Cancel",
            null,
            "🔵 Blue",
            "🟣 Purple", 
            "🟢 Green",
            "🟠 Orange",
            "🔴 Red",
            "💗 Pink",
            "📷 Take Photo",
            "🖼️ Choose from Gallery");
        
        if (action == null || action == "Cancel")
            return;
            
        switch (action)
        {
            case "🔵 Blue":
                await SetPresetColor("#2A7FE6");
                break;
            case "🟣 Purple":
                await SetPresetColor("#9B59B6");
                break;
            case "🟢 Green":
                await SetPresetColor("#1DB954");
                break;
            case "🟠 Orange":
                await SetPresetColor("#E67E22");
                break;
            case "🔴 Red":
                await SetPresetColor("#E74C3C");
                break;
            case "💗 Pink":
                await SetPresetColor("#E91E63");
                break;
            case "📷 Take Photo":
                await TakePhoto();
                break;
            case "🖼️ Choose from Gallery":
                await PickFromGallery();
                break;
        }
    }
    
    private async Task SetPresetColor(string color)
    {
        _tempProfileColor = color;
        _tempProfileImagePath = null;
        _tempImageType = ProfileImageType.PresetColor;
        UpdateMainProfileImageDisplay();
        SaveButton.IsEnabled = true;
        await Task.CompletedTask;
    }
    
    private async Task PickFromGallery()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a profile picture"
            });
            
            if (result != null)
            {
                // Copy to app data directory for persistence
                var newFilePath = await CopyImageToAppData(result);
                
                _tempProfileImagePath = newFilePath;
                _tempProfileColor = null;
                _tempImageType = ProfileImageType.CustomImage;
                UpdateMainProfileImageDisplay();
                SaveButton.IsEnabled = true;
            }
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission Denied", "Please grant photo access permission in settings.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not pick image: {ex.Message}", "OK");
        }
    }
    
    private async Task TakePhoto()
    {
        try
        {
            if (!MediaPicker.IsCaptureSupported)
            {
                await DisplayAlert("Not Supported", "Camera capture is not supported on this device.", "OK");
                return;
            }
            
            var result = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a profile picture"
            });
            
            if (result != null)
            {
                // Copy to app data directory for persistence
                var newFilePath = await CopyImageToAppData(result);
                
                _tempProfileImagePath = newFilePath;
                _tempProfileColor = null;
                _tempImageType = ProfileImageType.CustomImage;
                UpdateMainProfileImageDisplay();
                SaveButton.IsEnabled = true;
            }
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission Denied", "Please grant camera permission in settings.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not take photo: {ex.Message}", "OK");
        }
    }
    
    private async Task<string> CopyImageToAppData(FileResult photo)
    {
        // Create a unique filename
        var fileName = $"profile_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(photo.FileName)}";
        var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        
        // Delete old profile image if exists
        var profile = _musicService.GetUserProfile();
        if (!string.IsNullOrEmpty(profile.ProfileImagePath) && File.Exists(profile.ProfileImagePath))
        {
            try { File.Delete(profile.ProfileImagePath); } catch { }
        }
        
        using var sourceStream = await photo.OpenReadAsync();
        using var targetStream = File.Create(targetPath);
        await sourceStream.CopyToAsync(targetStream);
        
        return targetPath;
    }
}