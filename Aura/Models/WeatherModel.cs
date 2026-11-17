namespace Aura.Models;

public class WeatherModel
{
    public string City { get; set; } = "";
    public double Temperature { get; set; }
    public double ApparentTemperature { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int WeatherCode { get; set; }
    public string Condition { get; set; } = "";
}
