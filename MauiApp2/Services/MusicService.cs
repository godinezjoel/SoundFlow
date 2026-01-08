using MauiApp2.Models;

namespace MauiApp2.Services;

public class MusicService
{
    private static MusicService? _instance;
    public static MusicService Instance => _instance ??= new MusicService();

    private readonly List<Song> _allSongs;
    private readonly List<ListeningSession> _listeningSessions = new();
    private UserProfile _userProfile = new();

    public event EventHandler? LikedSongsChanged;
    public event EventHandler? StatsChanged;
    public event EventHandler? ProfileChanged;

    private MusicService()
    {
        _allSongs = GenerateSampleSongs();
        LoadUserProfile();
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
            TotalListeningTime = TimeSpan.FromTicks(_listeningSessions.Sum(s => s.Duration.Ticks)),
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

