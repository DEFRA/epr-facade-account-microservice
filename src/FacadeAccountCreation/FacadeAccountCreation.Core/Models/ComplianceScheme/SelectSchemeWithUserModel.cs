using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;
[ExcludeFromCodeCoverage]
public class SelectSchemeWithUserModel
{
    public SelectSchemeWithUserModel(SelectSchemeModel model, Guid oId)
    {
        ProducerOrganisationId = model.OrganisationId;
        ComplianceSchemeId = model.ComplianceSchemeId;
        UserOId = oId;
    }
    
    public Guid ProducerOrganisationId { get; set; }
    public Guid ComplianceSchemeId { get; set; }
    public Guid UserOId { get; set; }
}