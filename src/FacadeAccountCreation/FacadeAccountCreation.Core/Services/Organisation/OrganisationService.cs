﻿using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.Organisations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace FacadeAccountCreation.Core.Services.Organisation;

public class OrganisationService : IOrganisationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrganisationService> _logger;
    private readonly IConfiguration _config;
    private const string OrganisationUri = "api/organisations/organisation-by-externalId";
    private const string OrganisationNameUri = "api/organisations/organisation-by-invite-token";
    private const string OrganisationCreateAddSubsidiaryUri = "api/organisations/create-and-add-subsidiary";
    private const string OrganisationAddSubsidiaryUri = "api/organisations/add-subsidiary";

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
    
    public async Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteToken(string token)
    {
        var response = await _httpClient.GetAsync($"{OrganisationNameUri}?token={token}");
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
        var url = $"{_config.GetSection("RegulatorOrganisationEndpoints").GetSection("GetOrganisationIdFromNation").Value}{nationName}";
        _logger.LogInformation("Attempting to fetch the Regulator Organisation for nation id {nationName} from the backend", nationName);

        var response = await _httpClient.GetAsync(url);

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

        var response = await _httpClient.PostAsJsonAsync(OrganisationCreateAddSubsidiaryUri, linkOrganisationModel);

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
        var response = await _httpClient.PostAsJsonAsync(OrganisationAddSubsidiaryUri, subsidiaryAddModel);

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
}
