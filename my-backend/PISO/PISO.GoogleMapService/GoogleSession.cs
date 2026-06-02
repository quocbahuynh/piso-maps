namespace PISO.GoogleMapService;

public class GoogleSession
{
    public string Psi { get; init; } = "";
    public string Cookie { get; init; } = "";
    public string UserAgent { get; init; } = "";
    public string SecChUa { get; init; } = "";
    public string SecChUaPlatform { get; init; } = "";
    public DateTime UpdatedAt { get; init; }
}
