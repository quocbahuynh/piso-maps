namespace PISO.Shared;

public static class RedisKeys
{
    public static string ApiKeyInfo(string apiKey) => $"apikey:info:{apiKey}";

    public static string RateLimit(string apiKey, DateTime time) =>
        $"ratelimit:{apiKey}:{time:yyyy-MM-dd-HH}";

    public static string RecentLogs(string userId) => $"recentlogs:{userId}";

    public static string DailyUsage(string userId, string dateStr, string status) =>
        $"dailyusage:{userId}:{dateStr}:{status}";

    public static string ActiveUsers(string dateStr) => $"active_users:{dateStr}";

    public static string AllPlans => "all_plans";
}
