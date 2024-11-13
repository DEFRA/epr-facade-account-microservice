namespace FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

[ExcludeFromCodeCoverage]
public class OrganisationUserModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int PersonRoleId { get; set; }
    public Guid PersonId { get; set; }
    public int ServiceRoleId { get; set; }
    public string ServiceRoleKey { get; set; }
    public Guid ConnectionId { get; set; }
    public EnrolmentStatus EnrolmentStatus { get; set; }
}
