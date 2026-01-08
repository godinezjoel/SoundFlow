namespace MauiApp2.Models;

public class StatsData
{
    public TimeSpan TotalListeningTime { get; set; }
    public int TotalLikedSongs { get; set; }
    public List<GenreStat> TopGenres { get; set; } = new();
}

public class GenreStat
{
    public string Genre { get; set; } = string.Empty;
    public int Count { get; set; }
}

