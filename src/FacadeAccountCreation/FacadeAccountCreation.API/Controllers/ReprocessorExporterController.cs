using FacadeAccountCreation.Core.Services.ReprocessorExporter;
using Microsoft.AspNetCore.Mvc;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api")]
public class ReprocessorExporterController(IReprocessorExporterService reprocessorExporterService) : ControllerBase
{
	[HttpGet]
	[Route("organisations/organisation-with-persons/{organisationId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetOrganisationDetailsByOrgId(Guid organisationId)
	{
		var organisationDetails = await reprocessorExporterService.GetOrganisationDetailsByOrgId(organisationId);
		return Ok(organisationDetails);
	}
}
