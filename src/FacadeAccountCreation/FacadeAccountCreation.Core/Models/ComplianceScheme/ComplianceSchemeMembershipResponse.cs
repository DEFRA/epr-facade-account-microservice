using FacadeAccountCreation.Core.Services;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

[ExcludeFromCodeCoverage]
public class ComplianceSchemeMembershipResponse
{
    public PaginatedResponse<ComplianceSchemeMemberDto> PagedResult { get; set; }
    public string SchemeName { get; set; }
    public DateTimeOffset? LastUpdated { get; set; }
    public int LinkedOrganisationCount { get; set; }
}