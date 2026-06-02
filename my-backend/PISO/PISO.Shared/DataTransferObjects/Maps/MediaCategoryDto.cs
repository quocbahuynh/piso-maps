using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>A category of media (photos) grouped by type.</summary>
public class MediaCategoryDto
{
    /// <summary>Unique identifier for the category.</summary>
    [JsonPropertyName("category_id")]
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>Display label for the category (e.g. "Interior", "Exterior").</summary>
    [JsonPropertyName("category_label")]
    public string CategoryLabel { get; set; } = string.Empty;

    /// <summary>Media items belonging to this category.</summary>
    [JsonPropertyName("media")]
    public List<MediaItemDto> Media { get; set; } = new();
}
