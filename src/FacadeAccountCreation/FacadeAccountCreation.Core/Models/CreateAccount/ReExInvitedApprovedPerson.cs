namespace FacadeAccountCreation.Core.Models.CreateAccount;

/// <summary>
/// Re-Ex invited approved person model
/// </summary>
public class ReExInvitedApprovedPerson
{
    public Guid? Id { get; set; }

    /// <summary>
    ///Role/Job for approved person i.e. Director, CompanySecretary
    /// </summary>
    public string? Role { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string TelephoneNumber { get; set; }

    public string Email { get; set; }

    public string? InviteToken { get; set; }
}
