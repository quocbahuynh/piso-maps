using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>Response containing a list of autocomplete suggestions.</summary>
public class MapSearchResultDto
{
    /// <summary>List of autocomplete place suggestions matching the query.</summary>
    [JsonPropertyName("suggestions")]
    public List<MapSuggestionDto> Suggestions { get; set; } = new();
}
