using RestSharp;

namespace PISO.Contracts;

public interface IHttpClientManager
{
    RestClient CreateClient(string baseUrl);
}
