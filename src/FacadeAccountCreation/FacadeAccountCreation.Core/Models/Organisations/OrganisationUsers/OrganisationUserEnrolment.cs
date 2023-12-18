using FacadeAccountCreation.Core.Enums;
using System.Text.Json.Serialization;

namespace FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

public class OrganisationUserEnrolment
{
    [JsonPropertyName("EnrolmentStatusId")]
    public EnrolmentStatus EnrolmentStatus { get; set; }
    public int ServiceRoleId { get; set; }
}
