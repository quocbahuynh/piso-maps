using System.Globalization;
using Microsoft.Extensions.Configuration;
using PISO.Contracts;
using PISO.HttpClient;
using RestSharp;
using static PISO.GoogleMapService.GoogleMapsRequestBuilder;

namespace PISO.GoogleMapService;

public class GoogleMapsRequestExecutor : IGoogleMapsRequestExecutor
{
    private readonly SessionHelper _sessionHelper;
    private readonly ILoggerManager _logger;
    private readonly IHttpClientManager _httpClientManager;

    private static readonly HashSet<int> NonRetryableStatusCodes = [400, 401, 403, 404];

    public GoogleMapsRequestExecutor(ILoggerManager logger, IConfiguration configuration, IHttpClientManager httpClientManager)
    {
        _logger = logger;
        var sessionsSection = configuration.GetSection("GoogleSessions");
        var sessionsFilePath = sessionsSection["FilePath"] ?? "Data/google-sessions.json";
        _sessionHelper = new SessionHelper(sessionsFilePath, logger);
        _httpClientManager = httpClientManager;
    }

    public async Task<string?> ExecuteSearchRequestAsync(string q, double lat, double lng, CancellationToken ct)
    {
        var latStr = lat.ToString(CultureInfo.InvariantCulture);
        var lngStr = lng.ToString(CultureInfo.InvariantCulture);
        var pb = BuildSearchPb(latStr, lngStr);

        var request = new RestRequest("/search", Method.Get);
        request.AddParameter("tbm", "map");
        request.AddParameter("q", q);
        request.AddParameter("oq", q);

        AddRequireHeaders(request, pb);

        return await ExecuteAsync(request, "search", ct);
    }

    public async Task<string?> ExecutePlaceRequestAsync(string googleId, double lat, double lng, CancellationToken ct)
    {
        var latStr = lat.ToString(CultureInfo.InvariantCulture);
        var lngStr = lng.ToString(CultureInfo.InvariantCulture);
        var pb = BuildPlacePb(googleId, latStr, lngStr);

        var request = new RestRequest("/maps/preview/place", Method.Get);

        AddRequireHeaders(request, pb);

        return await ExecuteAsync(request, "place detail", ct);
    }

    public async Task<string?> ExecuteAutocompleteRequestAsync(string q, double lat, double lng, CancellationToken ct)
    {
        var latStr = lat.ToString(CultureInfo.InvariantCulture);
        var lngStr = lng.ToString(CultureInfo.InvariantCulture);
        var pb = BuildPb(latStr, lngStr);

        var request = new RestRequest("/s", Method.Get);
        request.AddParameter("gs_ri", "maps");
        request.AddParameter("suggest", "p");
        request.AddParameter("q", q);
        request.AddParameter("ech", "8");

        AddRequireHeaders(request, pb);

        return await ExecuteAsync(request, "autocomplete", ct);
    }

    private void AddRequireHeaders(RestRequest request, string pb)
    {
        request.AddParameter("authuser", "0");
        request.AddParameter("hl", "vi");
        request.AddParameter("gl", "vn");
        request.AddParameter("pb", pb);
        

        var session = _sessionHelper.GetRandomSession();
        request.AddParameter("psi", session.Psi);

        request.AddHeader("accept", "*/*");
        request.AddHeader("accept-language", "vi-VN,vi;q=0.9");
        request.AddHeader("available-dictionary", ":tgytezwj5mm4EtwdVjnax+p7F5SNXo0W2binmyUEgT0=:");
        request.AddHeader("downlink", "10");
        request.AddHeader("priority", "u=1, i");
        request.AddHeader("referer", "https://www.google.com/");
        request.AddHeader("rtt", "100");
        request.AddHeader("sec-ch-ua", session.SecChUa);
        request.AddHeader("sec-ch-ua-mobile", "?0");
        request.AddHeader("sec-ch-ua-platform", session.SecChUaPlatform);
        request.AddHeader("sec-fetch-dest", "empty");
        request.AddHeader("sec-fetch-mode", "cors");
        request.AddHeader("sec-fetch-site", "same-origin");
        request.AddHeader("user-agent", session.UserAgent);
        request.AddHeader("x-browser-channel", "stable");
        request.AddHeader("x-browser-copyright", "Copyright 2026 Google LLC. All Rights Reserved.");
        request.AddHeader("x-browser-validation", "bFG+f1f2OgVN3Rwx8hA6aEFwVUQ=");
        request.AddHeader("x-browser-year", "2026");
        request.AddHeader("x-maps-diversion-context-bin", "CAE=");
        request.AddHeader("Cookie", session.Cookie);
    }

    private async Task<string?> ExecuteAsync(RestRequest request, string operationName, CancellationToken ct)
    {
        var client = _httpClientManager.CreateClient("https://www.google.com");

        var maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var response = await client.ExecuteAsync(request, ct);

            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                if (attempt > 1)
                    _logger.LogInfo($"Google {operationName} succeeded on attempt {attempt}.");

                if (response.Content!.Length > 100_000)
                    _logger.LogDebug($"Google {operationName} returned {response.Content.Length} chars.");

                return response.Content;
            }

            var statusCode = (int)response.StatusCode;
            var isLastAttempt = attempt == maxAttempts;

            if (statusCode >= 400 && statusCode < 500 && NonRetryableStatusCodes.Contains(statusCode))
            {
                if (statusCode == 400 && response.Content?.Contains("DOCTYPE") == true)
                    _logger.LogWarn($"Google {operationName} returned 400 (bot detection). Cookie may be expired.");
                else
                    _logger.LogWarn($"Google {operationName} returned {response.StatusCode}. Not retrying.");

                if (!string.IsNullOrEmpty(response.Content))
                    _logger.LogDebug($"Response snippet: {Truncate(response.Content, 500)}");

                return null;
            }

            if (!isLastAttempt)
            {
                var delay = TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt - 1));
                _logger.LogWarn($"Google {operationName} attempt {attempt} failed ({response.StatusCode}). Retrying in {delay.TotalMilliseconds}ms...");
                await Task.Delay(delay, ct);
            }
            else
            {
                _logger.LogWarn($"Google {operationName} failed after {maxAttempts} attempts.");
                if (!string.IsNullOrEmpty(response.Content))
                    _logger.LogDebug($"Response snippet: {Truncate(response.Content, 500)}");
                return null;
            }
        }

        return null;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
