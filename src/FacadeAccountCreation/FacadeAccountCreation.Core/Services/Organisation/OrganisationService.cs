using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace FacadeAccountCreation.Core.Services.Organisation;

public class OrganisationService(
    HttpClient httpClient,
    ILogger<OrganisationService> logger,
    IConfiguration config)
    : IOrganisationService
{
    private const string OrganisationUri = "api/organisations/organisation-by-externalId";
    private const string OrganisationNameUri = "api/organisations/organisation-by-invite-token";
    private const string OrganisationByReferenceNumberUrl = "api/organisations/organisation-by-reference-number";
    private const string OrganisationCreateAddSubsidiaryUri = "api/organisations/create-and-add-subsidiary";
    private const string OrganisationAddSubsidiaryUri = "api/organisations/add-subsidiary";
    private const string OrganisationTerminateSubsidiaryUri = "api/organisations/terminate-subsidiary";
    private const string OrganisationGetSubsidiaryUri = "api/organisations";
    private const string OrganisationNationUrl = "api/organisations/nation-code";
    private const string OrganisationByCompanyHouseNumberUrl = "api/organisations/organisation-by-companies-house-number";
    private const string ReExCreateOrganisationUrl = "/api/v1/reprocessor-exporter-accounts";
    private const string OrganisationTeamMembersUri = "api/organisations/team-members";

    public async Task<HttpResponseMessage> GetOrganisationUserList(Guid userId, Guid organisationId, int serviceRoleId)
    {
        var url = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("GetOrganisationUsers").Value}?userId={userId}&organisationId={organisationId}&serviceRoleId={serviceRoleId}";

        logger.LogInformation("Attempting to fetch the users for organisation id {OrganisationId} from the backend", organisationId);

        return await httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetOrganisationAllUsersList(Guid userId, Guid organisationId, int serviceRoleId)
    {
        var url = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("GetAllOrganisationUsers").Value}?userId={userId}&organisationId={organisationId}&serviceRoleId={serviceRoleId}";

        logger.LogInformation("Attempting to fetch all users for organisation id {OrganisationId} from the backend", organisationId);

        return await httpClient.GetAsync(url);
	}

	public async Task<List<OrganisationTeamMemberModel>> GetOrganisationTeamMembers(Guid userId, Guid organisationId, int serviceRoleId)
	{
		var url = $"{OrganisationTeamMembersUri}?userId={userId}&organisationId={organisationId}&serviceRoleId={serviceRoleId}";

		logger.LogInformation("Attempting to fetch all team members for organisation id {OrganisationId}", organisationId);

		var response = await httpClient.GetAsync(url);

		if (!response.IsSuccessStatusCode)
		{
			var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

			if (problemDetails != null)
			{
				throw new ProblemResponseException(problemDetails, response.StatusCode);
			}
		}

		response.EnsureSuccessStatusCode();
		var teamMembers = await response.Content.ReadFromJsonWithEnumsAsync<List<OrganisationTeamMemberModel>>();

		return teamMembers;
	}

	public async Task<HttpResponseMessage> GetNationIdByOrganisationId(Guid organisationId)
    {
        var url = $"{config.GetSection("RegulatorOrganisationEndpoints").GetSection("GetNationIdFromOrganisationId").Value}?organisationId={organisationId}";

        logger.LogInformation("Attempting to fetch the nationId for organisation id {OrganisationId} from the backend", organisationId);

        return await httpClient.GetAsync(url);
    }

    public async Task<RemovedUserOrganisationModel?> GetOrganisationByExternalId(Guid organisationExternalId)
    {
        var response = await httpClient.GetAsync($"{OrganisationUri}?externalId={organisationExternalId}");
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

        return await response.Content.ReadFromJsonWithEnumsAsync<RemovedUserOrganisationModel>();
    }

    public async Task<OrganisationDto> GetOrganisationByReferenceNumber(string referenceNumber)
    {
        var response = await httpClient.GetAsync($"{OrganisationByReferenceNumberUrl}?referenceNumber={referenceNumber}");
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
        var organisation = await response.Content.
            ReadFromJsonWithEnumsAsync<OrganisationDto>();

        return organisation;
    }

    public async Task<OrganisationDto> GetOrganisationByCompanyHouseNumber(string companyHouseNumber)
    {
        var response = await httpClient.GetAsync($"{OrganisationByCompanyHouseNumberUrl}?companiesHouseNumber={companyHouseNumber}");
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
        var organisation = await response.Content.
            ReadFromJsonWithEnumsAsync<OrganisationDto>();

        return organisation;
    }

    public async Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteToken(string token)
    {
        var response = await httpClient.GetAsync($"{OrganisationNameUri}?token={token}");
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
        var organisationName = response.Content.
            ReadFromJsonWithEnumsAsync<ApprovedPersonOrganisationModel>();
        return organisationName.Result;
    }

    public async Task<CheckRegulatorOrganisationExistResponseModel> GetRegulatorOrganisationByNationId(int nationId)
    {
        var nationLookup = new NationLookup();
        var nationName = nationLookup.GetNationName(nationId);
        var url = $"{config.GetSection("RegulatorOrganisationEndpoints").GetSection("GetOrganisationIdFromNation").Value}{nationName}";
        logger.LogInformation("Attempting to fetch the Regulator Organisation for nation id {NationName} from the backend", nationName);

        var response = await httpClient.GetAsync(url);

        if (string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()))
        {
            throw new InvalidDataException($"Could not retrieve Regulator Data for NationId:{nationId}");
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

        return response.Content.ReadFromJsonAsync<CheckRegulatorOrganisationExistResponseModel>().Result;
    }

    public async Task<string?> CreateAndAddSubsidiaryAsync(LinkOrganisationModel linkOrganisationModel)
    {

        var response = await httpClient.PostAsJsonAsync(OrganisationCreateAddSubsidiaryUri, linkOrganisationModel);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        return result;
    }

    public async Task<string?> AddSubsidiaryAsync(SubsidiaryAddModel subsidiaryAddModel)
    {
        var response = await httpClient.PostAsJsonAsync(OrganisationAddSubsidiaryUri, subsidiaryAddModel);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        return result;
    }

    public async Task TerminateSubsidiaryAsync(SubsidiaryTerminateModel subsidiaryTerminateModel)
    {
        var response = await httpClient.PostAsJsonAsync(OrganisationTerminateSubsidiaryUri, subsidiaryTerminateModel);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }
    }

    public async Task<PagedOrganisationRelationshipsModel> GetPagedOrganisationRelationships(int page, int showPerPage, string search = null)
    {
        var endpoint = $"{OrganisationGetSubsidiaryUri}/organisationRelationships?page={page}&showPerPage={showPerPage}";

        if (search != null)
        {
            endpoint += $"&search={HttpUtility.UrlEncode(search)}";
        }

        try
        {
            logger.LogInformation("Attempting to get the paged Organisation Relationships");

            var response = await httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonWithEnumsAsync<PagedOrganisationRelationshipsModel>();
            result.Items.Clear();
            return result;
            
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get the paged Organisation Relationships");
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }

    public async Task<List<RelationshipResponseModel>> GetUnpagedOrganisationRelationships()
    {
        var endpoint = $"{OrganisationGetSubsidiaryUri}/organisationRelationshipsWithoutPaging";

        try
        {
            logger.LogInformation("Attempting to get unpaged Organisation Relationships");

            var response = await httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonWithEnumsAsync<List<RelationshipResponseModel>>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get the unpaged Organisation Relationships");
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }

    public async Task<OrganisationRelationshipModel> GetOrganisationRelationshipsByOrganisationId(Guid organisationExternalId)
    {
        var endpoint = $"{OrganisationGetSubsidiaryUri}/{organisationExternalId}/organisationRelationships";

        try
        {
            logger.LogInformation("Attempting to get the Organisation Relationships for Organisation Id : '{OrganisationId}'", organisationExternalId);

            var response = await httpClient.GetAsync(endpoint);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            return await response.Content.ReadFromJsonWithEnumsAsync<OrganisationRelationshipModel>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get Organisation Relationships for Organisation Id: '{OrganisationId}'", organisationExternalId);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }

    public async Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportOrganisationSubsidiaries(Guid organisationExternalId)
    {
        var endpoint = $"{OrganisationGetSubsidiaryUri}/{organisationExternalId}/export-subsidiaries";

        try
        {
            logger.LogInformation("Attempting to Export the Organisation Relationships for Organisation Id : '{OrganisationId}'", organisationExternalId);

            var response = await httpClient.GetAsync(endpoint);

            return await response.Content.ReadFromJsonWithEnumsAsync<List<ExportOrganisationSubsidiariesResponseModel>>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Export Organisation Relationships for Organisation Id: '{OrganisationId}'", organisationExternalId);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }
    /// <summary>
    /// Updates the details of an organisation
    /// </summary>
    /// <param name="organisationId">The id of the organisation</param>
    /// <param name="organisationDetails">The new organisation details for the organisation</param>
    /// <returns>Async task indicating success</returns>
    public async Task UpdateOrganisationDetails(
        Guid userId,
        Guid organisationId,
        OrganisationUpdateDto organisationDetails)
    {
        var url = $"{config.GetSection("OrganisationEndpoints").GetSection("UpdateOrganisation").Value}/{organisationId}";

        httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());

        var response = await httpClient.PutAsJsonAsync(
            url,
            organisationDetails);

        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetOrganisationNationCodeByExternalIdAsync(Guid organisationExternalId)
    {
        var url = $"{OrganisationNationUrl}?organisationId={organisationExternalId}";

        try
        {
            logger.LogInformation(message: "Attempting to fetch the nation for an organisation id {OrganisationExternalId} from the backend", organisationExternalId);

            var response = await httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get organisation nation for Organisation Id: '{OrganisationExternalId}'", organisationExternalId);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }

        return null;
    }

    /// <summary>
    /// Create organisation and get related response data from BackEndAccount service 
    /// to be processed further for email notifications
    /// </summary>
    /// <param name="reExOrganisation"></param>
    /// <param name="serviceKey"></param>
    /// <returns>required object and related ids</returns>
    /// <exception cref="ProblemResponseException"></exception>
    public async Task<ReExAddOrganisationResponse?> CreateReExOrganisationAsync(ReprocessorExporterAddOrganisation reExOrganisation, string serviceKey)
    {
        ReExAddOrganisationResponse? result = null;
        var response = await PostAsJsonAsyncWithAuditHeaders($"{ReExCreateOrganisationUrl}?serviceKey={serviceKey}", reExOrganisation, reExOrganisation.User.UserId);

        if (!response.IsSuccessStatusCode)
        {
            ProblemDetails? problemDetails;
            try
            {
                problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            }
            catch (JsonException e)
            {
                problemDetails = new ProblemDetails() { Detail = e.Message };
            }

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }
        
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return result;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReExAddOrganisationResponse>();
    }

    private Task<HttpResponseMessage> PostAsJsonAsyncWithAuditHeaders<TValue>(
        string requestUri,
        TValue value,
        Guid userId)
    {
        const string xEprUserHeader = "X-EPR-User";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value)
        };

        request.Headers.Add(xEprUserHeader, userId.ToString());

        return httpClient.SendAsync(request);
    }
}
