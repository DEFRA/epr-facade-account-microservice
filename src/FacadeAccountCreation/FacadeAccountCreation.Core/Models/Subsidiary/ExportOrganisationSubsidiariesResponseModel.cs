﻿namespace FacadeAccountCreation.Core.Models.Subsidiary;

[ExcludeFromCodeCoverage]
public class ExportOrganisationSubsidiariesResponseModel
{
    public string OrganisationId { get; set; }

    public string SubsidiaryId { get; set; }

    public string OrganisationName { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public DateTime? JoinerDate { get; set; }

    public string ReportingType { get; set; }
}