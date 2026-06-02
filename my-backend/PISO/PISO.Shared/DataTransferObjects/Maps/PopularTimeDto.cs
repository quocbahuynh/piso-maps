using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>Popular times data for a single day of the week.</summary>
public class PopularTimeDto
{
    /// <summary>Day of the week (e.g. Monday, Tuesday).</summary>
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;

    /// <summary>Hourly occupancy data for this day.</summary>
    [JsonPropertyName("hours")]
    public List<PopularTimeHourDto> Hours { get; set; } = new();
}
