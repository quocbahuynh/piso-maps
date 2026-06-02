using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>Full details for a single place, including all available fields.</summary>
public class PlaceDetailFullDto
{
    /// <summary>Google Maps data ID for the place.</summary>
    [JsonPropertyName("data_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Google Maps place ID (CID).</summary>
    [JsonPropertyName("place_id")]
    public string Cid { get; set; } = string.Empty;

    /// <summary>Display name of the place.</summary>
    [JsonPropertyName("title")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description or summary of the place.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Category or type of the place (e.g. cafe, restaurant).</summary>
    [JsonPropertyName("type")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Average rating (0–5).</summary>
    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    /// <summary>Total number of reviews.</summary>
    [JsonPropertyName("reviews")]
    public int TotalReviews { get; set; }

    /// <summary>Price level information for the place.</summary>
    [JsonPropertyName("pricing")]
    public PricingDto Pricing { get; set; } = new();

    /// <summary>Detailed pricing ranges.</summary>
    [JsonPropertyName("pricing_ranges")]
    public List<PricingRangeDto> PricingRanges { get; set; } = new();

    /// <summary>Contact details (phone, website).</summary>
    [JsonPropertyName("contacts")]
    public ContactDto Contacts { get; set; } = new();

    /// <summary>Location details (address, coordinates, timezone).</summary>
    [JsonPropertyName("location")]
    public LocationDto Location { get; set; } = new();

    /// <summary>Whether the place is currently open.</summary>
    [JsonPropertyName("open_state")]
    public StatusDto Status { get; set; } = new();

    /// <summary>Weekly opening hours.</summary>
    [JsonPropertyName("opening_hours")]
    public List<HourItemDto> WeeklyHours { get; set; } = new();

    /// <summary>Available features (accessibility, parking, payments, etc.).</summary>
    [JsonPropertyName("features")]
    public FeatureDto Features { get; set; } = new();

    /// <summary>Popular times histogram data by day.</summary>
    [JsonPropertyName("popular_times")]
    public List<PopularTimeDto> PopularTimes { get; set; } = new();

    /// <summary>User reviews for the place.</summary>
    [JsonPropertyName("review_list")]
    public List<ReviewDto> Reviews { get; set; } = new();

    /// <summary>Menu dishes available at the place.</summary>
    [JsonPropertyName("menu")]
    public List<MenuDishDto> Menu { get; set; } = new();

    /// <summary>Photo categories and their associated media.</summary>
    [JsonPropertyName("photos")]
    public List<MediaCategoryDto> Photos { get; set; } = new();

    /// <summary>Google Maps URL for the place.</summary>
    [JsonPropertyName("google_maps_url")]
    public string GoogleMapsUrl { get; set; } = string.Empty;

    /// <summary>Global Plus Code for the place location.</summary>
    [JsonPropertyName("global_plus_code")]
    public string GlobalPlusCode { get; set; } = string.Empty;
}
