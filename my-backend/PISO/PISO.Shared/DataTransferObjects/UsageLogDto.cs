namespace PISO.Shared.DataTransferObjects;

public class UsageLogDto
{
    public DateTime Timestamp { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Cost { get; set; }
}
