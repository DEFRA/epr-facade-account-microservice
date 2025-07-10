namespace FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

[ExcludeFromCodeCoverage]
public class OrganisationTeamMemberModel
{
	public string FirstName { get; set; }

	public string LastName { get; set; }

	public string Email { get; set; }

	public Guid PersonId { get; set; }

	public Guid ConnectionId { get; set; }

	public IEnumerable<TeamMemberEnrolment> Enrolments { get; set; }
}
