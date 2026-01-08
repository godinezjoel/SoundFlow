using MauiApp2.Services;

namespace MauiApp2.Pages;

public partial class StatsPage : ContentPage
{
    private readonly MusicService _musicService = MusicService.Instance;

    public StatsPage()
    {
        InitializeComponent();
        _musicService.StatsChanged += OnStatsChanged;
    }

    private void OnStatsChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(LoadStats);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadStats();
    }

    private void LoadStats()
    {
        var stats = _musicService.GetStats();
        
        // Update Listening Time
        var hours = (int)stats.TotalListeningTime.TotalHours;
        var minutes = stats.TotalListeningTime.Minutes;
        ListeningTimeLabel.Text = $"{hours}h {minutes} min";
        
        // Update Songs Liked
        SongsLikedLabel.Text = stats.TotalLikedSongs.ToString();
        
        // Update Genre Chart
        UpdateGenreChart(stats.TopGenres);
    }

    private void UpdateGenreChart(List<Models.GenreStat> topGenres)
    {
        GenreBarsStack.Children.Clear();
        
        if (topGenres.Count == 0)
        {
            NoGenresLabel.IsVisible = true;
            GenreChartContainer.IsVisible = false;
            return;
        }
        
        NoGenresLabel.IsVisible = false;
        GenreChartContainer.IsVisible = true;
        
        // Sort by count for podium display (2nd, 1st, 3rd)
        var sortedGenres = topGenres.OrderByDescending(g => g.Count).Take(3).ToList();
        
        // Reorder for podium style: 2nd place, 1st place, 3rd place
        var podiumOrder = new List<Models.GenreStat>();
        if (sortedGenres.Count >= 2) podiumOrder.Add(sortedGenres[1]); // 2nd
        if (sortedGenres.Count >= 1) podiumOrder.Add(sortedGenres[0]); // 1st
        if (sortedGenres.Count >= 3) podiumOrder.Add(sortedGenres[2]); // 3rd
        
        var heights = new[] { 100, 140, 70 }; // Heights for 2nd, 1st, 3rd
        var index = 0;
        
        foreach (var genre in podiumOrder)
        {
            var height = heights[Math.Min(index, heights.Length - 1)];
            var bar = CreateGenreBar(genre.Genre, genre.Count, height);
            GenreBarsStack.Children.Add(bar);
            index++;
        }
    }

    private View CreateGenreBar(string genre, int count, int height)
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 5,
            VerticalOptions = LayoutOptions.End
        };
        
        // Genre label on top
        stack.Children.Add(new Label
        {
            Text = genre,
            FontFamily = "RobotoMedium",
            FontSize = 12,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center
        });
        
        // Bar with count
        var bar = new Border
        {
            BackgroundColor = Color.FromArgb("#2A7FE6"),
            WidthRequest = 70,
            HeightRequest = height,
            StrokeThickness = 0
        };
        bar.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle 
        { 
            CornerRadius = new Microsoft.Maui.CornerRadius(8, 8, 0, 0) 
        };
        
        bar.Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = count.ToString(),
                    FontFamily = "RobotoBold",
                    FontSize = 20,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Songs",
                    FontFamily = "RobotoMedium",
                    FontSize = 10,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
        
        stack.Children.Add(bar);
        
        return stack;
    }
}
