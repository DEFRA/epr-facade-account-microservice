namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReExOrganisationModel
{
    public ReExUserModel ReExUser { get; set; } = new();

    /// <summary>
    /// Role/Job title can be Director, CompanySecretary, Partner or Member
    /// </summary>
    public string? UserRoleInOrganisation { get; set; }

    public bool IsApprovedUser { get; set; }

    public ReExCompanyModel? Company { get; set; }

    /// <summary>
    /// Used for non-company journey i.e. SoleTrader, partnership
    /// </summary>
    public ReExManualInputModel? ManualInput { get; set; }

    /// <summary>
    /// ReEx Approved persons information related to names/emails
    /// </summary>
    public List<ReExInvitedApprovedPerson> InvitedApprovedPersons { get; set; } = [];
}