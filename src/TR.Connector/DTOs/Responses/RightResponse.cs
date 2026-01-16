using System.Text.Json.Serialization;

namespace TR.Connector.DTOs.Responses;

internal class RightData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal class RightListResponse : ApiResponce<List<RightData>> { }
