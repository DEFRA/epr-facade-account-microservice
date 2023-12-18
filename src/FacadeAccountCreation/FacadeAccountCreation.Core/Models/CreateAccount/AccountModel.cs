using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class AccountModel
{
    [Required]
    public PersonModel Person { get; set; } = null!;

    [Required]
    public OrganisationModel Organisation { get; set; } = null!;

    [Required]
    public ConnectionModel Connection { get; set; } = null!;
}
