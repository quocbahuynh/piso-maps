using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>A photo attached to a review.</summary>
public class ReviewPhotoDto
{
    /// <summary>Unique identifier for the photo.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>URL of the photo.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>Width of the photo in pixels.</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>Height of the photo in pixels.</summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }
}
