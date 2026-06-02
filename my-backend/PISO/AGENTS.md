# AGENTS.md — PISO Project Guide

## Build & Run

```bash
dotnet build                          # Build the solution
dotnet run --project PISO.WebApp      # Run the API
dotnet watch run --project PISO.WebApp # Hot-reload development
```

## Project Structure

```
PISO.WebApp/              # ASP.NET Core host, middleware, Program.cs
PISO.Presentation/        # Controllers (MapsController, UsersController, RootController)
PISO.Service/             # Service implementations
PISO.Service.Contracts/   # Service interfaces
PISO.Contracts/           # Repository & infrastructure interfaces
PISO.Repository/          # Repository implementations (Firestore)
PISO.Entities/            # Domain models, config classes, response models
PISO.Shared/              # DTOs, constants, attributes
PISO.GoogleMapService/    # Google Maps scraping (executor, parser, session helper)
PISO.HttpClient/          # HTTP client pool & proxy rotation
PISO.RedisService/        # Redis caching
PISO.LoggerService/       # NLog logging
PISO.HangfireService/     # Hangfire job scheduling
```

## Code Conventions

- .NET 8, C#, ASP.NET Core, ImplicitUsings enabled
- `Lazy<T>` for thread-safe lazy initialization in managers (RepositoryManager, ServiceManager)
- Classes not DI-registered: `ProxyHelper`, `SessionHelper`, `HttpClientManager`
- `Random.Shared` (thread-safe since .NET 6+) for random selection — never custom `Random` instances
- Header-based API versioning (`[ApiVersion("1.0")]`) — not URL-based
- Global `[Produces("application/json")]` filter — no content negotiation (JSON only)
- Response models in `PISO.Entities/Responses/` — `ApiBaseResponse` (abstract, `Success`), `ApiErrorResponse` (`StatusCode`, `Message`)
- Entity DTOs in `PISO.Shared/DataTransferObjects/`
- `RestSharp` v112+ with `ConfigureMessageHandler` using `SocketsHttpHandler`
- `PooledConnectionLifetime=5min`, `MaxConnectionsPerServer=10`
- `System.Text.Json` is NOT implicitly available in class libraries — add explicit `using System.Text.Json.Serialization;`
- Snake_case JSON via `[JsonPropertyName("...")]`

## Key Patterns

- **Error responses**: always return `ApiErrorResponse { Success = false, StatusCode = ..., Message = "..." }`
- **Request pipeline**: `ApiKeyMiddleware` → `RateLimitingMiddleware` → `UsageTrackingMiddleware` → Controller
- **Scraping**: `GoogleMapsRequestExecutor` (scoped) builds and sends requests; `GoogleMapsParser` (1160-line monolith) parses responses
- **Session rotation**: `SessionHelper` loads from `Data/google-sessions.json`, picks random session via `Random.Shared` on every `AddRequireHeaders` call
- **Proxy rotation**: `HttpClientManager` creates a pool of `RestClient` at startup (one per proxy + direct), each with its own `SocketsHttpHandler`
- **Fail-hard**: `SessionHelper` throws on missing/empty/parse-error file (no graceful degradation)

## Data Files

- `PISO.WebApp/Data/proxy.txt` — gitignored, format: `host:port:user:pass` per line
- `PISO.WebApp/Data/google-sessions.json` — gitignored, array of `{ psi, cookie, userAgent, secChUa, secChUaPlatform, updatedAt }`

## Middleware Pipeline Order

1. `ApiKeyMiddleware` — validates `x-api-key`, sets `UserId`, `ApiKey`, `Cost`, `HourlyRateLimit` in `HttpContext.Items`
2. `RateLimitingMiddleware` — checks Redis counter against plan limit
3. `UsageTrackingMiddleware` — enqueues Hangfire job after successful response
4. `ExceptionMiddlewareExtensions` — global error handler, returns `ApiErrorResponse`

## Common Tasks

- **Add new endpoint**: create action in appropriate controller, add service method, ensure `[BillableEndpoint(Cost = N)]` if applicable
- **Add new API response type**: add class in `PISO.Entities/Responses/` extending `ApiBaseResponse`
- **Remove content negotiation dependency**: ensure `[Produces("application/json")]` is on controller or globally configured
- **Format**: `dotnet format` (auto-fix whitespace, unused usings etc.)

## .editorconfig

Only rule: `IDE0005` (unused usings) is a warning. No other custom rules.
