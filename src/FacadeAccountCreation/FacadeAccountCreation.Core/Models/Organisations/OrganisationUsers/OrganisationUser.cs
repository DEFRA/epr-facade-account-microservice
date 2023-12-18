namespace FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

public class OrganisationUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Guid PersonId { get; set; }
    public int PersonRoleId { get; set; }
    public Guid ConnectionId { get; set; }
    public List<OrganisationUserEnrolment> Enrolments { get; set; }
}
