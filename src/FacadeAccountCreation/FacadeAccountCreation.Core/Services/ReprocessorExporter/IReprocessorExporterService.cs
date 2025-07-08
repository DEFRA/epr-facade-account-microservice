using FacadeAccountCreation.Core.Models.ReprocessorExporter;
using System;
using System.Threading.Tasks;

namespace FacadeAccountCreation.Core.Services.ReprocessorExporter;
public interface IReprocessorExporterService
{
	Task<OrganisationDetailsResponseDto> GetOrganisationDetailsByOrgId(Guid organisationId);
}
