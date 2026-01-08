using MauiApp2.Models;
using MauiApp2.Services;
using System.Collections.ObjectModel;

namespace MauiApp2.Pages;

public partial class LikedSongsPage : ContentPage
{
    private readonly MusicService _musicService = MusicService.Instance;
    private ObservableCollection<Song> _likedSongs = new();
    private string _selectedGenre = "All";
    public bool HasSongs => _likedSongs.Count > 0;

    public LikedSongsPage()
    {
        InitializeComponent();
        BindingContext = this;
        _musicService.LikedSongsChanged += OnLikedSongsChanged;
    }

    private void OnLikedSongsChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(LoadLikedSongs);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadLikedSongs();
    }

    private void LoadLikedSongs()
    {
        var songs = _selectedGenre == "All" 
            ? _musicService.GetLikedSongs() 
            : _musicService.GetLikedSongsByGenre(_selectedGenre);
        
        _likedSongs = new ObservableCollection<Song>(songs);
        LikedSongsCollection.ItemsSource = _likedSongs;
        
        // Update UI visibility
        var hasSongs = _likedSongs.Count > 0;
        EmptyState.IsVisible = !hasSongs;
        LikedSongsCollection.IsVisible = hasSongs;
        
        // Update song count
        SongCountLabel.Text = $"{_likedSongs.Count} Songs";
        
        OnPropertyChanged(nameof(HasSongs));
    }

    private async void OnSearchClicked(object? sender, TappedEventArgs e)
    {
        var searchTerm = await DisplayPromptAsync(
            "Search", 
            "Enter song title or artist name:",
            "Search",
            "Cancel");
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var filteredSongs = _musicService.GetLikedSongs()
                .Where(s => s.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           s.Artist.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            _likedSongs = new ObservableCollection<Song>(filteredSongs);
            LikedSongsCollection.ItemsSource = _likedSongs;
            
            var hasSongs = _likedSongs.Count > 0;
            EmptyState.IsVisible = !hasSongs;
            LikedSongsCollection.IsVisible = hasSongs;
        }
    }

    private async void OnGenreFilterClicked(object? sender, TappedEventArgs e)
    {
        var genres = _musicService.GetAllGenres();
        var selectedGenre = await DisplayActionSheet(
            "Filter by Genre",
            "Cancel",
            null,
            new[] { "All" }.Concat(genres).ToArray());

        if (selectedGenre == "Cancel" || string.IsNullOrEmpty(selectedGenre))
            return;

        _selectedGenre = selectedGenre;
        GenreFilterLabel.Text = selectedGenre == "All" ? "Genre" : selectedGenre;
        LoadLikedSongs();
    }

    private void OnPlaySongClicked(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string songId)
        {
            // Show that song is playing (simulated)
            var song = _likedSongs.FirstOrDefault(s => s.Id == songId);
            if (song != null)
            {
                DisplayAlert("Now Playing", $"{song.Title} by {song.Artist}", "OK");
                _musicService.AddListeningSession(songId, song.Duration);
            }
        }
    }

    private void OnUnlikeClicked(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string songId)
        {
            _musicService.SetLike(songId, false);
            LoadLikedSongs();
        }
    }
}
