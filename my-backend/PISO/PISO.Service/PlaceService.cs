using PISO.Contracts;
using PISO.Service.Contracts;
using PISO.Shared.DataTransferObjects.Maps;

namespace PISO.Service;

public sealed class PlaceService : IPlaceService
{
    private readonly IGoogleMapManager _googleMap;

    public PlaceService(IGoogleMapManager googleMap)
    {
        _googleMap = googleMap;
    }

    public Task<MapSearchResultDto> AutocompleteAsync(string q, double lat, double lng, CancellationToken ct = default) =>
        _googleMap.AutocompleteAsync(q, lat, lng, ct);

    public Task<SearchResponseDto> SearchAsync(string q, double lat, double lng, CancellationToken ct = default) =>
        _googleMap.SearchAsync(q, lat, lng, ct);

    public Task<PlaceDetailFullDto?> PlaceDetailAsync(string googleId, double lat, double lng, CancellationToken ct = default) =>
        _googleMap.PlaceDetailAsync(googleId, lat, lng, ct);
}
