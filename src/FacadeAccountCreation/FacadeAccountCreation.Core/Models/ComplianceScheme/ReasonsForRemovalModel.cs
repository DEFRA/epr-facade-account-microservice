using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;
[ExcludeFromCodeCoverage]
public class ReasonsForRemovalModel
{
    public string Code { get; set; }

    public bool RequiresReason { get; set; }
}