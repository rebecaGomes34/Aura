
namespace Aura.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnDiscoveryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(DiscoveryPage)}");
    }

    private async void OnWeatherClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(WeatherPage)}");
    }

    private async void OnArtClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(ArtPage)}");
    }

    private async void OnMusicClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(MusicPage)}");
    }

    private async void OnMoodClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(MoodPage)}");
    }
}
