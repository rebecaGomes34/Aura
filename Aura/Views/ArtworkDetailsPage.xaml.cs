
using Aura.Models;

namespace Aura.Views;

public partial class ArtworkDetailsPage : ContentPage, IQueryAttributable
{
    private ArtworkModel? _artwork;

    public ArtworkDetailsPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Artwork", out var value) &&
            value is ArtworkModel artwork)
        {
            _artwork = artwork;
            DisplayArtwork(artwork);
        }
    }

    private void DisplayArtwork(ArtworkModel artwork)
    {
        ArtworkImage.Source = artwork.ImageUrl;

        ArtworkTitleLabel.Text = artwork.Title;
        ArtworkArtistLabel.Text = artwork.Artist;
        ArtworkDateLabel.Text = artwork.Date;

        ArtistInfoLabel.Text = artwork.Artist;
        DateInfoLabel.Text = artwork.Date;
        CultureInfoLabel.Text = artwork.Culture;
        ClassificationInfoLabel.Text = artwork.Classification;
        MediumInfoLabel.Text = artwork.Medium;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnMuseumClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_artwork?.DetailsUrl))
        {
            await Launcher.OpenAsync(_artwork.DetailsUrl);
        }
    }
}
