using FacadeAccountCreation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.CreateAccount;
[ExcludeFromCodeCoverage]
public class InvitedUserModel
{
    [EmailAddress]
    public string Email { get; set; }
    
    [NotDefault]
    public int? PersonRoleId { get; set; }
    
    [NotDefault]
    public int? ServiceRoleId { get; set; }
    
    public string Rolekey { get; set; }
    
    [Required]
    [NotDefault]
    public Guid OrganisationId { get; set; }
    
    [Required]
    public string OrganisationName { get; set; }
}