namespace PISO.Shared.DataTransferObjects;

public class ApiKeyCacheInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long RemainingCredits { get; set; }
    public int HourlyRateLimit { get; set; }
    public bool IsValid { get; set; } = true;
    public string? InvalidReason { get; set; }
}
