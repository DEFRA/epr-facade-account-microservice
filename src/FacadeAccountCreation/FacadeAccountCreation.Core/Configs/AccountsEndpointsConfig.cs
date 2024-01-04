namespace FacadeAccountCreation.Core.Configs;

public class AccountsEndpointsConfig
{
    public const string SectionName = "AccountsEndpoints";
    
    public string Accounts { get; set; }
    public string Organisations { get; set; }
    public string InviteUser { get; set; }
    public string EnrolInvitedUser { get; set; }
    public string DeleteUser { get; set; }
    public string ApprovedUserAccounts { get; set; }
}