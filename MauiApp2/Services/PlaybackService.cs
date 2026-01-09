using MauiApp2.Models;

namespace MauiApp2.Services;

public class PlaybackService
{
    private static PlaybackService? _instance;
    public static PlaybackService Instance => _instance ??= new PlaybackService();
    
    private IDispatcherTimer? _playbackTimer;
    private DateTime _playStartTime;
    
    // Playback state
    public Song? CurrentSong { get; private set; }
    public bool IsPlaying { get; private set; }
    public double CurrentPosition { get; private set; }
    public double Progress => CurrentSong != null ? (CurrentPosition / CurrentSong.Duration.TotalSeconds) * 100 : 0;
    
    // Events
    public event EventHandler? PlaybackStateChanged;
    public event EventHandler? ProgressUpdated;
    public event EventHandler? SongChanged;
    
    private PlaybackService()
    {
    }
    
    public void Initialize(IDispatcher dispatcher)
    {
        if (_playbackTimer == null)
        {
            _playbackTimer = dispatcher.CreateTimer();
            _playbackTimer.Interval = TimeSpan.FromMilliseconds(100);
            _playbackTimer.Tick += OnPlaybackTimerTick;
        }
    }
    
    private void OnPlaybackTimerTick(object? sender, EventArgs e)
    {
        if (IsPlaying && CurrentSong != null)
        {
            var elapsed = DateTime.Now - _playStartTime;
            CurrentPosition += elapsed.TotalSeconds;
            _playStartTime = DateTime.Now;
            
            if (CurrentPosition >= CurrentSong.Duration.TotalSeconds)
            {
                CurrentPosition = CurrentSong.Duration.TotalSeconds;
                Stop();
                return;
            }
            
            ProgressUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public void PlaySong(Song song)
    {
        var isNewSong = CurrentSong?.Id != song.Id;
        
        CurrentSong = song;
        CurrentPosition = 0;
        IsPlaying = true;
        _playStartTime = DateTime.Now;
        _playbackTimer?.Start();
        
        if (isNewSong)
        {
            SongChanged?.Invoke(this, EventArgs.Empty);
        }
        PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void Play()
    {
        if (CurrentSong == null) return;
        
        IsPlaying = true;
        _playStartTime = DateTime.Now;
        _playbackTimer?.Start();
        PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void Pause()
    {
        IsPlaying = false;
        _playbackTimer?.Stop();
        PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void TogglePlayPause()
    {
        if (IsPlaying)
            Pause();
        else
            Play();
    }
    
    public void Stop()
    {
        IsPlaying = false;
        CurrentPosition = 0;
        _playbackTimer?.Stop();
        PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        ProgressUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public void Seek(double progressPercent)
    {
        if (CurrentSong == null) return;
        
        CurrentPosition = (progressPercent / 100) * CurrentSong.Duration.TotalSeconds;
        _playStartTime = DateTime.Now;
        ProgressUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public void ClearSong()
    {
        Stop();
        CurrentSong = null;
        SongChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public string GetCurrentTimeString()
    {
        var currentTime = TimeSpan.FromSeconds(CurrentPosition);
        return $"{(int)currentTime.TotalMinutes}:{currentTime.Seconds:D2}";
    }
    
    public string GetDurationString()
    {
        if (CurrentSong == null) return "0:00";
        return $"{(int)CurrentSong.Duration.TotalMinutes}:{CurrentSong.Duration.Seconds:D2}";
    }
}

