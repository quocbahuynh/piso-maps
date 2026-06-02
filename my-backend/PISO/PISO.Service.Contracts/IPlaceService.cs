using PISO.Shared.DataTransferObjects.Maps;

namespace PISO.Service.Contracts;

public interface IPlaceService
{
    Task<MapSearchResultDto> AutocompleteAsync(string q, double lat, double lng, CancellationToken ct = default);
    Task<SearchResponseDto> SearchAsync(string q, double lat, double lng, CancellationToken ct = default);
    Task<PlaceDetailFullDto?> PlaceDetailAsync(string googleId, double lat, double lng, CancellationToken ct = default);
}
