using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.CreateAccount;
[ExcludeFromCodeCoverage]
public class ConnectionModel
{
    public string? JobTitle { get; set; }

    [Required]
    public string ServiceRole { get; set; } = null!;
}
