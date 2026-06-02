using System.Net;
using Microsoft.Extensions.Configuration;
using PISO.Contracts;
using RestSharp;

namespace PISO.HttpClient;

public class HttpClientManager : IHttpClientManager
{
    private readonly ILoggerManager _logger;
    private readonly RestClient[] _pool;

    public HttpClientManager(IConfiguration configuration, ILoggerManager logger)
    {
        _logger = logger;

        var proxySection = configuration.GetSection("Proxy");
        var proxyFilePath = proxySection["FilePath"] ?? "";
        var enabled = bool.TryParse(proxySection["Enabled"], out var e) && e;

        var clients = new List<RestClient>();

        if (enabled && !string.IsNullOrWhiteSpace(proxyFilePath) && File.Exists(proxyFilePath))
        {
            var addresses = File.ReadAllLines(proxyFilePath)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#'))
                .ToArray();

            foreach (var addr in addresses)
            {
                var proxy = CreateWebProxy(addr);
                if (proxy is not null)
                {
                    clients.Add(CreateRestClient(proxy));
                    logger.LogInfo($"Added proxy client: {addr}");
                }
            }
        }

        clients.Add(CreateRestClient(null));
        _pool = clients.ToArray();

        logger.LogInfo($"Google Maps HTTP pool initialized with {_pool.Length} clients.");
    }

    public RestClient CreateClient(string baseUrl)
    {
        var idx = Random.Shared.Next(_pool.Length);
        return _pool[idx];
    }

    private static RestClient CreateRestClient(IWebProxy? proxy)
    {
        var options = new RestClientOptions("https://www.google.com")
        {
            Proxy = proxy,
            ConfigureMessageHandler = inner =>
            {
                if (inner is SocketsHttpHandler h)
                {
                    h.PooledConnectionLifetime = TimeSpan.FromMinutes(5);
                    h.MaxConnectionsPerServer = 10;
                    h.EnableMultipleHttp2Connections = true;
                }
                return inner;
            }
        };
        return new RestClient(options);
    }

    private static IWebProxy? CreateWebProxy(string proxyAddress)
    {
        if (!proxyAddress.Contains("://", StringComparison.Ordinal))
            proxyAddress = $"http://{proxyAddress}";

        if (!Uri.TryCreate(proxyAddress, UriKind.Absolute, out var proxyUri))
            return null;

        var uriProxy = new WebProxy(proxyUri);

        if (!string.IsNullOrWhiteSpace(proxyUri.UserInfo))
        {
            var credentialParts = proxyUri.UserInfo.Split(':', 2);
            uriProxy.Credentials = new NetworkCredential(
                Uri.UnescapeDataString(credentialParts[0]),
                credentialParts.Length > 1 ? Uri.UnescapeDataString(credentialParts[1]) : "");
        }

        return uriProxy;
    }
}
