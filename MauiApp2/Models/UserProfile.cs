namespace MauiApp2.Models;

public class UserProfile
{
    public string Username { get; set; } = "Username";
    public bool SpotifyConnected { get; set; }
    public bool SoundCloudConnected { get; set; }
    public bool AppleMusicConnected { get; set; }
    
    // Profile picture settings
    public string? ProfileImagePath { get; set; } // Path to custom image from gallery/camera
    public string? ProfileColor { get; set; } // Preset color (null = default blue)
    public ProfileImageType ImageType { get; set; } = ProfileImageType.Default;
}

public enum ProfileImageType
{
    Default,      // Standard blue placeholder
    PresetColor,  // One of the preset colors
    CustomImage   // Image from gallery or camera
}

