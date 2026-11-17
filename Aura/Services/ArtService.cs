
using System.Text.Json;
using Aura.Models;

namespace Aura.Services;

public class ArtService
{
    private readonly HttpClient _httpClient = new();

    private const string BaseUrl =
        "https://collectionapi.metmuseum.org/public/collection/v1";

    public async Task<List<ArtworkModel>> GetArtworksAsync(string searchTerm = "")
    {
        string term = string.IsNullOrWhiteSpace(searchTerm)
            ? "painting"
            : searchTerm.Trim();

        string searchUrl =
            $"{BaseUrl}.1/search" +
            $"?q={Uri.EscapeDataString(term)}" +
            "&hasImages=true" +
            "&limit=100";

        using var searchResponse = await _httpClient.GetAsync(searchUrl);

        searchResponse.EnsureSuccessStatusCode();

        string searchJson = await searchResponse.Content.ReadAsStringAsync();

        using JsonDocument searchDocument = JsonDocument.Parse(searchJson);

        var root = searchDocument.RootElement;

        var artworks = new List<ArtworkModel>();

        if (!root.TryGetProperty("objectIDs", out var objectIDs) ||
            objectIDs.ValueKind != JsonValueKind.Array)
        {
            return artworks;
        }

        var ids = objectIDs
            .EnumerateArray()
            .Where(id => id.ValueKind == JsonValueKind.Number)
            .Select(id => id.GetInt32())
            .ToList();

        // Consultar os detalhes em lotes de 10.
        const int batchSize = 10;

        for (int i = 0; i < ids.Count; i += batchSize)
        {
            var batch = ids
                .Skip(i)
                .Take(batchSize);

            var tasks = batch.Select(GetArtworkDetailsAsync);

            var results = await Task.WhenAll(tasks);

            foreach (var artwork in results)
            {
                if (artwork == null)
                    continue;

                artworks.Add(artwork);

                // Limitar a galeria a 10 obras.
                if (artworks.Count >= 10)
                    return artworks;
            }
        }

        return artworks;
    }

    private async Task<ArtworkModel?> GetArtworkDetailsAsync(int objectId)
    {
        try
        {
            string objectUrl = $"{BaseUrl}/objects/{objectId}";

            using var response = await _httpClient.GetAsync(objectUrl);

            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(json);

            var record = document.RootElement;

            bool isPublicDomain = GetBool(record, "isPublicDomain");

            if (!isPublicDomain)
                return null;

            string imageUrl = GetString(record, "primaryImageSmall", "");

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                imageUrl = GetString(record, "primaryImage", "");
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            return new ArtworkModel
            {
                ObjectId = GetInt(record, "objectID"),
                Title = GetString(record, "title", "Obra sem título"),
                Artist = GetString(
                    record,
                    "artistDisplayName",
                    "Artista desconhecido"
                ),
                Date = GetString(
                    record,
                    "objectDate",
                    "Data desconhecida"
                ),
                Culture = GetString(
                    record,
                    "culture",
                    "Não informada"
                ),
                Classification = GetString(
                    record,
                    "classification",
                    "Não informada"
                ),
                Medium = GetString(
                    record,
                    "medium",
                    "Não informado"
                ),
                ImageUrl = imageUrl,
                DetailsUrl = GetString(
                    record,
                    "objectURL",
                    ""
                )
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Erro ao carregar obra {objectId}: {ex.Message}"
            );

            return null;
        }
    }

    private static string GetString(
        JsonElement element,
        string property,
        string fallback)
    {
        if (element.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? fallback;
        }

        return fallback;
    }

    private static int GetInt(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.Number)
        {
            return value.GetInt32();
        }

        return 0;
    }

    private static bool GetBool(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) &&
               value.ValueKind == JsonValueKind.True;
    }
}
