namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class UpdateSchemeWithUserModel(UpdateSchemeModel model, Guid oId)
{
    public Guid SelectedSchemeId { get; set; } = model.SelectedSchemeId;
    public Guid ProducerOrganisationId { get; set; } = model.OrganisationId;
    public Guid ComplianceSchemeId { get; set; } = model.ComplianceSchemeId;
    public Guid UserOId { get; set; } = oId;
}