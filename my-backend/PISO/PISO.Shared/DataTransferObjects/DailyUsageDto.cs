namespace PISO.Shared.DataTransferObjects;

public class DailyUsageDto
{
    public string Date { get; set; } = string.Empty;
    public long SuccessCount { get; set; }
    public long FailedCount { get; set; }
}
