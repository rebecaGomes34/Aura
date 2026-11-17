namespace Aura.Models;

public class MusicTrackModel
{
    public string TrackId { get; set; } = "";

    public string Title { get; set; } = "";

    public string Artist { get; set; } = "";

    public string Album { get; set; } = "";

    public string ArtworkUrl { get; set; } = "";

    public string PreviewUrl { get; set; } = "";

    public string StoreUrl { get; set; } = "";

    public bool HasPreview => !string.IsNullOrWhiteSpace(PreviewUrl);
}
