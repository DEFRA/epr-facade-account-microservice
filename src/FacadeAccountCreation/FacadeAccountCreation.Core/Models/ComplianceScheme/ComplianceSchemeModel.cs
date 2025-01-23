namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeModel
{
    public int RowNumber { get; set; }

    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public DateTimeOffset CreatedOn { get; set; }

    public int NationId { get; set; }
}