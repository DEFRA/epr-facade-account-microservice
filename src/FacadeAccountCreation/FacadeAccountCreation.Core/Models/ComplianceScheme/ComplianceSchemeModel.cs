namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

public class ComplianceSchemeModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public DateTimeOffset CreatedOn { get; set; }

    public int NationId { get; set; }
}