using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.ReprocessorExporter;
using System;
using System.Threading.Tasks;

namespace FacadeAccountCreation.Core.Services.ReprocessorExporter;

public class ReprocessorExporterService(HttpClient httpClient) : IReprocessorExporterService
{
	private const string OrganisationUri = "api/organisations/organisation-with-persons/";

	public async Task<OrganisationDetailsResponseDto> GetOrganisationDetailsByOrgId(Guid organisationId)
	{
		var response = await httpClient.GetAsync($"{OrganisationUri}{organisationId}");

		if (response.StatusCode == HttpStatusCode.NoContent)
		{
			return null;
		}

		if (!response.IsSuccessStatusCode)
		{
			var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

			if (problemDetails != null)
			{
				throw new ProblemResponseException(problemDetails, response.StatusCode);
			}
		}

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonWithEnumsAsync<OrganisationDetailsResponseDto>();
	}
}
