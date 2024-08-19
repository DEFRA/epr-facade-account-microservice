﻿using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Organisations;

[ExcludeFromCodeCoverage]
public class RelationshipResponseModel
{
    public string OrganisationNumber { get; set; }

    public string OrganisationName { get; set; }

    public string RelationshipType { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public string OldSubsidiaryId { get; set; }
}