using FacadeAccountCreation.Core.Attributes;

namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class InvitingUserModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [NotDefault]
    public Guid? UserId { get; set; }
}