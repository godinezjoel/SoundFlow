using MauiApp2.Models;
using System.Text.Json;

namespace MauiApp2.Services;

public class MusicService
{
    private static MusicService? _instance;
    public static MusicService Instance => _instance ??= new MusicService();

    private readonly List<Song> _allSongs;
    private readonly List<ListeningSession> _listeningSessions = new();
    private UserProfile _userProfile = new();
    private TimeSpan _totalListeningTime = TimeSpan.Zero;

    public event EventHandler? LikedSongsChanged;
    public event EventHandler? StatsChanged;
    public event EventHandler? ProfileChanged;

    private MusicService()
    {
        _allSongs = GenerateSampleSongs();
        LoadUserProfile();
        LoadLikedSongs();
        LoadStats();
    }

    private void LoadUserProfile()
    {
        _userProfile = new UserProfile
        {
            Username = Preferences.Get("Username", "Username"),
            SpotifyConnected = Preferences.Get("SpotifyConnected", false),
            SoundCloudConnected = Preferences.Get("SoundCloudConnected", false),
            AppleMusicConnected = Preferences.Get("AppleMusicConnected", false),
            ProfileImagePath = Preferences.Get("ProfileImagePath", string.Empty),
            ProfileColor = Preferences.Get("ProfileColor", string.Empty),
            ImageType = (ProfileImageType)Preferences.Get("ProfileImageType", 0)
        };
        
        // Clean up empty strings
        if (string.IsNullOrEmpty(_userProfile.ProfileImagePath))
            _userProfile.ProfileImagePath = null;
        if (string.IsNullOrEmpty(_userProfile.ProfileColor))
            _userProfile.ProfileColor = null;
    }

    private void SaveUserProfile()
    {
        Preferences.Set("Username", _userProfile.Username);
        Preferences.Set("SpotifyConnected", _userProfile.SpotifyConnected);
        Preferences.Set("SoundCloudConnected", _userProfile.SoundCloudConnected);
        Preferences.Set("AppleMusicConnected", _userProfile.AppleMusicConnected);
        Preferences.Set("ProfileImagePath", _userProfile.ProfileImagePath ?? string.Empty);
        Preferences.Set("ProfileColor", _userProfile.ProfileColor ?? string.Empty);
        Preferences.Set("ProfileImageType", (int)_userProfile.ImageType);
    }
    
    private void LoadLikedSongs()
    {
        try
        {
            var likedSongIds = Preferences.Get("LikedSongIds", string.Empty);
            if (!string.IsNullOrEmpty(likedSongIds))
            {
                var ids = likedSongIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ids)
                {
                    var song = _allSongs.FirstOrDefault(s => s.Id == id);
                    if (song != null)
                    {
                        song.IsLiked = true;
                        // Try to load LikedAt time
                        var likedAtTicks = Preferences.Get($"LikedAt_{id}", 0L);
                        if (likedAtTicks > 0)
                        {
                            song.LikedAt = new DateTime(likedAtTicks);
                        }
                        else
                        {
                            song.LikedAt = DateTime.Now;
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore errors loading liked songs
        }
    }
    
    private void SaveLikedSongs()
    {
        try
        {
            var likedSongs = _allSongs.Where(s => s.IsLiked).ToList();
            var likedSongIds = string.Join(",", likedSongs.Select(s => s.Id));
            Preferences.Set("LikedSongIds", likedSongIds);
            
            // Save LikedAt times
            foreach (var song in likedSongs)
            {
                if (song.LikedAt.HasValue)
                {
                    Preferences.Set($"LikedAt_{song.Id}", song.LikedAt.Value.Ticks);
                }
            }
        }
        catch
        {
            // Ignore errors saving liked songs
        }
    }
    
    private void LoadStats()
    {
        try
        {
            // Load total listening time
            var totalTicks = Preferences.Get("TotalListeningTimeTicks", 0L);
            _totalListeningTime = TimeSpan.FromTicks(totalTicks);
            
            // Load listening sessions (only recent ones to save space)
            var sessionsJson = Preferences.Get("ListeningSessions", string.Empty);
            if (!string.IsNullOrEmpty(sessionsJson))
            {
                var sessions = JsonSerializer.Deserialize<List<ListeningSessionData>>(sessionsJson);
                if (sessions != null)
                {
                    foreach (var sessionData in sessions)
                    {
                        _listeningSessions.Add(new ListeningSession
                        {
                            SongId = sessionData.SongId,
                            Genre = sessionData.Genre,
                            Duration = TimeSpan.FromTicks(sessionData.DurationTicks),
                            Timestamp = new DateTime(sessionData.TimestampTicks)
                        });
                    }
                }
            }
        }
        catch
        {
            // Ignore errors loading stats
        }
    }
    
    private void SaveStats()
    {
        try
        {
            // Save total listening time
            Preferences.Set("TotalListeningTimeTicks", _totalListeningTime.Ticks);
            
            // Save recent listening sessions (last 100 to save space)
            var recentSessions = _listeningSessions
                .OrderByDescending(s => s.Timestamp)
                .Take(100)
                .Select(s => new ListeningSessionData
                {
                    SongId = s.SongId,
                    Genre = s.Genre,
                    DurationTicks = s.Duration.Ticks,
                    TimestampTicks = s.Timestamp.Ticks
                })
                .ToList();
            
            var sessionsJson = JsonSerializer.Serialize(recentSessions);
            Preferences.Set("ListeningSessions", sessionsJson);
        }
        catch
        {
            // Ignore errors saving stats
        }
    }
    
    // Helper class for JSON serialization
    private class ListeningSessionData
    {
        public string SongId { get; set; } = "";
        public string Genre { get; set; } = "";
        public long DurationTicks { get; set; }
        public long TimestampTicks { get; set; }
    }

    private List<Song> GenerateSampleSongs()
    {
        return new List<Song>
        {
            // Hip Hop Songs
            new Song
            {
                Id = "1",
                Title = "Street Dreams",
                Artist = "MC Flow",
                Genre = "Hip Hop",
                Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(24))
            },
            new Song
            {
                Id = "2",
                Title = "City Lights",
                Artist = "Urban Beat",
                Genre = "Hip Hop",
                Duration = TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(12))
            },
            
            // Rap Songs
            new Song
            {
                Id = "3",
                Title = "Real Talk",
                Artist = "Lyric Master",
                Genre = "Rap",
                Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(45))
            },
            new Song
            {
                Id = "4",
                Title = "Underground",
                Artist = "Verbal Assassin",
                Genre = "Rap",
                Duration = TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(58))
            },
            
            // Jazz Songs
            new Song
            {
                Id = "5",
                Title = "Midnight Blues",
                Artist = "Jazz Quartet",
                Genre = "Jazz",
                Duration = TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(30))
            },
            new Song
            {
                Id = "6",
                Title = "Smooth Evening",
                Artist = "The Sax Players",
                Genre = "Jazz",
                Duration = TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(45))
            },
            
            // Rock Songs
            new Song
            {
                Id = "7",
                Title = "Electric Thunder",
                Artist = "Rock Revolution",
                Genre = "Rock",
                Duration = TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(18))
            },
            new Song
            {
                Id = "8",
                Title = "Highway Rider",
                Artist = "The Amplifiers",
                Genre = "Rock",
                Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(52))
            },
            
            // Pop Songs
            new Song
            {
                Id = "9",
                Title = "Summer Vibes",
                Artist = "Pop Star",
                Genre = "Pop",
                Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(15))
            },
            new Song
            {
                Id = "10",
                Title = "Dance Tonight",
                Artist = "Melody Queen",
                Genre = "Pop",
                Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(28))
            }
        };
    }

    public List<Song> GetAllSongs() => new(_allSongs);

    public List<Song> GetSongsByGenres(List<string> genres)
    {
        if (genres == null || genres.Count == 0)
            return new List<Song>(_allSongs);

        return _allSongs.Where(s => genres.Contains(s.Genre)).ToList();
    }

    public List<Song> GetLikedSongs()
    {
        return _allSongs.Where(s => s.IsLiked).OrderByDescending(s => s.LikedAt).ToList();
    }

    public List<Song> GetLikedSongsByGenre(string genre)
    {
        if (string.IsNullOrEmpty(genre) || genre == "All")
            return GetLikedSongs();

        return _allSongs.Where(s => s.IsLiked && s.Genre == genre)
                        .OrderByDescending(s => s.LikedAt).ToList();
    }

    public void ToggleLike(string songId)
    {
        var song = _allSongs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            song.IsLiked = !song.IsLiked;
            song.LikedAt = song.IsLiked ? DateTime.Now : null;
            SaveLikedSongs();
            LikedSongsChanged?.Invoke(this, EventArgs.Empty);
            StatsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetLike(string songId, bool liked)
    {
        var song = _allSongs.FirstOrDefault(s => s.Id == songId);
        if (song != null && song.IsLiked != liked)
        {
            song.IsLiked = liked;
            song.LikedAt = liked ? DateTime.Now : null;
            SaveLikedSongs();
            LikedSongsChanged?.Invoke(this, EventArgs.Empty);
            StatsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddListeningSession(string songId, TimeSpan duration)
    {
        var song = _allSongs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            _listeningSessions.Add(new ListeningSession
            {
                SongId = songId,
                Genre = song.Genre,
                Duration = duration,
                Timestamp = DateTime.Now
            });
            
            // Update total listening time
            _totalListeningTime = _totalListeningTime.Add(duration);
            
            // Save stats
            SaveStats();
            StatsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public StatsData GetStats()
    {
        var likedSongs = _allSongs.Where(s => s.IsLiked).ToList();
        
        var genreStats = likedSongs
            .GroupBy(s => s.Genre)
            .Select(g => new GenreStat { Genre = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(3)
            .ToList();

        return new StatsData
        {
            TotalListeningTime = _totalListeningTime,
            TotalLikedSongs = likedSongs.Count,
            TopGenres = genreStats
        };
    }

    public List<string> GetAllGenres()
    {
        return _allSongs.Select(s => s.Genre).Distinct().OrderBy(g => g).ToList();
    }

    public UserProfile GetUserProfile() => _userProfile;

    public void UpdateUserProfile(UserProfile profile)
    {
        _userProfile = profile;
        SaveUserProfile();
        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }
}

