namespace MauiApp2.Models;

public class Song
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string CoverImage { get; set; } = "dotnet_bot.png";
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(3);
    public bool IsLiked { get; set; }
    public DateTime? LikedAt { get; set; }
}

