﻿using MauiApp2.Models;
using MauiApp2.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp2.Pages;

public partial class HomePage : ContentPage
{
    private readonly MusicService _musicService = MusicService.Instance;
    private readonly PlaybackService _playbackService = PlaybackService.Instance;
    private ObservableCollection<Song> _songs = new();
    private List<string> _selectedGenres = new();
    private Song? _currentSong;
    private bool _isPlaying;
    private bool _isDraggingSlider;
    private IDispatcherTimer? _playbackTimer;
    private double _currentPosition;
    private DateTime _playStartTime;
    
    // References to UI elements in the current carousel item
    private Slider? _currentSlider;
    private Label? _currentTimeLabel;
    private Microsoft.Maui.Controls.Shapes.Path? _currentPlayIcon;
    private Microsoft.Maui.Controls.Shapes.Path? _currentPauseIcon;

    public HomePage()
    {
        InitializeComponent();
        LoadSongs();
        SetupPlaybackTimer();
        
        // Initialize PlaybackService with dispatcher
        _playbackService.Initialize(Dispatcher);
    }

    private void SetupPlaybackTimer()
    {
        _playbackTimer = Dispatcher.CreateTimer();
        _playbackTimer.Interval = TimeSpan.FromMilliseconds(100);
        _playbackTimer.Tick += OnPlaybackTimerTick;
    }

    private void OnPlaybackTimerTick(object? sender, EventArgs e)
    {
        if (_isPlaying && _currentSong != null && !_isDraggingSlider)
        {
            var elapsed = DateTime.Now - _playStartTime;
            _currentPosition += elapsed.TotalSeconds;
            _playStartTime = DateTime.Now;

            var progress = (_currentPosition / _currentSong.Duration.TotalSeconds) * 100;
            
            if (progress >= 100)
            {
                progress = 100;
                _currentPosition = _currentSong.Duration.TotalSeconds;
                StopPlayback();
                // Auto-advance to next song
                if (SongCarousel.Position < _songs.Count - 1)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SongCarousel.Position++;
                    });
                }
                return;
            }

            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateProgressUI(progress);
            });
        }
    }

    private void UpdateProgressUI(double progress)
    {
        if (_currentSong == null) return;
        
        // Update the current time label
        var currentTime = TimeSpan.FromSeconds(_currentPosition);
        var timeString = $"{(int)currentTime.TotalMinutes}:{currentTime.Seconds:D2}";
        
        // Find and update the UI elements in the current carousel item
        FindAndUpdateCurrentItemUI(progress, timeString);
    }
    
    private void FindAndUpdateCurrentItemUI(double progress, string timeString)
    {
        try
        {
            // Try to find the slider and label in the current visible item
            if (_currentSlider != null && !_isDraggingSlider)
            {
                _currentSlider.Value = progress;
            }
            
            if (_currentTimeLabel != null)
            {
                _currentTimeLabel.Text = timeString;
            }
        }
        catch
        {
            // Ignore errors when UI elements are not found
        }
    }

    private void LoadSongs()
    {
        var songs = _selectedGenres.Count > 0 
            ? _musicService.GetSongsByGenres(_selectedGenres) 
            : _musicService.GetAllSongs();
        
        _songs = new ObservableCollection<Song>(songs);
        SongCarousel.ItemsSource = _songs;
        
        if (_songs.Count > 0)
        {
            _currentSong = _songs[0];
        }
    }

    private async void OnFilterClicked(object? sender, TappedEventArgs e)
    {
        var genres = _musicService.GetAllGenres();
        var selectedGenre = await DisplayActionSheet(
            "Select Genre", 
            "Cancel", 
            null, 
            new[] { "All Genres" }.Concat(genres).ToArray());

        if (selectedGenre == "Cancel" || string.IsNullOrEmpty(selectedGenre))
            return;

        if (selectedGenre == "All Genres")
        {
            _selectedGenres.Clear();
            FilterLabel.Text = "All Genres";
        }
        else
        {
            _selectedGenres = new List<string> { selectedGenre };
            FilterLabel.Text = selectedGenre;
        }

        StopPlayback();
        LoadSongs();
    }

    private void OnCurrentSongChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        // Stop previous song
        StopPlayback();
        
        // Reset UI element references (they will be re-cached when user interacts)
        _currentSlider = null;
        _currentTimeLabel = null;
        _currentPlayIcon = null;
        _currentPauseIcon = null;
        
        // Track listening time for previous song
        if (e.PreviousItem is Song previousSong && _currentPosition > 0)
        {
            _musicService.AddListeningSession(previousSong.Id, TimeSpan.FromSeconds(_currentPosition));
        }

        // Set new current song
        if (e.CurrentItem is Song newSong)
        {
            _currentSong = newSong;
            _currentPosition = 0;
        }
        
        // Cache UI elements from the new carousel item after a short delay
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(100); // Wait for visual tree to update
            CacheCurrentCarouselItemUI();
        });
    }
    
    private void CacheCurrentCarouselItemUI()
    {
        try
        {
            // Find the current visible item's UI elements
            // The CarouselView's currently displayed item should be in the visual tree
            FindUIElementsInVisualTree(SongCarousel);
        }
        catch
        {
            // Ignore errors when UI elements are not found yet
        }
    }
    
    private void FindUIElementsInVisualTree(Element element)
    {
        if (_currentSlider != null && _currentPlayIcon != null) return; // Already found
        
        if (element is Slider slider && slider.Parent is Grid grid)
        {
            // Check if this slider belongs to the current visible item
            _currentSlider = slider;
            
            // Find time label in same grid
            foreach (var child in grid.Children)
            {
                if (child is Label label && Grid.GetColumn(label) == 0)
                {
                    _currentTimeLabel = label;
                    break;
                }
            }
        }
        
        if (element is Microsoft.Maui.Controls.Shapes.Path path)
        {
            // Identify play/pause icons by their data
            var pathData = path.Data?.ToString() ?? "";
            if (pathData.Contains("8.6 5.2") || pathData.Contains("M8.6"))
            {
                _currentPlayIcon = path;
            }
            else if (pathData.Contains("M8 5a2") || pathData.Contains("8 5a2"))
            {
                _currentPauseIcon = path;
            }
        }
        
        // Recurse through children
        if (element is IVisualTreeElement visualTreeElement)
        {
            foreach (var child in visualTreeElement.GetVisualChildren())
            {
                if (child is Element childElement)
                {
                    FindUIElementsInVisualTree(childElement);
                }
            }
        }
    }

    private void OnLikeClicked(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string songId)
        {
            _musicService.ToggleLike(songId);
            
            // Refresh the current item to update UI
            var song = _songs.FirstOrDefault(s => s.Id == songId);
            if (song != null)
            {
                var index = _songs.IndexOf(song);
                _songs[index] = song; // Trigger UI update
                SongCarousel.ItemsSource = null;
                SongCarousel.ItemsSource = _songs;
                SongCarousel.Position = index;
            }
        }
    }

    private void OnPlayPauseClicked(object? sender, TappedEventArgs e)
    {
        // Cache the play/pause icons from the sender (Border contains a Grid with Path icons)
        if (sender is Border border)
        {
            // Find the icons inside the Border's Grid
            if (border.Content is Grid iconGrid)
            {
                foreach (var child in iconGrid.Children)
                {
                    if (child is Microsoft.Maui.Controls.Shapes.Path path)
                    {
                        // Identify by visibility - PlayIcon is visible by default
                        if (path.IsVisible)
                            _currentPlayIcon = path;
                        else
                            _currentPauseIcon = path;
                    }
                }
            }
            
            // Also try to find slider and time label in the parent grid
            if (border.Parent is Grid parentGrid)
            {
                CacheUIElementsFromParent(parentGrid);
            }
        }
        
        if (_isPlaying)
        {
            PausePlayback();
        }
        else
        {
            // Stop any song playing in MiniPlayer when starting playback on HomePage
            _playbackService.ClearSong();
            StartPlayback();
        }
    }
    
    private void CacheUIElementsFromParent(Grid parentGrid)
    {
        // Navigate up to the main item grid (the DataTemplate root)
        // parentGrid is the Grid containing the Play/Pause button (Row 4)
        // We need to go up to find the DataTemplate root Grid
        var itemGrid = parentGrid.Parent?.Parent as Grid;
        if (itemGrid == null)
        {
            itemGrid = parentGrid.Parent as Grid;
        }
        if (itemGrid == null) return;
        
        // Search all children recursively for Slider and CurrentTimeLabel
        FindUIElementsRecursive(itemGrid);
    }
    
    private void FindUIElementsRecursive(Element element)
    {
        if (element is Slider slider)
        {
            _currentSlider = slider;
            return;
        }
        
        if (element is Grid grid)
        {
            // Check if this grid contains a Slider (progress bar grid)
            bool hasSlider = false;
            foreach (var child in grid.Children)
            {
                if (child is Slider s)
                {
                    _currentSlider = s;
                    hasSlider = true;
                }
            }
            
            // If this grid has a slider, find the time label at column 0
            if (hasSlider)
            {
                foreach (var child in grid.Children)
                {
                    if (child is Label label && Grid.GetColumn(label) == 0)
                    {
                        _currentTimeLabel = label;
                        break;
                    }
                }
            }
            
            // Continue searching in children
            foreach (var child in grid.Children)
            {
                if (child is Element e)
                    FindUIElementsRecursive(e);
            }
        }
        else if (element is Border border && border.Content is Element content)
        {
            FindUIElementsRecursive(content);
        }
    }
    
    private void StartPlayback()
    {
        _isPlaying = true;
        _playStartTime = DateTime.Now;
        _playbackTimer?.Start();
        UpdatePlayPauseButton();
    }

    private void PausePlayback()
    {
        _isPlaying = false;
        _playbackTimer?.Stop();
        UpdatePlayPauseButton();
    }

    private void StopPlayback()
    {
        _isPlaying = false;
        _playbackTimer?.Stop();
        _currentPosition = 0;
        UpdatePlayPauseButton();
        
        // Reset slider and time label
        if (_currentSlider != null)
        {
            _currentSlider.Value = 0;
        }
        if (_currentTimeLabel != null)
        {
            _currentTimeLabel.Text = "0:00";
        }
    }

    private void UpdatePlayPauseButton()
    {
        if (_currentPlayIcon != null && _currentPauseIcon != null)
        {
            _currentPlayIcon.IsVisible = !_isPlaying;
            _currentPauseIcon.IsVisible = _isPlaying;
        }
    }

    private void OnProgressChanged(object? sender, ValueChangedEventArgs e)
    {
        // Cache the slider reference
        if (sender is Slider slider)
        {
            _currentSlider = slider;
            
            // Try to find the time label in the same grid
            if (slider.Parent is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is Label label && Grid.GetColumn(label) == 0)
                    {
                        // Current time label is at column 0
                        _currentTimeLabel = label;
                        break;
                    }
                }
            }
        }
        
        if (_isDraggingSlider && _currentSong != null)
        {
            _currentPosition = (e.NewValue / 100) * _currentSong.Duration.TotalSeconds;
            
            // Update time label while dragging
            var currentTime = TimeSpan.FromSeconds(_currentPosition);
            var timeString = $"{(int)currentTime.TotalMinutes}:{currentTime.Seconds:D2}";
            if (_currentTimeLabel != null)
            {
                _currentTimeLabel.Text = timeString;
            }
        }
    }

    private void OnSliderDragStarted(object? sender, EventArgs e)
    {
        _isDraggingSlider = true;
        
        // Cache slider and time label reference
        if (sender is Slider slider)
        {
            _currentSlider = slider;
            
            // Find the time label in the same grid
            if (slider.Parent is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is Label label && Grid.GetColumn(label) == 0)
                    {
                        _currentTimeLabel = label;
                        break;
                    }
                }
            }
        }
    }

    private void OnSliderDragCompleted(object? sender, EventArgs e)
    {
        _isDraggingSlider = false;
        if (_isPlaying)
        {
            _playStartTime = DateTime.Now;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh songs in case likes changed from other pages
        var currentPosition = SongCarousel.Position;
        LoadSongs();
        if (currentPosition < _songs.Count)
        {
            SongCarousel.Position = currentPosition;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Track listening time when leaving page
        if (_currentSong != null && _currentPosition > 0)
        {
            _musicService.AddListeningSession(_currentSong.Id, TimeSpan.FromSeconds(_currentPosition));
        }
        PausePlayback();
    }
}
