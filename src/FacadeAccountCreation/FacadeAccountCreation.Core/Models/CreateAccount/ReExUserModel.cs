namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class ReExUserModel
{
    public Guid? UserId { get; set; }

    public string? UserFirstName { get; set; }

    public string? UserLastName { get; set; }

    public string? UserEmail { get; set; }

    public bool IsApprovedUser { get; set; }
}
