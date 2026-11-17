
using Aura.Models;
using Aura.Services;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;

namespace Aura.Views;

public partial class MoodPage : ContentPage
{
    private readonly ITunesMusicService _musicService = new();
    private readonly ArtService _artService = new();

    private string _selectedMood = "";
    private MusicTrackModel? _currentTrack;
    private ArtworkModel? _currentArtwork;

    private readonly Dictionary<string, MoodConfig> _moods = new()
    {
        ["Feliz"] = new MoodConfig
        {
            MusicSearch = "happy feel good",
            ArtSearch = "flowers",
            Messages =
            [
                "Que bom que você está vivendo um momento feliz! Aproveite essa energia e permita que ela ilumine seu dia.",
                "A felicidade também mora nos pequenos detalhes. Permita-se aproveitar cada um deles.",
                "Guarde um pouco dessa energia para os dias em que precisar lembrar de como é bom sorrir."
            ]
        },

        ["Tranquilo"] = new MoodConfig
        {
            MusicSearch = "calm relaxing instrumental",
            ArtSearch = "landscape",
            Messages =
            [
                "Aproveite essa sensação de calma. Respire, desacelere e valorize o presente.",
                "Você não precisa estar em movimento o tempo todo. Às vezes, descansar também é avançar.",
                "Encontre conforto no silêncio e aproveite a tranquilidade deste instante."
            ]
        },

        ["Animado"] = new MoodConfig
        {
            MusicSearch = "upbeat dance pop",
            ArtSearch = "colorful",
            Messages =
            [
                "Sua energia está em alta! Que tal aproveitar esse entusiasmo para experimentar algo novo?",
                "Hoje pode ser um ótimo dia para transformar ideias em experiências.",
                "Continue explorando, criando e descobrindo. Sua curiosidade pode levar você longe."
            ]
        },

        ["Triste"] = new MoodConfig
        {
            MusicSearch = "soft emotional acoustic",
            ArtSearch = "flowers",
            Messages =
            [
                "Tudo bem não estar bem o tempo todo. Respeite seu próprio ritmo e lembre-se de que você merece carinho e cuidado.",
                "Não precisa resolver tudo hoje. Permita-se sentir e dê a si mesmo o tempo necessário.",
                "Mesmo os dias difíceis fazem parte da sua história. Seja gentil consigo mesmo."
            ]
        },

        ["Ansioso"] = new MoodConfig
        {
            MusicSearch = "peaceful ambient piano",
            ArtSearch = "nature",
            Messages =
            [
                "Tente fazer uma pausa. Respire devagar e concentre-se em uma coisa de cada vez.",
                "Você não precisa resolver tudo agora. Dê um passo de cada vez.",
                "Volte sua atenção para o presente. Um momento tranquilo já pode fazer diferença."
            ]
        },

        ["Reflexivo"] = new MoodConfig
        {
            MusicSearch = "cinematic piano instrumental",
            ArtSearch = "abstract",
            Messages =
            [
                "Às vezes, olhar para dentro é o começo de uma grande descoberta. Permita-se pensar, imaginar e criar.",
                "Algumas respostas aparecem quando damos espaço para novas perguntas.",
                "Aproveite este momento para observar seus pensamentos e descobrir novas perspectivas."
            ]
        }
    };

    private readonly Dictionary<string, string> _lastMessages = new();

    public MoodPage()
    {
        InitializeComponent();
    }

    private async void OnMoodSelected(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        _selectedMood = button.Text;

        await LoadExperienceAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_selectedMood))
            await LoadExperienceAsync();
    }

    private async Task LoadExperienceAsync()
    {
        if (!_moods.TryGetValue(_selectedMood, out var config))
            return;

        ExperienceLayout.IsVisible = true;

        SelectedMoodLabel.Text =
            $"Você está se sentindo {_selectedMood.ToLower()}";

        MoodMessageLabel.Text = GetNewMessage(config.Messages);

        RefreshButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        LoadingLabel.IsVisible = true;
        LoadingLabel.Text = "Buscando músicas e obras para você...";

        // Limpa a experiência anterior.
        MoodAudioPlayer.Source = null;
        PreviewStatusLabel.Text = "Buscando uma música...";
        SongSuggestionLabel.Text = "Carregando...";
        SongArtistLabel.Text = "";
        SongCoverImage.Source = null;
        ExternalTrackButton.IsVisible = false;

        ArtworkImage.Source = null;
        ArtSuggestionLabel.Text = "Buscando uma obra...";
        ArtArtistLabel.Text = "";
        ArtworkDetailsButton.IsVisible = false;

        _currentTrack = null;
        _currentArtwork = null;

        try
        {
            await LoadMusicAsync(config.MusicSearch);
        }
        catch (Exception ex)
        {
            SongSuggestionLabel.Text = "Não foi possível carregar uma música.";
            PreviewStatusLabel.Text = "Verifique sua conexão e tente novamente.";
            System.Diagnostics.Debug.WriteLine($"Erro no Mood/iTunes: {ex}");
        }

        try
        {
            await LoadArtworkAsync(config.ArtSearch);
        }
        catch (Exception ex)
        {
            ArtSuggestionLabel.Text = "Não foi possível carregar uma obra.";
            System.Diagnostics.Debug.WriteLine($"Erro no Mood/The Met: {ex}");
        }

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        LoadingLabel.IsVisible = false;
        RefreshButton.IsEnabled = true;
    }

    private async Task LoadMusicAsync(string searchTerm)
    {
        var tracks = await _musicService.SearchTracksAsync(searchTerm);

        if (tracks.Count == 0)
        {
            SongSuggestionLabel.Text = "Nenhuma música encontrada.";
            PreviewStatusLabel.Text = "Tente gerar uma nova combinação.";
            return;
        }

        var candidates = tracks
            .Where(track => track.HasPreview)
            .ToList();

        if (candidates.Count == 0)
            candidates = tracks;

        var differentTracks = candidates
            .Where(track => track.TrackId != _currentTrack?.TrackId)
            .ToList();

        if (differentTracks.Count > 0)
            candidates = differentTracks;

        var selectedTrack = candidates[Random.Shared.Next(candidates.Count)];

        DisplayTrack(selectedTrack);
    }

    private void DisplayTrack(MusicTrackModel track)
    {
        _currentTrack = track;

        SongSuggestionLabel.Text = track.Title;
        SongArtistLabel.Text = track.Artist;

        SongCoverImage.Source = string.IsNullOrWhiteSpace(track.ArtworkUrl)
            ? null
            : ImageSource.FromUri(new Uri(track.ArtworkUrl));

        ExternalTrackButton.IsVisible =
            !string.IsNullOrWhiteSpace(track.StoreUrl);

        MoodAudioPlayer.Source = null;

        if (track.HasPreview &&
            Uri.TryCreate(track.PreviewUrl, UriKind.Absolute, out Uri? previewUri))
        {
            PreviewStatusLabel.Text = "Carregando prévia da música...";
            MoodAudioPlayer.Source = MediaSource.FromUri(previewUri);
        }
        else
        {
            PreviewStatusLabel.Text =
                "Esta música não possui prévia disponível.";
        }
    }

    private async Task LoadArtworkAsync(string searchTerm)
    {
        var artworks = await _artService.GetArtworksAsync(searchTerm);

        if (artworks.Count == 0)
        {
            ArtSuggestionLabel.Text = "Nenhuma obra encontrada.";
            ArtArtistLabel.Text = "Tente gerar uma nova combinação.";
            return;
        }

        var differentArtworks = artworks
            .Where(artwork =>
                artwork.ObjectId != _currentArtwork?.ObjectId)
            .ToList();

        if (differentArtworks.Count > 0)
            artworks = differentArtworks;

        var selectedArtwork =
            artworks[Random.Shared.Next(artworks.Count)];

        DisplayArtwork(selectedArtwork);
    }

    private void DisplayArtwork(ArtworkModel artwork)
    {
        _currentArtwork = artwork;

        ArtworkImage.Source = ImageSource.FromUri(
            new Uri(artwork.ImageUrl));

        ArtSuggestionLabel.Text = artwork.Title;
        ArtArtistLabel.Text = string.IsNullOrWhiteSpace(artwork.Artist)
            ? "Artista não informado"
            : artwork.Artist;

        ArtworkDetailsButton.IsVisible = true;
    }

    private string GetNewMessage(List<string> messages)
    {
        var alternatives = messages
            .Where(message =>
                message != _lastMessages.GetValueOrDefault(_selectedMood))
            .ToList();

        if (alternatives.Count == 0)
            alternatives = messages;

        string selectedMessage =
            alternatives[Random.Shared.Next(alternatives.Count)];

        _lastMessages[_selectedMood] = selectedMessage;

        return selectedMessage;
    }

    private void OnMoodMediaOpened(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PreviewStatusLabel.Text = "Prévia disponível para reprodução.";
        });
    }

    private void OnMoodMediaFailed(
        object? sender,
        MediaFailedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PreviewStatusLabel.Text =
                "Não foi possível reproduzir esta prévia.";
        });

        System.Diagnostics.Debug.WriteLine(
            $"Falha no player do Mood: {e.ErrorMessage}");
    }

    private async void OnExternalTrackClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_currentTrack?.StoreUrl))
            await Launcher.OpenAsync(_currentTrack.StoreUrl);
    }

    private async void OnArtworkDetailsClicked(object sender, EventArgs e)
    {
        if (_currentArtwork == null)
            return;

        await Shell.Current.GoToAsync(
            nameof(ArtworkDetailsPage),
            true,
            new Dictionary<string, object>
            {
                { "Artwork", _currentArtwork }
            });
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        MoodAudioPlayer.Source = null;

        _currentTrack = null;
        _currentArtwork = null;
        _selectedMood = "";

        ExperienceLayout.IsVisible = false;
    }

    private class MoodConfig
    {
        public string MusicSearch { get; set; } = "";
        public string ArtSearch { get; set; } = "";
        public List<string> Messages { get; set; } = [];
    }
}
