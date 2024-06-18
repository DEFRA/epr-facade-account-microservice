using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class ProducerComplianceSchemeModel
{
    public Guid SelectedSchemeId { get; set; }
    
    public string ComplianceSchemeName { get; set; }
    
    public Guid ComplianceSchemeId { get; set; }
    
    public string ComplianceSchemeOperatorName { get; set; }
    
    public Guid ComplianceSchemeOperatorId { get; set; }
}