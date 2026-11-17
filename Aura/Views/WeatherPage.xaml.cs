
using Aura.Services;
using System.Globalization;

namespace Aura.Views;

public partial class WeatherPage : ContentPage
{
    private readonly WeatherService _weatherService = new();

    public WeatherPage()
    {
        InitializeComponent();
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string city = CityEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(city))
        {
            ErrorLabel.Text = "Digite o nome de uma cidade.";
            ErrorLabel.IsVisible = true;
            WeatherResultLayout.IsVisible = false;
            return;
        }

        try
        {
            SearchButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            ErrorLabel.IsVisible = false;
            WeatherResultLayout.IsVisible = false;

            var weather = await _weatherService.GetWeatherAsync(city);

            CityResultLabel.Text = weather.City;

            TemperatureLabel.Text =
                $"{weather.Temperature.ToString("0.#", CultureInfo.GetCultureInfo("pt-BR"))}°";

            ConditionLabel.Text = weather.Condition;

            FeelsLikeLabel.Text =
                $"Sensação térmica: {weather.ApparentTemperature.ToString("0.#", CultureInfo.GetCultureInfo("pt-BR"))}°C";

            HumidityLabel.Text = $"{weather.Humidity}%";

            WindLabel.Text =
                $"{weather.WindSpeed.ToString("0.#", CultureInfo.GetCultureInfo("pt-BR"))} km/h";

            WeatherResultLayout.IsVisible = true;
        }
        catch (HttpRequestException)
        {
            ErrorLabel.Text = "Não foi possível acessar o serviço. Verifique sua conexão.";
            ErrorLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            SearchButton.IsEnabled = true;
        }
    }
}

