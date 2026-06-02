using System.Collections.Concurrent;
using PISO.Contracts;
using PISO.Shared;
using PISO.Shared.DataTransferObjects;

namespace PISO.Service;

public class ApiKeyValidator : IApiKeyValidator
{
    private readonly IRepositoryManager _repository;
    private readonly IRedisManager _redis;
    private readonly IHangfireManager _hangfire;
    private readonly ILoggerManager _logger;

    private static readonly ConcurrentDictionary<string, Task<(bool IsValid, string? Reason, string? UserId, int HourlyRateLimit)>> _activeLookups = new();

    public ApiKeyValidator(
        IRepositoryManager repository,
        IRedisManager redis,
        IHangfireManager hangfire,
        ILoggerManager logger)
    {
        _repository = repository;
        _redis = redis;
        _hangfire = hangfire;
        _logger = logger;
    }

    public async Task<(bool IsValid, string? Reason, string? UserId, int HourlyRateLimit)> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (false, "API Key is missing.", null, 0);
        }

        try
        {
            var cacheKey = RedisKeys.ApiKeyInfo(apiKey);
            var cachedInfo = await _redis.GetAsync<ApiKeyCacheInfo>(cacheKey);

            if (cachedInfo is not null)
            {
                if (!cachedInfo.IsValid)
                {
                    return (false, cachedInfo.InvalidReason, null, 0);
                }

                if (cachedInfo.Status != "active")
                {
                    return (false, "User account is not active.", null, 0);
                }

                if (cachedInfo.RemainingCredits <= 0)
                {
                    return (false, "Insufficient credits.", null, 0);
                }

                return (true, null, cachedInfo.UserId, cachedInfo.HourlyRateLimit);
            }

            var lookupTask = _activeLookups.GetOrAdd(apiKey, key => FetchFromDbAndCacheAsync(key, cacheKey));

            try
            {
                return await lookupTask;
            }
            finally
            {
                _activeLookups.TryRemove(apiKey, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while validating API Key: {ex.Message}");
            return (false, "An internal error occurred during verification.", null, 0);
        }
    }

    private async Task<(bool IsValid, string? Reason, string? UserId, int HourlyRateLimit)> FetchFromDbAndCacheAsync(string apiKey, string cacheKey)
    {
        try
        {
            var globalKey = await _repository.GlobalApiKey.GetByIdAsync(apiKey);
            if (globalKey is null)
            {
                var negCache = new ApiKeyCacheInfo { IsValid = false, InvalidReason = "Invalid API Key." };
                await _redis.SetAsync(cacheKey, negCache, TimeSpan.FromMinutes(5));
                return (false, "Invalid API Key.", null, 0);
            }

            var userTask = _repository.User.GetByIdAsync(globalKey.UserId);
            var balanceTask = _repository.Balance.GetBalanceAsync(globalKey.UserId);
            var systemConfigTask = _repository.SystemConfig.GetPlansAsync();

            await Task.WhenAll(userTask, balanceTask, systemConfigTask);

            var user = await userTask;
            if (user is null)
            {
                var negCache = new ApiKeyCacheInfo { IsValid = false, InvalidReason = "User associated with API key not found." };
                await _redis.SetAsync(cacheKey, negCache, TimeSpan.FromMinutes(5));
                return (false, "User associated with API key not found.", null, 0);
            }

            if (user.Status != "active")
            {
                var negCache = new ApiKeyCacheInfo { IsValid = false, InvalidReason = "User account is not active.", UserId = user.UserId, Status = user.Status };
                await _redis.SetAsync(cacheKey, negCache, TimeSpan.FromSeconds(30));
                return (false, "User account is not active.", null, 0);
            }

            var balance = await balanceTask;
            if (balance is null)
            {
                var negCache = new ApiKeyCacheInfo { IsValid = false, InvalidReason = "Balance information not found.", UserId = user.UserId, Status = user.Status };
                await _redis.SetAsync(cacheKey, negCache, TimeSpan.FromSeconds(30));
                return (false, "Balance information not found.", null, 0);
            }

            var systemConfig = await systemConfigTask;
            int hourlyRateLimit = 100; // default
            if (systemConfig != null)
            {
                hourlyRateLimit = user.Plan == "Gói Developer" ? systemConfig.Developer.HourlyRateLimit : systemConfig.Free.HourlyRateLimit;
            }

            var remainingCredits = balance.RemainingCredits;
            if (remainingCredits <= 0)
            {
                var negCache = new ApiKeyCacheInfo
                {
                    IsValid = false,
                    InvalidReason = "Insufficient credits.",
                    UserId = user.UserId,
                    Status = user.Status,
                    Plan = user.Plan,
                    RemainingCredits = remainingCredits,
                    HourlyRateLimit = hourlyRateLimit
                };
                await _redis.SetAsync(cacheKey, negCache, TimeSpan.FromSeconds(30));
                return (false, "Insufficient credits.", null, hourlyRateLimit);
            }

            var newCacheInfo = new ApiKeyCacheInfo
            {
                UserId = user.UserId,
                Plan = user.Plan,
                Status = user.Status,
                RemainingCredits = remainingCredits,
                HourlyRateLimit = hourlyRateLimit,
                IsValid = true
            };
            await _redis.SetAsync(cacheKey, newCacheInfo, TimeSpan.FromMinutes(5));

            return (true, null, user.UserId, hourlyRateLimit);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Database fetch failed in Single-flight for API Key: {ex.Message}");
            throw;
        }
    }
}
