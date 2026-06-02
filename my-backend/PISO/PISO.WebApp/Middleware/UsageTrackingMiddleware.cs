using PISO.Contracts;
using PISO.Service.Contracts;

namespace PISO.WebApp.Middleware;

public class UsageTrackingMiddleware
{
    private readonly RequestDelegate _next;

    public UsageTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IHangfireManager hangfire)
    {
        await _next(context);

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userId &&
                context.Items.TryGetValue("ApiKey", out var apiKeyObj) && apiKeyObj is string apiKeyForJob &&
                context.Items.TryGetValue("Cost", out var costObj) && costObj is int cost)
            {
                hangfire.Enqueue<IUsageService>(
                    usage => usage.TrackUsageAsync(userId, apiKeyForJob, context.Request.Path, context.Response.StatusCode, cost)
                );
            }
        }
    }
}
