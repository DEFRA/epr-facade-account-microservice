namespace FacadeAccountCreation.Core.Models.Organisations;

[ExcludeFromCodeCoverage]
public class RelationshipResponseModel
{
    public Guid? ParentOrganisationExternalId { get; set; }

    public string OrganisationNumber { get; set; }

    public string OrganisationName { get; set; }

    public string RelationshipType { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public DateTime? JoinerDate { get; set; }

    public string? ReportingType { get; set; }
}
