namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class EnrolInvitedUserModel
{
    [Required]
    public string InviteToken { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    public string? Email  { get; set; }
    
    public Guid? UserId { get; set; }
}