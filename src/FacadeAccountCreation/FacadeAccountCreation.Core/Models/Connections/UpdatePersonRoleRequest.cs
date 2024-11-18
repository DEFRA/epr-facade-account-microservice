namespace FacadeAccountCreation.Core.Models.Connections;

[ExcludeFromCodeCoverage]
public class UpdatePersonRoleRequest
{
    [Required]
    public PersonRole PersonRole { get; set; }
}