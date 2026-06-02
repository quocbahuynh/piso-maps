using PISO.Shared.DataTransferObjects.Maps;

namespace PISO.Contracts;

public interface IGoogleMapManager
{
    Task<MapSearchResultDto> AutocompleteAsync(string query, double lat, double lng, CancellationToken ct = default);
    Task<SearchResponseDto> SearchAsync(string query, double lat, double lng, CancellationToken ct = default);
    Task<PlaceDetailFullDto?> PlaceDetailAsync(string googleId, double lat, double lng, CancellationToken ct = default);
}
