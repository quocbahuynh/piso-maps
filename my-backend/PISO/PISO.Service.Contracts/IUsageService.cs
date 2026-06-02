namespace PISO.Service.Contracts;

public interface IUsageService
{
    Task TrackUsageAsync(string userId, string apiKey, string endpoint, int statusCode, int cost);
    Task<System.Collections.Generic.IEnumerable<PISO.Shared.DataTransferObjects.UsageLogDto>> GetUserLogsAsync(string userId, int limit = 20);
    Task SyncDailyUsageAsync();
    Task<System.Collections.Generic.IEnumerable<PISO.Shared.DataTransferObjects.DailyUsageDto>> GetDailyUsagesAsync(string userId, int days = 7);
}
