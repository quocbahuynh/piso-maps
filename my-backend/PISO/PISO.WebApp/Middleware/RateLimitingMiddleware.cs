using PISO.Contracts;
using PISO.Shared;
using PISO.Shared.DataTransferObjects;

namespace PISO.WebApp.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRedisManager redis)
    {
        // Only apply rate limiting if the request passed API key validation and is billable
        if (context.Items.TryGetValue("ApiKey", out var apiKeyObj) && apiKeyObj is string apiKey &&
            context.Items.TryGetValue("HourlyRateLimit", out var limitObj) && limitObj is int hourlyLimit &&
            context.Items.TryGetValue("Cost", out var costObj))
        {
            var now = DateTime.UtcNow;
            var rateLimitKey = RedisKeys.RateLimit(apiKey, now);

            var currentCount = await redis.IncrementAsync(rateLimitKey, TimeSpan.FromHours(1));

            if (currentCount > hourlyLimit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                {
                    StatusCode = 429,
                    Message = $"Rate limit exceeded. You have exceeded your plan's limit of {hourlyLimit} requests per hour."
                });
                return;
            }

            // Expose remaining requests in headers if needed
            context.Response.Headers["X-RateLimit-Limit"] = hourlyLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, hourlyLimit - currentCount).ToString();
        }

        await _next(context);
    }
}
