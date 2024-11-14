using FacadeAccountCreation.Core.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme
{
    [ExcludeFromCodeCoverage]
    public class ComplianceSchemeMembershipResponse
    {
        public PaginatedResponse<ComplianceSchemeMemberDto> PagedResult { get; set; }
        public string SchemeName { get; set; }
        public DateTimeOffset? LastUpdated { get; set; }
        public int LinkedOrganisationCount { get; set; }
        public int SubsidiariesCount { get; set; }
    }
}
