using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>A single media item (photo) with dimensions.</summary>
public class MediaItemDto
{
    /// <summary>Unique identifier for the media item.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>URL of the media item.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>Width in pixels.</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>Height in pixels.</summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }
}
