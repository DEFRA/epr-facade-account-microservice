namespace FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

[ExcludeFromCodeCoverage]
public class OrganisationUserEnrolment
{
    [JsonPropertyName("EnrolmentStatusId")]
    public EnrolmentStatus EnrolmentStatus { get; set; }
    public int ServiceRoleId { get; set; }
}
