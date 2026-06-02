using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects;

public class ErrorResponseDto
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
