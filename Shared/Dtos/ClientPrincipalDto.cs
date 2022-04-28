using System.Text.Json.Serialization;

namespace SharedLibrary.Dtos;

public class ClientPrincipalDto
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; set; }

    [JsonPropertyName("userDetails")]
    public string UserDetails { get; set; }

    [JsonPropertyName("userRoles")]
    public IEnumerable<string> UserRoles { get; set; }
}