using Microsoft.AspNetCore.Mvc;
using PISO.Service.Contracts;
using PISO.Shared.Attributes;
using PISO.Shared.DataTransferObjects;
using PISO.Shared.DataTransferObjects.Maps;

namespace PISO.Presentation.Controllers;

/// <summary>
/// Google Maps scraping endpoints — autocomplete suggestions, place search, and place detail.
/// All endpoints require an <c>x-api-key</c> header and consume credits per request.
/// </summary>
[Route("api/maps")]
[ApiController]
[ApiVersion("1.0")]
[BillableEndpoint(Cost = 1)]
public class MapsController : ControllerBase
{
    private readonly IServiceManager _services;

    public MapsController(IServiceManager services)
    {
        _services = services;
    }

    /// <summary>Returns autocomplete suggestions for a given query and location.</summary>
    /// <param name="q">Search query (e.g. "highland", "starbucks").</param>
    /// <param name="lat">Latitude of the search center.</param>
    /// <param name="lng">Longitude of the search center.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of autocomplete suggestions with place details.</returns>
    /// <response code="200">Autocomplete suggestions returned successfully.</response>
    /// <response code="500">Scraping failed due to an internal error.</response>
    [HttpGet("autocomplete", Name = "Autocomplete")]
    public async Task<ActionResult<MapSearchResultDto>> Autocomplete(
        [FromQuery] string q = "highland",
        [FromQuery] double lat = 10.801670806563099,
        [FromQuery] double lng = 106.61743959351683,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _services.Place.AutocompleteAsync(q, lat, lng, ct);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponseDto { StatusCode = 500, Message = "Scraping failed. Please try again." });
        }
    }

    /// <summary>Searches for places matching a query near a given location.</summary>
    /// <param name="q">Search query (e.g. "highland", "coffee shop").</param>
    /// <param name="lat">Latitude of the search center.</param>
    /// <param name="lng">Longitude of the search center.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of place results matching the search query.</returns>
    /// <response code="200">Search results returned successfully.</response>
    /// <response code="500">Scraping failed due to an internal error.</response>
    [HttpGet("search", Name = "Search")]
    public async Task<ActionResult<SearchResponseDto>> Search(
        [FromQuery] string q = "highland",
        [FromQuery] double lat = 10.801670806563099,
        [FromQuery] double lng = 106.61743959351683,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _services.Place.SearchAsync(q, lat, lng, ct);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponseDto { StatusCode = 500, Message = "Scraping failed. Please try again." });
        }
    }

    /// <summary>Returns detailed information about a specific place.</summary>
    /// <param name="google_id">Google Maps data ID for the place (e.g. "0x31752bb396ca47f9:0x237d4c6fae045dd9").</param>
    /// <param name="lat">Latitude of the place.</param>
    /// <param name="lng">Longitude of the place.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Full place details including rating, reviews, hours, photos, and more.</returns>
    /// <response code="200">Place details returned successfully.</response>
    /// <response code="404">Place not found or request failed.</response>
    /// <response code="500">Scraping failed due to an internal error.</response>
    [HttpGet("place", Name = "PlaceDetail")]
    public async Task<ActionResult<PlaceDetailResultDto>> PlaceDetail(
        [FromQuery(Name = "data_id")] string google_id = "0x31752bb396ca47f9:0x237d4c6fae045dd9",
        [FromQuery] double lat = 10.801670806563099,
        [FromQuery] double lng = 106.61743959351683,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _services.Place.PlaceDetailAsync(google_id, lat, lng, ct);
            if (result is null)
                return NotFound(new ErrorResponseDto { StatusCode = 404, Message = "Place not found or request failed" });

            return Ok(new PlaceDetailResultDto { PlaceResult = result });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponseDto { StatusCode = 500, Message = "Scraping failed. Please try again." });
        }
    }
}
