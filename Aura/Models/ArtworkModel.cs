
namespace Aura.Models;

public class ArtworkModel
{
    public int ObjectId { get; set; }

    public string Title { get; set; } = "";

    public string Artist { get; set; } = "";

    public string Date { get; set; } = "";

    public string Culture { get; set; } = "";

    public string Classification { get; set; } = "";

    public string Medium { get; set; } = "";

    public string ImageUrl { get; set; } = "";

    public string DetailsUrl { get; set; } = "";
}
