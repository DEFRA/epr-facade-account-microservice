using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.CreateAccount;
[ExcludeFromCodeCoverage]
public class AccountInvitationModel
{
    public InvitedUserModel InvitedUser { get; set; }
    public InvitingUserModel InvitingUser { get; set; }
}