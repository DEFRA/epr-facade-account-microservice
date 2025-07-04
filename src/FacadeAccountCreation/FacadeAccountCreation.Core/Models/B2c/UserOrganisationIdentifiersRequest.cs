using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.B2c;

[ExcludeFromCodeCoverage]
public class UserOrganisationIdentifiersRequest
{
    [Required]
    public Guid ObjectId { get; set; }
}