namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class SelectSchemeWithUserModel(SelectSchemeModel model, Guid oId)
{
    public Guid ProducerOrganisationId { get; set; } = model.OrganisationId;
    public Guid ComplianceSchemeId { get; set; } = model.ComplianceSchemeId;
    public Guid UserOId { get; set; } = oId;
}