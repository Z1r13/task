using System.Text.Json;
using System.Text.Json.Serialization;

namespace TR.Connector.DTOs.Responses;

internal class UserData
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

internal class UserResponse : ApiResponce<UserData> { }

internal class UserPropertyData
{
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("telephoneNumber")]
    public string? TelephoneNumber { get; set; }

    [JsonPropertyName("isLead")]
    public bool? IsLead { get; set; }

    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

internal class UserPropertyResponse : ApiResponce<UserPropertyData> { }
