namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class ReExNotificationModel
{
    public string UserId { get; set; }
    public string UserFirstName { get; set; }
    public string UserLastName { get; set; }
    public string UserEmail { get; set; }
    public string OrganisationId { get; set; }
    public string OrganisationExternalId { get; set; }
    public string CompanyName { get; set; }
    public string CompanyHouseNumber { get; set; }

    /// <summary>
    ///  Invited approved persons detail
    /// </summary>
    public List<ReExInvitedApprovedPerson> ReExInvitedApprovedPersons { get; set; } = [];
}
