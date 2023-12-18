using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.Core.Models.Connections;

public class UpdatePersonRoleRequest
{
    [Required]
    public PersonRole PersonRole { get; set; }
}