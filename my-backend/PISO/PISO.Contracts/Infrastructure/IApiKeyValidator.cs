namespace PISO.Contracts;

public interface IApiKeyValidator
{
    Task<(bool IsValid, string? Reason, string? UserId, int HourlyRateLimit)> ValidateApiKeyAsync(string apiKey);
}
