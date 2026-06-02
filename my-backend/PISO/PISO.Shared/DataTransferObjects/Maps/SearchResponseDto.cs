using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>Response containing a list of place search results.</summary>
public class SearchResponseDto
{
    /// <summary>List of places returned by the search query.</summary>
    [JsonPropertyName("local_result")]
    public List<PlaceDetailResponse> Results { get; set; } = new();
}
