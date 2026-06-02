namespace PISO.Shared.DataTransferObjects;

public class PlansResponseDto
{
    public PlanConfigDto Free { get; set; } = new();
    public PlanConfigDto Developer { get; set; } = new();
}
