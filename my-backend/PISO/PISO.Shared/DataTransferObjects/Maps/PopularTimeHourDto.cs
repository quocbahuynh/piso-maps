using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>Occupancy data for a specific hour of the day.</summary>
public class PopularTimeHourDto
{
    /// <summary>Hour of the day (0–23).</summary>
    [JsonPropertyName("hour")]
    public int Hour { get; set; }

    /// <summary>Occupancy percentage at this hour (0–100).</summary>
    [JsonPropertyName("occupancy_percent")]
    public int OccupancyPercent { get; set; }

    /// <summary>Occupancy status label (e.g. Not too busy, A bit busy).</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Human-readable time label (e.g. "9 AM", "2 PM").</summary>
    [JsonPropertyName("time_label")]
    public string TimeLabel { get; set; } = string.Empty;
}
