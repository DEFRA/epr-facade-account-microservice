namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeMemberDto
{
    public Guid SelectedSchemeOrganisationExternalId { get; set; }
    public Guid SelectedSchemeId { get; set; }
    public string OrganisationNumber { get; set; }
    public string OrganisationName { get; set; }
    public string CompaniesHouseNumber { get; set; }
    public List<RelationshipResponseModel> Relationships { get; set; }
}