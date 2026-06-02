namespace PISO.Contracts;

public interface IGoogleMapsRequestExecutor
{
    Task<string?> ExecuteSearchRequestAsync(string q, double lat, double lng, CancellationToken ct);
    Task<string?> ExecutePlaceRequestAsync(string googleId, double lat, double lng, CancellationToken ct);
    Task<string?> ExecuteAutocompleteRequestAsync(string q, double lat, double lng, CancellationToken ct);
}
