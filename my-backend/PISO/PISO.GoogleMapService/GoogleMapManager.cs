using PISO.Contracts;
using PISO.Shared.DataTransferObjects.Maps;
using static PISO.GoogleMapService.GoogleMapsParser;

namespace PISO.GoogleMapService;

public class GoogleMapManager : IGoogleMapManager
{
    private readonly ILoggerManager _logger;
    private readonly IGoogleMapsRequestExecutor _executor;

    public GoogleMapManager(ILoggerManager logger, IGoogleMapsRequestExecutor executor)
    {
        _logger = logger;
        _executor = executor;
    }

    public async Task<MapSearchResultDto> AutocompleteAsync(string query, double lat, double lng, CancellationToken ct = default)
    {
        _logger.LogInfo($"Place autocomplete: q={query}, lat={lat}, lng={lng}");

        var content = await _executor.ExecuteAutocompleteRequestAsync(query, lat, lng, ct);
        if (content is null)
            return new MapSearchResultDto();

        var result = ParseResponse(content, _logger);
        _logger.LogInfo($"Found {result.Suggestions.Count} suggestions");
        return result;
    }

    public async Task<SearchResponseDto> SearchAsync(string query, double lat, double lng, CancellationToken ct = default)
    {
        _logger.LogInfo($"Place search: q={query}, lat={lat}, lng={lng}");

        var content = await _executor.ExecuteSearchRequestAsync(query, lat, lng, ct);
        if (content is null)
            return new SearchResponseDto();

        var result = ParseSearchResponse(content, _logger);
        _logger.LogInfo($"Found {result.Results.Count} places");
        return result;
    }

    public async Task<PlaceDetailFullDto?> PlaceDetailAsync(string googleId, double lat, double lng, CancellationToken ct = default)
    {
        _logger.LogInfo($"Place detail: id={googleId}, lat={lat}, lng={lng}");

        var content = await _executor.ExecutePlaceRequestAsync(googleId, lat, lng, ct);
        if (content is null)
            return null;

        var result = ParsePlaceFullResponse(content, _logger);
        if (result is null)
        {
            _logger.LogWarn($"No place found for id={googleId}");
            return null;
        }

        _logger.LogInfo($"Extracted place: name={result.Name}, id={result.Id}");
        return result;
    }
}
