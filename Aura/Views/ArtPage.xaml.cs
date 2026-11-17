
using Aura.Models;
using Aura.Services;
using System.Collections.ObjectModel;

namespace Aura.Views;

public partial class ArtPage : ContentPage
{
    private readonly ArtService _artService = new();

    public ObservableCollection<ArtworkModel> Artworks { get; set; } = new();

    public ArtPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Artworks.Count == 0)
        {
            await LoadArtworksAsync();
        }
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string searchTerm = SearchEntry.Text?.Trim() ?? "";

        await LoadArtworksAsync(searchTerm);
    }

    private async void OnArtworkSelected(
        object sender,
        SelectionChangedEventArgs e)
    {
        var artwork = e.CurrentSelection.FirstOrDefault() as ArtworkModel;

        if (artwork == null)
            return;

        ArtworksCollection.SelectedItem = null;

        await Shell.Current.GoToAsync(
            nameof(ArtworkDetailsPage),
            true,
            new Dictionary<string, object>
            {
                { "Artwork", artwork }
            }
        );
    }

    private async Task LoadArtworksAsync(string searchTerm = "")
    {
        try
        {
            SearchButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            ErrorLabel.IsVisible = false;
            EmptyLabel.IsVisible = false;
            ArtworksCollection.IsVisible = false;

            Artworks.Clear();

            var results = await _artService.GetArtworksAsync(searchTerm);

            foreach (var artwork in results)
            {
                Artworks.Add(artwork);
            }

            ArtworkCountLabel.Text = $"{Artworks.Count} OBRAS";

            GalleryTitle.Text = string.IsNullOrWhiteSpace(searchTerm)
                ? "OBRAS PARA DESCOBRIR"
                : "RESULTADOS DA PESQUISA";

            if (Artworks.Count > 0)
            {
                ArtworksCollection.IsVisible = true;
            }
            else
            {
                EmptyLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Erro ao carregar obras: {ex}"
            );

            ErrorLabel.Text =
                "Não foi possível carregar a coleção. Verifique sua conexão e tente novamente.";

            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            SearchButton.IsEnabled = true;
        }
    }
}
