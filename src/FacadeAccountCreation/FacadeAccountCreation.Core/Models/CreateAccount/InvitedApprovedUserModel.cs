namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class InvitedApprovedUserModel
{
    public PersonModel Person { get; set; }

    /// <summary>
    /// Role/Job title can be Director, CompanySecretary
    /// </summary>
    public string JobTitle { get; set; }
}
