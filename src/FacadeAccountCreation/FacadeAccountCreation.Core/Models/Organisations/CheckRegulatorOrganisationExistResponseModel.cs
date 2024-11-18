namespace FacadeAccountCreation.Core.Models.Organisations;

[ExcludeFromCodeCoverage]
public class CheckRegulatorOrganisationExistResponseModel
{
    [JsonPropertyName("createdOn")]
    public DateTimeOffset CreatedOn { get; set; }

    [JsonPropertyName("externalId")]
    public Guid ExternalId { get; set; }

    [JsonPropertyName("organisationName")]
    public string OrganisationName { get; set; }
}