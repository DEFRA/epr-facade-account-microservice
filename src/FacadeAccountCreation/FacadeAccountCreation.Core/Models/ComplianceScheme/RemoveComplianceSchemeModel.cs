namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

public class RemoveComplianceSchemeModel
{
    public Guid SelectedSchemeId { get; set; }
    
    public Guid UserOId { get; set; }
    
    public Guid OrganisationId { get; set; }
}