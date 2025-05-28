namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class ReExOrganisationModel
{
    public ReExUserModel ReExUser { get; set; } = new();

    /// <summary>
    /// Role/Job title can be Director, CompanySecretary, Partner or Member
    /// </summary>
    public string? UserRoleInOrganisation { get; set; }

    public bool IsApprovedUser { get; set; }

    public ReExCompanyModel Company { get; set; }

    /// <summary>
    /// ReEx Approved persons information related to names/emails
    /// </summary>
    public List<ReExInvitedApprovedPerson> InvitedApprovedPersons { get; set; } = [];
}