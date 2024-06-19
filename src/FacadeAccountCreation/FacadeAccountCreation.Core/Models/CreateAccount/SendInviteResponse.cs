using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.CreateAccount;
[ExcludeFromCodeCoverage]
public class SendInviteResponse
{
    public string? InviteToken { get; set; }
}