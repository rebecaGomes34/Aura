
using System.Text.Json;
using Aura.Models;

namespace Aura.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient = new();

    public async Task<WeatherModel> GetWeatherAsync(string city)
    {
        // 1. Buscar as coordenadas da cidade
        string cityUrl =
            $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=pt&format=json";

        var cityResponse = await _httpClient.GetStringAsync(cityUrl);

        using var cityJson = JsonDocument.Parse(cityResponse);

        if (!cityJson.RootElement.TryGetProperty("results", out var results)
            || results.GetArrayLength() == 0)
        {
            throw new Exception("Cidade não encontrada. Verifique o nome e tente novamente.");
        }

        var location = results[0];

        double latitude = location.GetProperty("latitude").GetDouble();
        double longitude = location.GetProperty("longitude").GetDouble();

        string cityName = location.GetProperty("name").GetString() ?? city;

        // 2. Consultar o clima usando as coordenadas
        string weatherUrl =
            $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m&timezone=auto";

        var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);

        using var weatherJson = JsonDocument.Parse(weatherResponse);

        var current = weatherJson.RootElement.GetProperty("current");

        int weatherCode = current.GetProperty("weather_code").GetInt32();

        return new WeatherModel
        {
            City = cityName,
            Temperature = current.GetProperty("temperature_2m").GetDouble(),
            ApparentTemperature = current.GetProperty("apparent_temperature").GetDouble(),
            Humidity = current.GetProperty("relative_humidity_2m").GetInt32(),
            WindSpeed = current.GetProperty("wind_speed_10m").GetDouble(),
            WeatherCode = weatherCode,
            Condition = GetWeatherCondition(weatherCode)
        };
    }

    private static string GetWeatherCondition(int code)
    {
        return code switch
        {
            0 => "Céu limpo",
            1 => "Predominantemente limpo",
            2 => "Parcialmente nublado",
            3 => "Nublado",
            45 or 48 => "Neblina",
            51 or 53 or 55 => "Garoa",
            56 or 57 => "Garoa congelante",
            61 or 63 or 65 => "Chuva",
            66 or 67 => "Chuva congelante",
            71 or 73 or 75 => "Neve",
            77 => "Grãos de neve",
            80 or 81 or 82 => "Pancadas de chuva",
            85 or 86 => "Pancadas de neve",
            95 => "Tempestade",
            96 or 99 => "Tempestade com granizo",
            _ => "Condição desconhecida"
        };
    }
}
