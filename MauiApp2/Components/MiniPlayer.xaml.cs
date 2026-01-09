using MauiApp2.Services;

namespace MauiApp2.Components;

public partial class MiniPlayer : ContentView
{
    private readonly PlaybackService _playbackService = PlaybackService.Instance;
    private bool _isDraggingSlider;

    public MiniPlayer()
    {
        InitializeComponent();
        
        // Subscribe to playback events
        _playbackService.PlaybackStateChanged += OnPlaybackStateChanged;
        _playbackService.ProgressUpdated += OnProgressUpdated;
        _playbackService.SongChanged += OnSongChanged;
        
        // Initial state
        UpdateVisibility();
        UpdateUI();
    }
    
    private void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateVisibility();
            UpdatePlayPauseButton();
        });
    }
    
    private void OnProgressUpdated(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_isDraggingSlider)
            {
                ProgressSlider.Value = _playbackService.Progress;
                CurrentTimeLabel.Text = _playbackService.GetCurrentTimeString();
            }
        });
    }
    
    private void OnSongChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateVisibility();
            UpdateUI();
        });
    }
    
    private void UpdateVisibility()
    {
        // Show mini player only when a song is selected (playing or paused)
        IsVisible = _playbackService.CurrentSong != null;
    }
    
    private void UpdateUI()
    {
        var song = _playbackService.CurrentSong;
        if (song != null)
        {
            SongTitleLabel.Text = song.Title;
            ArtistLabel.Text = song.Artist;
            DurationLabel.Text = _playbackService.GetDurationString();
            CurrentTimeLabel.Text = _playbackService.GetCurrentTimeString();
            ProgressSlider.Value = _playbackService.Progress;
        }
        UpdatePlayPauseButton();
    }
    
    private void UpdatePlayPauseButton()
    {
        PlayIcon.IsVisible = !_playbackService.IsPlaying;
        PauseIcon.IsVisible = _playbackService.IsPlaying;
    }
    
    private void OnPlayPauseClicked(object? sender, TappedEventArgs e)
    {
        _playbackService.TogglePlayPause();
    }
    
    private void OnProgressChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_isDraggingSlider)
        {
            // Update time label while dragging
            var song = _playbackService.CurrentSong;
            if (song != null)
            {
                var position = (e.NewValue / 100) * song.Duration.TotalSeconds;
                var currentTime = TimeSpan.FromSeconds(position);
                CurrentTimeLabel.Text = $"{(int)currentTime.TotalMinutes}:{currentTime.Seconds:D2}";
            }
        }
    }
    
    private void OnSliderDragStarted(object? sender, EventArgs e)
    {
        _isDraggingSlider = true;
    }
    
    private void OnSliderDragCompleted(object? sender, EventArgs e)
    {
        _isDraggingSlider = false;
        _playbackService.Seek(ProgressSlider.Value);
    }
}

