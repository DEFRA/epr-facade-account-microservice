using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class ConnectionModel
{
    public string? JobTitle { get; set; }

    [Required]
    public string ServiceRole { get; set; } = null!;
}
