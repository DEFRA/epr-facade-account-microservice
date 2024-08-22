using FacadeAccountCreation;
using FacadeAccountCreation.Core;
using FacadeAccountCreation.Core.Models;
using FacadeAccountCreation.Core.Models.Subsidiary;

namespace FacadeAccountCreation.Core.Models.Subsidiary;
public class ExportOrganisationSubsidiariesResponseModel
{
    public int OrganisationId { get; set; }

    public int? SubsidiaryId { get; set; }

    public string OrganisationName { get; set; }

    public string CompaniesHouseNumber { get; set; }
}