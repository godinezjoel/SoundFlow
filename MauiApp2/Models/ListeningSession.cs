namespace MauiApp2.Models;

public class ListeningSession
{
    public string SongId { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

