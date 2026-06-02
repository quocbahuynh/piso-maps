using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using NLog;
using PISO.Contracts;
using PISO.Entities.Configuration;
using PISO.Repository;
using PISO.WebApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

var firebase = builder.Configuration.GetSection("Firebase").Get<FirebaseConfig>()!;
var redis = builder.Configuration.GetSection("Redis").Get<RedisConfig>()!;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PISO Maps API",
        Version = "v1",
        Description = "Google Maps scraping API — autocomplete, search, and place detail endpoints."
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "API key required for Maps endpoints."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
        }] = Array.Empty<string>()
    });

    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
        c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
});
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureHttpClientManager();
builder.Services.ConfigureGoogleMapService();
builder.Services.ConfigureVersioning();
builder.Services.ConfigureJWT(firebase);
var multiplexer = builder.Services.ConfigureRedis(redis);
builder.Services.ConfigureHangfire(multiplexer);
builder.Services.ConfigureIpRateLimiting();
builder.Services.ConfigureResponseCompression();
builder.Services.AddControllers(config =>
{
    config.Filters.Add(new ProducesAttribute("application/json"));
})
.AddApplicationPart(typeof(PISO.Presentation.AssemblyReference).Assembly);

var firebaseCredentialsJson = JsonSerializer.Serialize(firebase);
using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(firebaseCredentialsJson));
var serviceCredential = await CredentialFactory.FromStreamAsync<ServiceAccountCredential>(stream, CancellationToken.None);
GoogleCredential credential = serviceCredential.ToGoogleCredential();

var firestoreDb = new FirestoreDbBuilder
{
    ProjectId = firebase.project_id,
    GoogleCredential = credential
}.Build();
builder.Services.AddSingleton(firestoreDb);
builder.Services.AddScoped<RepositoryContext>();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureApiKeyValidator();
builder.Services.ConfigureAutoMapper();

await RepositoryContextSeed.SeedAsync(firestoreDb);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseResponseCompression();
app.UseCors("CorsPolicy");

app.UseRouting();
app.UseRateLimiter();

app.UseMiddleware<PISO.WebApp.Middleware.ApiKeyMiddleware>();
app.UseMiddleware<PISO.WebApp.Middleware.RateLimitingMiddleware>();
app.UseMiddleware<PISO.WebApp.Middleware.UsageTrackingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<PISO.Service.Contracts.IUsageService>(
    "SyncDailyUsage",
    usageService => usageService.SyncDailyUsageAsync(),
    Cron.Hourly);

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();