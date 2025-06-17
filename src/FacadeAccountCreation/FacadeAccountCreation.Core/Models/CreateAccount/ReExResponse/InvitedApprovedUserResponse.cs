namespace FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;

[ExcludeFromCodeCoverage]
public record InvitedApprovedUserResponse
{
    public required string Email { get; set; }
    public required string InviteToken { get; set; }
}
