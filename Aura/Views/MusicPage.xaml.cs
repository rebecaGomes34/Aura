
using Aura.Models;
using Aura.Services;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;

namespace Aura.Views;

public partial class MusicPage : ContentPage
{
    private readonly ITunesMusicService _iTunesMusicService = new();

    private string _currentTrackUrl = "";

    public MusicPage()
    {
        InitializeComponent();
    }

    private async void OnTrackSearchClicked(object sender, EventArgs e)
    {
        string searchTerm = TrackSearchEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            ErrorLabel.Text = "Digite o nome de uma música ou artista para pesquisar.";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;
            TrackResultsLayout.IsVisible = false;

            var tracks = await _iTunesMusicService.SearchTracksAsync(searchTerm);
            TrackResultsCollection.ItemsSource = tracks;
            TrackResultsLayout.IsVisible = tracks.Count > 0;

            if (tracks.Count == 0)
            {
                ErrorLabel.Text = "Nenhuma música encontrada. Tente outra pesquisa.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = "Não foi possível pesquisar músicas. Verifique sua conexão.";
            ErrorLabel.IsVisible = true;
            System.Diagnostics.Debug.WriteLine($"Erro na pesquisa do iTunes: {ex}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void OnTrackSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not MusicTrackModel selectedTrack)
            return;

        TrackResultsCollection.SelectedItem = null;
        DisplayTrack(selectedTrack);
    }

    private void DisplayTrack(MusicTrackModel track)
    {
        NowPlayingLabel.Text = track.Title;
        NowPlayingArtistLabel.Text = track.Artist;
        _currentTrackUrl = track.StoreUrl;

        ExternalTrackButton.IsVisible = !string.IsNullOrWhiteSpace(track.StoreUrl);
        PlayerLayout.IsVisible = true;

        AudioPlayer.Source = null;

        if (track.HasPreview && Uri.TryCreate(track.PreviewUrl, UriKind.Absolute, out Uri? previewUri))
        {
            PreviewStatusLabel.Text = "Carregando prévia...";
            AudioPlayer.Source = MediaSource.FromUri(previewUri);
        }
        else
        {
            PreviewStatusLabel.Text = "Esta música não possui prévia de áudio disponível.";
        }
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PreviewStatusLabel.Text = "Prévia disponível. Reprodução iniciada.";
        });
    }

    private void OnMediaFailed(object? sender, MediaFailedEventArgs e)
    {
        string errorMessage = e.ErrorMessage;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            PreviewStatusLabel.Text = "Não foi possível reproduzir esta prévia neste dispositivo.";
        });

        System.Diagnostics.Debug.WriteLine($"Falha no MediaElement: {errorMessage}");
    }

    private async void OnExternalTrackClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_currentTrackUrl))
            await Launcher.OpenAsync(_currentTrackUrl);
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        TrackSearchButton.IsEnabled = !isLoading;
    }
}
