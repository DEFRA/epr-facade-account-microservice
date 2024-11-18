namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class AccountModel
{
    [Required]
    public PersonModel Person { get; set; } = null!;

    [Required]
    public OrganisationModel Organisation { get; set; } = null!;

    [Required]
    public ConnectionModel Connection { get; set; } = null!;
    
    public Guid? UserId { get; set; }

    public string? DeclarationFullName { get; set; }

    public DateTime? DeclarationTimeStamp { get; set; }
}
