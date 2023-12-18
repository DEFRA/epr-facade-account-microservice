using System.Net;
using System.Net.Http.Json;
using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.Organisations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FacadeAccountCreation.Core.Services.Organisation;

public class OrganisationService : IOrganisationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrganisationService> _logger;
    private readonly IConfiguration _config;
    private const string OrganisationUri = "api/organisations/organisation-by-externalId";
    
    public OrganisationService(
        HttpClient httpClient,
        ILogger<OrganisationService> logger,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }
    
    public async Task<HttpResponseMessage> GetOrganisationUserList(Guid userId, Guid organisationId, int serviceRoleId)
    {
        var url = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetOrganisationUsers").Value}?userId={userId}&organisationId={organisationId}&serviceRoleId={serviceRoleId}";
        
        _logger.LogInformation("Attempting to fetch the users for organisation id {OrganisationId} from the backend", organisationId);
        
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetNationIdByOrganisationId(Guid organisationId)
    {
        var url = $"{_config.GetSection("RegulatorOrganisationEndpoints").GetSection("GetNationIdFromOrganisationId").Value}?organisationId={organisationId}";
        
        _logger.LogInformation("Attempting to fetch the nationId for organisation id {OrganisationId} from the backend", organisationId);
        
        return await _httpClient.GetAsync(url);
    }
    
    public async Task<RemovedUserOrganisationModel?> GetOrganisationByExternalId(Guid organisationExternalId)
    {
        var response = await _httpClient.GetAsync($"{OrganisationUri}?externalId={organisationExternalId}");
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
}
