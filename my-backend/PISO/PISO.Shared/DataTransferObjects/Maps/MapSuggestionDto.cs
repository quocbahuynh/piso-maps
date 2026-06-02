using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>A single autocomplete suggestion from Google Maps.</summary>
public class MapSuggestionDto
{
    /// <summary>Display name of the suggested place.</summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>Additional context (e.g. address or area).</summary>
    [JsonPropertyName("subtext")]
    public string Subtext { get; set; } = string.Empty;

    /// <summary>Latitude of the suggested place.</summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>Longitude of the suggested place.</summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>Google Maps data ID for the place.</summary>
    [JsonPropertyName("data_id")]
    public string? DataId { get; set; }

    /// <summary>Google Maps place ID.</summary>
    [JsonPropertyName("place_id")]
    public string? PlaceId { get; set; }
}
