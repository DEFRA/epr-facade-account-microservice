namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

public class UpdateSchemeModel
{
    public Guid SelectedSchemeId { get; set; }
    public Guid OrganisationId { get; set; }
    public Guid ComplianceSchemeId { get; set; }
}