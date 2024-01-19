using System.Text.Json.Serialization;

namespace FacadeAccountCreation.Core.Models.Organisations
{
    public class CheckRegulatorOrganisationExistResponseModel
    {
        [JsonPropertyName("createdOn")]
        public DateTimeOffset CreatedOn { get; set; }

        [JsonPropertyName("externalId")]
        public Guid ExternalId { get; set; }

        [JsonPropertyName("organisationName")]
        public string OrganisationName { get; set; }
    }
}
