namespace FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;

[ExcludeFromCodeCoverage]
public class TeamMemberEnrolment
{
	public int ServiceRoleId { get; set; }

	public int EnrolmentStatusId { get; set; }

	public string EnrolmentStatusName { get; set; }

	public string ServiceRoleKey { get; set; }

	public string AddedBy { get; set; }
}
