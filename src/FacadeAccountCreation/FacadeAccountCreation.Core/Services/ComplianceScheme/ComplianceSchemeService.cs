using EPR.Common.Logging.Constants;
using EPR.Common.Logging.Models;
using EPR.Common.Logging.Services;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Helpers;
using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.Organisations;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FacadeAccountCreation.Core.Services.ComplianceScheme;

public class ComplianceSchemeService : IComplianceSchemeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ComplianceSchemeService> _logger;
    private readonly IConfiguration _config;
    private readonly ILoggingService _loggingService;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private const string XEprUserHeader = "X-EPR-User";

    public ComplianceSchemeService(
        HttpClient httpClient,
        ILogger<ComplianceSchemeService> logger,
        ILoggingService loggingService,
        ICorrelationIdProvider correlationIdProvider,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _loggingService = loggingService;
        _correlationIdProvider = correlationIdProvider;
        _config = config;
    }

    public async Task<HttpResponseMessage> GetAllComplianceSchemesAsync()
    {
        var endpoint = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("Get").Value}";
        
        try
        {
            _logger.LogInformation($"Attempting to call {nameof(GetAllComplianceSchemesAsync)}");
            return await _httpClient.GetAsync(endpoint);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to call {nameof(GetAllComplianceSchemesAsync)}");
            throw;
        }
    }
    
    public async Task<HttpResponseMessage> GetComplianceSchemeForProducerAsync(Guid organisationId, Guid userOid)
    {
        var endpoint = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemeForProducer").Value}";
        
        try
        {
            _logger.LogInformation("Attempting to get the compliance scheme for the organisation id : '{organisationId}'", organisationId);
            
            return await _httpClient.GetAsync( $"{endpoint}?organisationId={organisationId}&userOid={userOid}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get compliance scheme for the organisation id: '{organisationId}'", organisationId);
            throw;
        }
    }

    public async Task<HttpResponseMessage> GetComplianceSchemeMembersAsync(Guid userId, Guid organisationId, Guid selectedSchemeId, string? query, int pageSize, int page)
    {
        HttpResponseMessage result = null;
        var endpointConfigValue =  $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemeMembers").Value}";
        var endpoint = string.Format(endpointConfigValue, organisationId, selectedSchemeId, pageSize, page, query);

        try
        {
            _logger.LogInformation("Attempting to get the compliance schemes members for organisation id : '{organisationId}'", organisationId);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
            result = await _httpClient.GetAsync(endpoint);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get compliance scheme members for organisation id: '{organisationId}'", organisationId);
            throw;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Clear();
        }

        return result;
    }


    public async Task<HttpResponseMessage> GetComplianceSchemesForOperatorAsync(Guid operatorOrganisationId)
    {
        var endpoint =
            $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemesForOperator").Value}";

        try
        {
            _logger.LogInformation(
                "Attempting to get the compliance schemes for operator with the organisation id : '{operatorOrganisationId}'",
                operatorOrganisationId);

            return await _httpClient.GetAsync($"{endpoint}?organisationId={operatorOrganisationId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Failed to get compliance schemes for operator with the organisation id: '{operatorOrganisationId}'",
                operatorOrganisationId);
            throw;
        }
    }

    public async Task<ComplianceSchemeSummary> GetComplianceSchemesSummary(Guid complianceSchemeId, Guid organisationId, Guid userId)
    {
        _httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
        _httpClient.DefaultRequestHeaders.Add("X-EPR-Organisation", organisationId.ToString());

        var endpoint = string.Format(
            _config.GetSection("ComplianceSchemeEndpoints").GetSection("ComplianceSchemeSummaryPath").Value, 
            complianceSchemeId);
            
        var response = await _httpClient.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ComplianceSchemeSummary>();
    }

    public async Task<HttpResponseMessage> RemoveComplianceScheme(RemoveComplianceSchemeModel model)
    {
        var endpoint = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("remove").Value}";

        try
        {
            _logger.LogInformation("Attempting to remove the selected compliance scheme id '{selectedSchemeId}' from the backend", model.SelectedSchemeId);
            return await _httpClient.PostAsJsonAsync(endpoint, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Request failed to remove selected compliance scheme id '{selectedSchemeId}'", model.SelectedSchemeId);
            throw;
        }
    }
    
    public async Task<HttpResponseMessage> SelectComplianceSchemeAsync(SelectSchemeWithUserModel model)
    {
        var endpoint = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("select").Value}";

        try
        {
            return await _httpClient.PostAsJsonAsync(endpoint, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Request failed to add selected compliance scheme id '{ModelComplianceSchemeId}'", model.ComplianceSchemeId);

            throw;
        }
    }
    
    public async Task<HttpResponseMessage> UpdateComplianceSchemeAsync(UpdateSchemeWithUserModel model)
    {
        var endpoint = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("update").Value}";

        try
        {
            return await _httpClient.PostAsJsonAsync(endpoint, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Request failed to update selected compliance scheme id '{ModelComplianceSchemeId}'", model.ComplianceSchemeId);

            throw;
        }
    }

    public async Task<HttpResponseMessage> GetComplianceSchemeMemberDetailsAsync(Guid userId ,Guid organisationId, Guid selectedSchemeId)
    {
        var endpoint = string.Format(_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemeMemberDetails").Value, organisationId, selectedSchemeId);
        
        try
        {
            _logger.LogInformation("Attempting to get the compliance scheme for the organisation id : '{organisationId}' and selected scheme id : '{selectedSchemeId}'", organisationId, selectedSchemeId);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
            return await _httpClient.GetAsync( $"{endpoint}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get compliance scheme for the organisation id: '{organisationId}' and selected scheme id : '{selectedSchemeId}'", organisationId, selectedSchemeId);
            throw;
        }
    }
    
    public async Task<InfoForSelectedSchemeRemovalResponse> GetInfoForSelectedSchemeRemoval(Guid organisationId, Guid selectedSchemeId, Guid userId)
    {
        var endpointConfigValue =  $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetInfoForSelectedSchemeRemoval").Value}";
        var endpoint = string.Format(endpointConfigValue, organisationId, selectedSchemeId);

        try
        {
            _logger.LogInformation("Attempting to retrieve details for selected scheme id: '{selectedSchemeId}'", selectedSchemeId);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
            
            var response  = await _httpClient.GetAsync(endpoint);
            
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<InfoForSelectedSchemeRemovalResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve details for selected scheme id: '{selectedSchemeId}'", selectedSchemeId);
            throw;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Clear();
        }
    }

    public async Task<RemoveComplianceSchemeMemberResponse> RemoveComplianceSchemeMember(Guid organisationId, Guid selectedSchemeId, Guid userId, RemoveComplianceSchemeMemberModel model)
    {
        var endpointConfigValue =  $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("RemoveComplianceSchemeMember").Value}";
        var endpoint = string.Format(endpointConfigValue, organisationId, selectedSchemeId);

        try
        {
            _logger.LogInformation("Attempting to remove selected scheme id : '{selectedSchemeId}'", selectedSchemeId);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
            
            var response  = await _httpClient.PostAsJsonAsync(endpoint, model);

            // Specific try catch here for logging protective monitoring event
            // as we don't want failure if protective monitoring call fails
            try
            {
                await ProtectiveMonitoringLogAsync(organisationId, selectedSchemeId, userId, _correlationIdProvider.GetHttpRequestCorrelationIdOrNew());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred creating the protective monitoring event");
            }

            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RemoveComplianceSchemeMemberResponse>();
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove selected scheme id: '{selectedSchemeId}'", selectedSchemeId);
            throw;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Clear();
        }
    }

    private async Task ProtectiveMonitoringLogAsync(Guid organisationId, Guid selectedSchemeId, Guid userId, Guid correlationId)
    {
        await _loggingService.SendEventAsync(
            userId,
            new ProtectiveMonitoringEvent(
                SessionId: correlationId,
                Component: "facade-account-microservice",
                PmcCode: PmcCodes.Code0706,
                Priority: Priorities.NormalEvent,
                TransactionCode: TransactionCodes.SchemeMemberRemoved,
                Message: $"Scheme membership removed for the organisation id: '{organisationId}' and selected scheme id: {selectedSchemeId}",
                AdditionalInfo: $"OrganisationId: '{organisationId}'"));
    }
       
    public async Task<HttpResponseMessage> GetAllReasonsForRemovalsAsync()
    {
        var endpoint = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetAllReasonsForRemovals").Value}";

        try
        {
            _logger.LogInformation($"Attempting to call {nameof(GetAllReasonsForRemovalsAsync)}");
            return await _httpClient.GetAsync(endpoint);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to call {nameof(GetAllReasonsForRemovalsAsync)}");
            throw;
        }
    }

    public async Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportComplianceSchemeSubsidiaries(Guid userId, Guid organisationId, Guid complianceSchemeId)
    {
            HttpResponseMessage result = null;
            var endpointConfigValue = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("ExportComplianceSchemeSubsidiaries").Value}";
            var endpoint = string.Format(endpointConfigValue, organisationId, complianceSchemeId);
            var response = await _httpClient.GetAsync(endpoint);
            try
                {
                    _logger.LogInformation("Attempting to Export the Compliance Scheme Subsidiaries for Organisation Id : '{organisationId}'", organisationId);
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
                    result = await _httpClient.GetAsync(endpoint);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to Export the Compliance Scheme Subsidiaries for Organisation Id : '{organisationId}'", organisationId);
                    throw;
                }
                finally
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                }

                return await response.Content.ReadFromJsonWithEnumsAsync<List<ExportOrganisationSubsidiariesResponseModel>>();
            }
    }
