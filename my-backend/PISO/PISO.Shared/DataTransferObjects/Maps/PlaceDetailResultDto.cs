using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>Response containing full place detail information.</summary>
public class PlaceDetailResultDto
{
    /// <summary>Detailed place information including hours, reviews, photos, and more.</summary>
    [JsonPropertyName("place_result")]
    public PlaceDetailFullDto? PlaceResult { get; set; }
}
