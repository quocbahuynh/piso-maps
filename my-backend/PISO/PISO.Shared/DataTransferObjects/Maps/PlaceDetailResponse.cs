using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>Summary of a place returned in search results.</summary>
public class PlaceDetailResponse
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
}

/// <summary>Price level range for a place.</summary>
public class PricingDto
{
    /// <summary>Minimum price level.</summary>
    [JsonPropertyName("min")]
    public double Min { get; set; }

    /// <summary>Maximum price level.</summary>
    [JsonPropertyName("max")]
    public double Max { get; set; }
}

/// <summary>Contact information for a place.</summary>
public class ContactDto
{
    /// <summary>Phone number.</summary>
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Website URL.</summary>
    [JsonPropertyName("website")]
    public string Website { get; set; } = string.Empty;
}

/// <summary>Location details for a place.</summary>
public class LocationDto
{
    /// <summary>Structured address information.</summary>
    [JsonPropertyName("address")]
    public AddressDto Address { get; set; } = new();

    /// <summary>Latitude coordinate.</summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>Longitude coordinate.</summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>Timezone identifier (e.g. Asia/Ho_Chi_Minh).</summary>
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    /// <summary>Country code (e.g. VN).</summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = string.Empty;
}

/// <summary>Structured address for a place.</summary>
public class AddressDto
{
    /// <summary>Full address string.</summary>
    [JsonPropertyName("full")]
    public string Full { get; set; } = string.Empty;

    /// <summary>Street name and number.</summary>
    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    /// <summary>Ward or district.</summary>
    [JsonPropertyName("ward")]
    public string Ward { get; set; } = string.Empty;

    /// <summary>City or province.</summary>
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    /// <summary>Country name.</summary>
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}

/// <summary>Open/closed status of a place.</summary>
public class StatusDto
{
    /// <summary>Indicates whether the place is currently open.</summary>
    [JsonPropertyName("is_open_now")]
    public bool IsOpenNow { get; set; }

    /// <summary>Human-readable status text (e.g. "Open ⋅ Closes 10 PM").</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>Opening hours for a specific day of the week.</summary>
public class HourItemDto
{
    /// <summary>Day of the week (e.g. Monday, Tuesday).</summary>
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;

    /// <summary>Opening hours string (e.g. "9 AM–10 PM").</summary>
    [JsonPropertyName("hours")]
    public string Hours { get; set; } = string.Empty;
}

/// <summary>Available features and amenities at a place.</summary>
public class FeatureDto
{
    /// <summary>Accessibility features (e.g. wheelchair-accessible entrance).</summary>
    [JsonPropertyName("accessibility")]
    public List<string> Accessibility { get; set; } = new();

    /// <summary>Parking options (e.g. free parking lot, street parking).</summary>
    [JsonPropertyName("parking")]
    public List<string> Parking { get; set; } = new();

    /// <summary>Accepted payment methods.</summary>
    [JsonPropertyName("payments")]
    public List<string> Payments { get; set; } = new();

    /// <summary>Service options (e.g. dine-in, takeaway, delivery).</summary>
    [JsonPropertyName("service_options")]
    public List<string> ServiceOptions { get; set; } = new();

    /// <summary>Other features not covered by the categories above.</summary>
    [JsonPropertyName("other")]
    public List<string> Other { get; set; } = new();
}
