namespace FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;

[ExcludeFromCodeCoverage]
public class ReExAddOrganisationResponse
{
    public required string UserFirstName { get; set; }
    public required string UserLastName { get; set; }
    public required IEnumerable<ServiceRoleResponse> UserServiceRoles { get; set; }

    public required Guid OrganisationId { get; set; }
    public required string ReferenceNumber { get; set; }
    public required IEnumerable<InvitedApprovedUserResponse> InvitedApprovedUsers { get; set; }
}
