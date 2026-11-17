
using Aura.Models;
using Aura.Services;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;

namespace Aura.Views;

public partial class DiscoveryPage : ContentPage
{
    private readonly WeatherService _weatherService = new();
    private readonly ITunesMusicService _musicService = new();
    private readonly ArtService _artService = new();

    private readonly HashSet<string> _selectedMusicGenres =
        new(StringComparer.OrdinalIgnoreCase) { "Indie" };

    private readonly HashSet<string> _selectedArtCategories =
        new(StringComparer.OrdinalIgnoreCase) { "Paisagens" };

    private MusicTrackModel? _currentTrack;
    private ArtworkModel? _currentArtwork;

    public DiscoveryPage()
    {
        InitializeComponent();

        UpdateMusicButtons();
        UpdateArtButtons();
        UpdatePreferenceLabels();
    }

    // SELEÇÃO MUSICAL

    private void OnMusicPreferenceSelected(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        string genre = button.Text;

        if (!_selectedMusicGenres.Add(genre))
            _selectedMusicGenres.Remove(genre);

        UpdateMusicButtons();
        UpdatePreferenceLabels();
    }

    private void UpdateMusicButtons()
    {
        SetButtonState(MusicIndieButton, _selectedMusicGenres.Contains("Indie"));
        SetButtonState(MusicPopButton, _selectedMusicGenres.Contains("Pop"));
        SetButtonState(MusicInstrumentalButton, _selectedMusicGenres.Contains("Instrumental"));
        SetButtonState(MusicClassicaButton, _selectedMusicGenres.Contains("Clássica"));
        SetButtonState(MusicRockNacionalButton, _selectedMusicGenres.Contains("Rock nacional"));
        SetButtonState(MusicRockInternacionalButton, _selectedMusicGenres.Contains("Rock internacional"));
        SetButtonState(MusicMetalButton, _selectedMusicGenres.Contains("Metal"));
        SetButtonState(MusicMpBButton, _selectedMusicGenres.Contains("MPB"));
        SetButtonState(MusicFunkButton, _selectedMusicGenres.Contains("Funk"));
        SetButtonState(MusicSertanejoButton, _selectedMusicGenres.Contains("Sertanejo"));
        SetButtonState(MusicRnbButton, _selectedMusicGenres.Contains("R&B"));
        SetButtonState(MusicRapButton, _selectedMusicGenres.Contains("Rap / Hip-hop"));
        SetButtonState(MusicEletronicaButton, _selectedMusicGenres.Contains("Eletrônica"));
        SetButtonState(MusicJazzButton, _selectedMusicGenres.Contains("Jazz"));
        SetButtonState(MusicLoFiButton, _selectedMusicGenres.Contains("Lo-fi"));
    }

    // SELEÇÃO ARTÍSTICA

    private void OnArtPreferenceSelected(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        string category = button.Text;

        if (!_selectedArtCategories.Add(category))
            _selectedArtCategories.Remove(category);

        UpdateArtButtons();
        UpdatePreferenceLabels();
    }

    private void UpdateArtButtons()
    {
        SetButtonState(ArtPaisagensButton, _selectedArtCategories.Contains("Paisagens"));
        SetButtonState(ArtFloresButton, _selectedArtCategories.Contains("Flores e plantas"));
        SetButtonState(ArtRetratosButton, _selectedArtCategories.Contains("Pessoas e retratos"));
        SetButtonState(ArtArquiteturaButton, _selectedArtCategories.Contains("Cidades e arquitetura"));
        SetButtonState(ArtAbstrataButton, _selectedArtCategories.Contains("Arte abstrata"));
        SetButtonState(ArtAnimaisButton, _selectedArtCategories.Contains("Animais"));
    }

    private static void SetButtonState(Button button, bool selected)
    {
        button.BackgroundColor = selected
            ? Color.FromArgb("#76538F")
            : Color.FromArgb("#292039");

        button.BorderColor = selected
            ? Color.FromArgb("#E8A6FF")
            : Color.FromArgb("#514565");

        button.BorderWidth = selected ? 1.5 : 1;
        button.TextColor = Colors.White;
    }

    private void UpdatePreferenceLabels()
    {
        MusicPreferenceLabel.Text = _selectedMusicGenres.Count == 0
            ? "Nenhum estilo selecionado"
            : $"Selecionados: {string.Join(", ", _selectedMusicGenres)}";

        ArtPreferenceLabel.Text = _selectedArtCategories.Count == 0
            ? "Nenhuma categoria selecionada"
            : $"Selecionados: {string.Join(", ", _selectedArtCategories)}";
    }

    // GERAÇÃO DA DESCOBERTA

    private async void OnDiscoverClicked(object sender, EventArgs e)
    {
        await GenerateDiscoveryAsync();
    }

    private async Task GenerateDiscoveryAsync()
    {
        string city = CityEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(city))
        {
            ErrorLabel.Text = "Digite uma cidade para descobrir sua Aura.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (_selectedMusicGenres.Count == 0)
        {
            ErrorLabel.Text = "Selecione pelo menos um estilo musical.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (_selectedArtCategories.Count == 0)
        {
            ErrorLabel.Text = "Selecione pelo menos uma categoria artística.";
            ErrorLabel.IsVisible = true;
            return;
        }

        string previousTrackId = _currentTrack?.TrackId ?? "";
        int previousArtworkId = _currentArtwork?.ObjectId ?? 0;

        DiscoverButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;
        DiscoveryResultLayout.IsVisible = true;

        DiscoveryWeatherLabel.Text = "Consultando o clima...";
        DiscoveryMessageLabel.Text = "Preparando sua experiência...";
        WeatherStatusLabel.Text = "Buscando informações meteorológicas...";

        DiscoverySongTitle.Text = "Buscando música...";
        DiscoverySongArtist.Text = "";
        DiscoverySongImage.Source = null;
        DiscoveryMusicStatusLabel.Text = "Consultando o iTunes...";
        DiscoveryExternalMusicButton.IsVisible = false;
        DiscoveryAudioPlayer.Source = null;

        DiscoveryArtworkTitle.Text = "Buscando obra...";
        DiscoveryArtworkArtist.Text = "";
        DiscoveryArtworkImage.Source = null;
        DiscoveryArtworkDetailsButton.IsVisible = false;

        _currentTrack = null;
        _currentArtwork = null;

        bool hasError = false;

        // 1. CONSULTAR O CLIMA

        try
        {
            WeatherModel weather = await _weatherService.GetWeatherAsync(city);

            WeatherStatusLabel.Text = $"Clima consultado: {weather.City}";

            DiscoveryWeatherLabel.Text =
                $"{weather.City} • {weather.Temperature:F0}°C • {weather.Condition}";

            DiscoveryMessageLabel.Text = GetWeatherMessage(weather);
        }
        catch (Exception ex)
        {
            hasError = true;

            WeatherStatusLabel.Text = "Não foi possível consultar o clima.";
            DiscoveryWeatherLabel.Text = "Informações meteorológicas indisponíveis.";

            DiscoveryMessageLabel.Text =
                "Mesmo sem os dados do clima, ainda podemos preparar uma descoberta musical e artística.";

            System.Diagnostics.Debug.WriteLine($"Erro no clima: {ex}");
        }

        // 2. BUSCAR MÚSICAS DOS GÊNEROS SELECIONADOS

        try
        {
            var musicTasks = _selectedMusicGenres
                .Select(genre => _musicService.SearchTracksAsync(GetMusicSearchTerm(genre)));

            var musicResults = await Task.WhenAll(musicTasks);

            var tracks = musicResults
                .SelectMany(result => result)
                .GroupBy(track =>
                    string.IsNullOrWhiteSpace(track.TrackId)
                        ? $"{track.Title}|{track.Artist}"
                        : track.TrackId)
                .Select(group => group.First())
                .ToList();

            if (tracks.Count == 0)
            {
                DiscoverySongTitle.Text = "Nenhuma música encontrada.";
                DiscoveryMusicStatusLabel.Text = "Tente outras combinações.";
            }
            else
            {
                var candidates = tracks
                    .Where(track => track.HasPreview)
                    .ToList();

                if (candidates.Count == 0)
                    candidates = tracks;

                var differentTracks = candidates
                    .Where(track => track.TrackId != previousTrackId)
                    .ToList();

                if (differentTracks.Count > 0)
                    candidates = differentTracks;

                var selectedTrack =
                    candidates[Random.Shared.Next(candidates.Count)];

                DisplayTrack(selectedTrack);
            }
        }
        catch (Exception ex)
        {
            hasError = true;

            DiscoverySongTitle.Text = "Não foi possível carregar a música.";
            DiscoveryMusicStatusLabel.Text = "Verifique sua conexão e tente novamente.";

            System.Diagnostics.Debug.WriteLine($"Erro no iTunes: {ex}");
        }

        // 3. BUSCAR OBRAS DAS CATEGORIAS SELECIONADAS

        try
        {
            var artTasks = _selectedArtCategories
                .Select(category => _artService.GetArtworksAsync(GetArtSearchTerm(category)));

            var artResults = await Task.WhenAll(artTasks);

            var artworks = artResults
                .SelectMany(result => result)
                .GroupBy(artwork => artwork.ObjectId)
                .Select(group => group.First())
                .ToList();

            if (artworks.Count == 0)
            {
                DiscoveryArtworkTitle.Text = "Nenhuma obra encontrada.";
                DiscoveryArtworkArtist.Text = "Tente outras categorias.";
            }
            else
            {
                var differentArtworks = artworks
                    .Where(artwork => artwork.ObjectId != previousArtworkId)
                    .ToList();

                if (differentArtworks.Count > 0)
                    artworks = differentArtworks;

                var selectedArtwork =
                    artworks[Random.Shared.Next(artworks.Count)];

                DisplayArtwork(selectedArtwork);
            }
        }
        catch (Exception ex)
        {
            hasError = true;

            DiscoveryArtworkTitle.Text = "Não foi possível carregar a obra.";
            DiscoveryArtworkArtist.Text = "Verifique sua conexão e tente novamente.";

            System.Diagnostics.Debug.WriteLine($"Erro no The Met: {ex}");
        }

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        DiscoverButton.IsEnabled = true;

        if (hasError)
        {
            ErrorLabel.Text =
                "Algumas informações não puderam ser carregadas. Você ainda pode aproveitar as sugestões disponíveis.";

            ErrorLabel.IsVisible = true;
        }
    }

    // TERMOS DE PESQUISA MUSICAL

    private string GetMusicSearchTerm(string genre)
    {
        return genre switch
        {
            "Indie" => "indie alternative",
            "Pop" => "pop hits",
            "Instrumental" => "instrumental ambient",
            "Clássica" => "classical music piano",
            "Rock nacional" => "rock brasileiro",
            "Rock internacional" => "international rock",
            "Metal" => "heavy metal",
            "MPB" => "musica popular brasileira MPB",
            "Funk" => "funk brasileiro",
            "Sertanejo" => "sertanejo brasileiro",
            "R&B" => "R&B soul",
            "Rap / Hip-hop" => "rap hip hop",
            "Eletrônica" => "electronic dance music",
            "Jazz" => "jazz music",
            "Lo-fi" => "lofi chill beats",
            _ => genre
        };
    }

    // TERMOS DE PESQUISA ARTÍSTICA

    private string GetArtSearchTerm(string category)
    {
        return category switch
        {
            "Paisagens" => "landscape",
            "Flores e plantas" => "flowers botanical",
            "Pessoas e retratos" => "portrait",
            "Cidades e arquitetura" => "architecture city",
            "Arte abstrata" => "abstract art",
            "Animais" => "animals",
            _ => "painting"
        };
    }

    // MENSAGEM BASEADA NO CLIMA

    private string GetWeatherMessage(WeatherModel weather)
    {
        if (weather.WeatherCode >= 95)
            return "O céu está criando uma atmosfera intensa. Aproveite este momento para descobrir novas sonoridades e expressões artísticas.";

        if (weather.WeatherCode >= 51 && weather.WeatherCode <= 67 ||
            weather.WeatherCode >= 80 && weather.WeatherCode <= 82)
            return "Um clima chuvoso pode ser um convite para desacelerar, ouvir uma boa música e apreciar a beleza da arte.";

        if (weather.WeatherCode == 0 || weather.WeatherCode == 1)
            return "O céu está mais aberto. Que tal aproveitar essa atmosfera para explorar novas ideias e inspirações?";

        if (weather.WeatherCode == 2 || weather.WeatherCode == 3)
            return "Entre nuvens e momentos de luz, existe espaço para novas descobertas. Explore sua combinação de hoje.";

        return "Cada condição do tempo cria uma atmosfera diferente. Descubra qual música e obra combinam com o seu momento.";
    }

    // EXIBIR MÚSICA

    private void DisplayTrack(MusicTrackModel track)
    {
        _currentTrack = track;

        DiscoverySongTitle.Text = track.Title;
        DiscoverySongArtist.Text = track.Artist;

        DiscoverySongImage.Source =
            string.IsNullOrWhiteSpace(track.ArtworkUrl)
                ? null
                : ImageSource.FromUri(new Uri(track.ArtworkUrl));

        DiscoveryExternalMusicButton.IsVisible =
            !string.IsNullOrWhiteSpace(track.StoreUrl);

        DiscoveryAudioPlayer.Source = null;

        if (track.HasPreview &&
            Uri.TryCreate(track.PreviewUrl, UriKind.Absolute, out Uri? previewUri))
        {
            DiscoveryMusicStatusLabel.Text = "Carregando prévia da música...";
            DiscoveryAudioPlayer.Source = MediaSource.FromUri(previewUri);
        }
        else
        {
            DiscoveryMusicStatusLabel.Text =
                "Esta música não possui prévia disponível.";
        }
    }

    // EXIBIR OBRA

    private void DisplayArtwork(ArtworkModel artwork)
    {
        _currentArtwork = artwork;

        DiscoveryArtworkImage.Source =
            ImageSource.FromUri(new Uri(artwork.ImageUrl));

        DiscoveryArtworkTitle.Text = artwork.Title;

        DiscoveryArtworkArtist.Text =
            string.IsNullOrWhiteSpace(artwork.Artist)
                ? "Artista não informado"
                : artwork.Artist;

        DiscoveryArtworkDetailsButton.IsVisible = true;
    }

    // PLAYER DE ÁUDIO

    private void OnDiscoveryMediaOpened(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DiscoveryMusicStatusLabel.Text =
                "Prévia disponível para reprodução.";
        });
    }

    private void OnDiscoveryMediaFailed(object? sender, MediaFailedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DiscoveryMusicStatusLabel.Text =
                "Não foi possível reproduzir esta prévia.";
        });

        System.Diagnostics.Debug.WriteLine(
            $"Falha no player: {e.ErrorMessage}");
    }

    private async void OnExternalMusicClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_currentTrack?.StoreUrl))
            await Launcher.OpenAsync(_currentTrack.StoreUrl);
    }

    // DETALHES DA OBRA

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
}
