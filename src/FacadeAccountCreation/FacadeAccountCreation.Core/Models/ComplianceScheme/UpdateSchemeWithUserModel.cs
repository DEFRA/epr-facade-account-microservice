namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

public class UpdateSchemeWithUserModel
{
    public UpdateSchemeWithUserModel(UpdateSchemeModel model, Guid oId)
    {
        SelectedSchemeId = model.SelectedSchemeId;
        ProducerOrganisationId = model.OrganisationId;
        ComplianceSchemeId = model.ComplianceSchemeId;
        UserOId = oId;
    }
    
    public Guid SelectedSchemeId { get; set; }
    public Guid ProducerOrganisationId { get; set; }
    public Guid ComplianceSchemeId { get; set; }
    public Guid UserOId { get; set; }
}