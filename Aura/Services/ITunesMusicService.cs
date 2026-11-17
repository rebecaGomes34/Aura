using System.Text.Json;
using Aura.Models;

namespace Aura.Services;

public class ITunesMusicService
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<MusicTrackModel>> SearchTracksAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return [];

        string url =
            "https://itunes.apple.com/search" +
            $"?term={Uri.EscapeDataString(searchTerm.Trim())}" +
            "&media=music&entity=song&limit=25&country=BR";

        using HttpResponseMessage response = await _httpClient.GetAsync(
            url,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using Stream contentStream = await response.Content.ReadAsStreamAsync(
            cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(
            contentStream,
            cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("results", out JsonElement results)
            || results.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var tracks = new List<MusicTrackModel>();

        foreach (JsonElement result in results.EnumerateArray())
        {
            string previewUrl = GetString(result, "previewUrl");
            string trackName = GetString(result, "trackName");
            string artistName = GetString(result, "artistName");

            if (string.IsNullOrWhiteSpace(trackName)
                || string.IsNullOrWhiteSpace(artistName))
            {
                continue;
            }

            tracks.Add(new MusicTrackModel
            {
                TrackId = GetString(result, "trackId"),
                Title = trackName,
                Artist = artistName,
                Album = GetString(result, "collectionName", "Álbum não informado"),
                ArtworkUrl = GetString(result, "artworkUrl100"),
                PreviewUrl = previewUrl,
                StoreUrl = GetString(result, "trackViewUrl")
            });
        }

        return tracks;
    }

    private static string GetString(
        JsonElement element,
        string property,
        string fallback = "")
    {
        return element.TryGetProperty(property, out JsonElement value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;
    }
}
