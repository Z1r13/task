using System.Text.Json.Serialization;

namespace TR.Connector.DTOs.Responses;

internal class TokenData
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

internal class TokenResponse : ApiResponce<TokenData> { }
