namespace PISO.Shared.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class BillableEndpointAttribute : Attribute
{
    public int Cost { get; set; }

    public BillableEndpointAttribute(int cost = 1)
    {
        Cost = cost;
    }
}
