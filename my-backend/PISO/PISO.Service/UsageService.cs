using PISO.Contracts;
using PISO.Entities.Models;
using PISO.Service.Contracts;
using PISO.Shared;
using PISO.Shared.DataTransferObjects;

namespace PISO.Service;

internal sealed class UsageService : IUsageService
{
    private readonly IRepositoryManager _repository;
    private readonly IRedisManager _redis;
    private readonly ILoggerManager _logger;

    public UsageService(IRepositoryManager repository, IRedisManager redis, ILoggerManager logger)
    {
        _repository = repository;
        _redis = redis;
        _logger = logger;
    }

    public async Task TrackUsageAsync(string userId, string apiKey, string endpoint, int statusCode, int cost)
    {
        try
        {
            _logger.LogInfo($"Tracking usage for User: {userId}, ApiKey: {apiKey}, Endpoint: {endpoint}");

            // 1. Update Redis Cache Balance
            var cacheKey = RedisKeys.ApiKeyInfo(apiKey);
            var cachedInfo = await _redis.GetAsync<ApiKeyCacheInfo>(cacheKey);

            if (cachedInfo is not null)
            {
                cachedInfo.RemainingCredits -= cost;

                // Invalidate if it drops to 0 or below
                if (cachedInfo.RemainingCredits <= 0)
                {
                    cachedInfo.IsValid = false;
                    cachedInfo.InvalidReason = "Insufficient credits.";
                }

                await _redis.SetAsync(cacheKey, cachedInfo, TimeSpan.FromMinutes(5));
            }

            // 2. Decrement Firestore Balance
            await _repository.Balance.DecrementBalanceAsync(userId, cost);

            // 3. Save Usage Log to Redis (Top 20)
            var logDto = new UsageLogDto
            {
                Timestamp = DateTime.UtcNow,
                ApiKey = apiKey,
                Endpoint = endpoint,
                Status = statusCode,
                Cost = cost
            };
            await _redis.PushToFixedListAsync(RedisKeys.RecentLogs(userId), logDto, 20);

            // 4. Update Daily Usage (Redis Counters)
            var dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var statusKey = statusCode >= 200 && statusCode < 300 ? "success" : "failed";

            await _redis.IncrementAsync(RedisKeys.DailyUsage(userId, dateStr, statusKey));
            await _redis.AddToSetAsync(RedisKeys.ActiveUsers(dateStr), userId);

            _logger.LogInfo($"Successfully tracked usage for User: {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to track usage for user {userId}: {ex.Message}");
            throw; // Rethrow so Hangfire knows the job failed and can retry
        }
    }

    public async Task<System.Collections.Generic.IEnumerable<UsageLogDto>> GetUserLogsAsync(string userId, int limit = 20)
    {
        var logs = await _redis.GetListAsync<UsageLogDto>(RedisKeys.RecentLogs(userId));
        return logs;
    }

    public async Task SyncDailyUsageAsync()
    {
        var dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var activeUsers = await _redis.GetSetMembersAsync(RedisKeys.ActiveUsers(dateStr));

        foreach (var uId in activeUsers)
        {
            var successCount = await _redis.GetLongAsync(RedisKeys.DailyUsage(uId, dateStr, "success"));
            var failedCount = await _redis.GetLongAsync(RedisKeys.DailyUsage(uId, dateStr, "failed"));

            var usage = new DailyUsage
            {
                Date = dateStr,
                SuccessCount = successCount,
                FailedCount = failedCount
            };

            await _repository.DailyUsage.UpdateDailyUsageAsync(uId, usage);
        }
    }

    public async Task<System.Collections.Generic.IEnumerable<DailyUsageDto>> GetDailyUsagesAsync(string userId, int days = 7)
    {
        var usages = await _repository.DailyUsage.GetDailyUsagesAsync(userId, days);
        var dtos = new System.Collections.Generic.List<DailyUsageDto>();

        // Convert Firestore data
        foreach (var u in usages)
        {
            dtos.Add(new DailyUsageDto
            {
                Date = u.Date,
                SuccessCount = u.SuccessCount,
                FailedCount = u.FailedCount
            });
        }

        // Augment with current live data from Redis for today (since it might not be synced yet)
        var dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var todaySuccess = await _redis.GetLongAsync(RedisKeys.DailyUsage(userId, dateStr, "success"));
        var todayFailed = await _redis.GetLongAsync(RedisKeys.DailyUsage(userId, dateStr, "failed"));

        if (todaySuccess > 0 || todayFailed > 0)
        {
            var todayDto = dtos.Find(d => d.Date == dateStr);
            if (todayDto != null)
            {
                // Update with live values
                todayDto.SuccessCount = todaySuccess;
                todayDto.FailedCount = todayFailed;
            }
            else
            {
                // Add new entry for today
                dtos.Insert(0, new DailyUsageDto
                {
                    Date = dateStr,
                    SuccessCount = todaySuccess,
                    FailedCount = todayFailed
                });
            }
        }

        return dtos;
    }
}
