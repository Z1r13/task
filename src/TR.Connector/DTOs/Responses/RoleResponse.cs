using System.Text.Json.Serialization;

namespace TR.Connector.DTOs.Responses;

internal class RoleData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("corporatePhoneNumber")]
    public string? CorporatePhoneNumber { get; set; }
}

internal class RoleListResponse : ApiResponce<List<RoleData>> { }
