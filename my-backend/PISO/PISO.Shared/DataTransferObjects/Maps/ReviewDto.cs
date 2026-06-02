using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>A user review for a place.</summary>
public class ReviewDto
{
    /// <summary>Unique identifier for the review.</summary>
    [JsonPropertyName("review_id")]
    public string ReviewId { get; set; } = string.Empty;

    /// <summary>Display name of the review author.</summary>
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>URL to the author's profile picture.</summary>
    [JsonPropertyName("author_profile_pic")]
    public string AuthorProfilePic { get; set; } = string.Empty;

    /// <summary>Rating given by the author (1–5).</summary>
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    /// <summary>Relative date of the review (e.g. "2 weeks ago").</summary>
    [JsonPropertyName("relative_date")]
    public string RelativeDate { get; set; } = string.Empty;

    /// <summary>Review text content.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Photos attached to the review.</summary>
    [JsonPropertyName("photos")]
    public List<ReviewPhotoDto> Photos { get; set; } = new();
}
