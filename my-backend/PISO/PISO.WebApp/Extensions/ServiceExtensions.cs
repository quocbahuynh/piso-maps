using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using PISO.Contracts;
using PISO.Entities.Configuration;
using PISO.HangfireService;
using PISO.LoggerService;
using PISO.GoogleMapService;
using PISO.HttpClient;
using PISO.RedisService;
using PISO.Repository;
using PISO.Service;
using PISO.Service.Contracts;
using PISO.WebApp.Mapping;
using StackExchange.Redis;

namespace PISO.WebApp.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());
        });

    public static void ConfigureIISIntegration(this IServiceCollection services) =>
        services.Configure<IISOptions>(options => { });

    public static void ConfigureLoggerService(this IServiceCollection services) =>
        services.AddSingleton<ILoggerManager, LoggerManager>();

    public static void ConfigureHttpClientManager(this IServiceCollection services) =>
        services.AddSingleton<IHttpClientManager, HttpClientManager>();

    public static void ConfigureGoogleMapService(this IServiceCollection services)
    {
        services.AddScoped<IGoogleMapsRequestExecutor, GoogleMapsRequestExecutor>();
        services.AddScoped<IGoogleMapManager, GoogleMapManager>();
    }

    public static void ConfigureRepositoryManager(this IServiceCollection services) =>
        services.AddScoped<IRepositoryManager, RepositoryManager>();

    public static void ConfigureServiceManager(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<IUsageService>(provider => provider.GetRequiredService<IServiceManager>().Usage);
    }
    public static void ConfigureApiKeyValidator(this IServiceCollection services) =>
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

    public static void ConfigureAutoMapper(this IServiceCollection services) =>
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

    public static void ConfigureVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(opt =>
        {
            opt.ReportApiVersions = true;
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.ApiVersionReader = new HeaderApiVersionReader("api-version");
        });
    }

    public static void ConfigureJWT(this IServiceCollection services, FirebaseConfig firebase)
    {
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.Authority = $"https://securetoken.google.com/{firebase.project_id}";
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://securetoken.google.com/{firebase.project_id}",
                ValidateAudience = true,
                ValidAudience = firebase.project_id,
                ValidateLifetime = true
            };
        });
    }

    public static ConnectionMultiplexer ConfigureRedis(this IServiceCollection services, RedisConfig redis)
    {
        var options = GetRedisOptions(redis.Url);

        var connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        services.AddSingleton<IRedisManager, RedisManager>();
        return connectionMultiplexer;
    }

    public static void ConfigureHangfire(this IServiceCollection services, IConnectionMultiplexer redis)
    {
        services.AddHangfire(config =>
        {
            config.UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseRedisStorage(redis, new RedisStorageOptions
                  {
                      Prefix = "hangfire:",
                      ExpiryCheckInterval = TimeSpan.FromMinutes(15),
                      SucceededListSize = 50,
                      DeletedListSize = 50
                  });
        });

        services.AddHangfireServer(opts =>
        {
            opts.WorkerCount = 3;
        });
        services.AddScoped<IHangfireManager, HangfireManager>();
    }

    private static ConfigurationOptions GetRedisOptions(string redisUrl)
    {
        if (string.IsNullOrEmpty(redisUrl))
        {
            throw new ArgumentNullException(nameof(redisUrl), "Redis URL configuration is missing.");
        }

        if (redisUrl.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase) || redisUrl.StartsWith("redis://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(redisUrl);
            var host = uri.Host;
            var port = uri.Port == -1 ? 6379 : uri.Port;
            var password = uri.UserInfo?.Split(':').LastOrDefault();

            return new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                Password = password,
                Ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase),
                AbortOnConnectFail = false
            };
        }
        else
        {
            var options = ConfigurationOptions.Parse(redisUrl);
            options.AbortOnConnectFail = false;
            return options;
        }
    }

    public static void ConfigureIpRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 20,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(1)
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too Many Requests",
                    message = "IP address rate limit exceeded. Max 20 requests per second allowed."
                }, cancellationToken: token);
            };
        });
    }

    public static void ConfigureResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });
    }
}
