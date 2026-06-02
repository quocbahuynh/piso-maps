namespace PISO.Shared.DataTransferObjects;

public class PlanConfigDto
{
    public string Name { get; set; } = string.Empty;
    public long MaxCredits { get; set; }
    public long Price { get; set; }
}
