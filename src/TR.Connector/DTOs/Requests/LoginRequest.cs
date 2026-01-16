using System.Text.Json.Serialization;

namespace TR.Connector.DTOs.Requests;

internal class LoginRequest
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}
