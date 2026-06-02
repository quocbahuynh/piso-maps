namespace PISO.Shared.DataTransferObjects;

public class UserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public long MaxCredits { get; set; }
    public long RemainingCredits { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
