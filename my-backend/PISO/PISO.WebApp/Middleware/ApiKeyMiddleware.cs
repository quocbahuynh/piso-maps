using PISO.Contracts;
using PISO.Shared.Attributes;
using PISO.Shared.DataTransferObjects;

namespace PISO.WebApp.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyValidator apiKeyValidator)
    {
        var endpoint = context.GetEndpoint();
        var billableAttr = endpoint?.Metadata.GetMetadata<BillableEndpointAttribute>();

        if (billableAttr != null)
        {
            if (!context.Request.Headers.TryGetValue("x-api-key", out var extractedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { StatusCode = 401, Message = "API Key is missing." });
                return;
            }

            var apiKey = extractedApiKey.ToString();
            var (isValid, reason, userId, hourlyRateLimit) = await apiKeyValidator.ValidateApiKeyAsync(apiKey);

            if (!isValid)
            {
                context.Response.StatusCode = reason == "Insufficient credits."
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status401Unauthorized;

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { StatusCode = context.Response.StatusCode, Message = reason ?? "Invalid API key." });
                return;
            }

            if (userId is not null)
            {
                context.Items["UserId"] = userId;
                context.Items["ApiKey"] = apiKey;
                context.Items["Cost"] = billableAttr.Cost;
                context.Items["HourlyRateLimit"] = hourlyRateLimit;
            }
        }

        await _next(context);
    }
}
