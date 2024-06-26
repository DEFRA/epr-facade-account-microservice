﻿using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme
{
    [ExcludeFromCodeCoverage]
    public class ComplianceSchemeMemberDto
    {
        public Guid SelectedSchemeId { get; set; }
        public string OrganisationNumber { get; set; }
        public string OrganisationName { get; set; }
    }
}
