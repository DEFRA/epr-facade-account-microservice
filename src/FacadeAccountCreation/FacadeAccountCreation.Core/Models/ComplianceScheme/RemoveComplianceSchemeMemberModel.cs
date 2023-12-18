using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

public class RemoveComplianceSchemeMemberModel
{
    [Required]
    [MaxLength(100)]
    public string Code { get; set; }

    [MaxLength(2000)]
    public string? TellUsMore { get; set; }
}