using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.User;
[ExcludeFromCodeCoverage]
public class OrganisationDetailModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string OrganisationRole { get; set; }

    public string OrganisationType { get; set; }

    public string OrganisationNumber { get; set; }

    public int? NationId { get; set; }

    public string? BuildingName { get; set; }

    public string? BuildingNumber { get; set; }

    public string? Street { get; set; }

    public string? Locality { get; set; }

    public string? DependentLocality { get; set; }

    public string? Town { get; set; }

    public string? County { get; set; }

    public string? Country { get; set; }

    public string? Postcode { get; set; }

    public string OrgAddress { get; set; }

    public string? JobTitle { get; set; }
}