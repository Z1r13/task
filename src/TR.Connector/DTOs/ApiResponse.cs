using System.Text.Json.Serialization;

namespace TR.Connector.DTOs;

internal abstract class ApiResponse
{
    [JsonPropertyName("sucess")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }
}

internal class ApiResponce<T> : ApiResponse
{
    [JsonPropertyName("data")]
    public T Data { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
