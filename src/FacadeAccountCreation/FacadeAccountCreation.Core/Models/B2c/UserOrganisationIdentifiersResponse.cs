using System.Text.Json.Serialization;

namespace FacadeAccountCreation.Core.Models.B2c;

[ExcludeFromCodeCoverage]
public class UserOrganisationIdentifiersResponse
{
    public const string ApiVersion = "1.0.0";

    [JsonPropertyName("version")]
    public string Version { get; } = ApiVersion;

    [JsonPropertyName("action")]
    public string Action { get; set; } = "Continue";

    [JsonPropertyName("orgIds")]
    public string OrganisationIds { get; set; }
}
