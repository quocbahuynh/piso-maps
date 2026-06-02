using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>A specific pricing range category for a place.</summary>
public class PricingRangeDto
{
    /// <summary>Display label for the pricing range (e.g. "$", "$$").</summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>Key identifying the pricing range category.</summary>
    [JsonPropertyName("range_key")]
    public string RangeKey { get; set; } = string.Empty;

    /// <summary>Weight or confidence score for this pricing range.</summary>
    [JsonPropertyName("weight")]
    public double Weight { get; set; }
}
