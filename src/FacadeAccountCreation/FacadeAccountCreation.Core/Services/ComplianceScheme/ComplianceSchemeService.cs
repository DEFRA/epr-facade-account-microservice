using EPR.Common.Logging.Constants;
using EPR.Common.Logging.Models;
using EPR.Common.Logging.Services;
using FacadeAccountCreation.Core.Helpers;
using Microsoft.Extensions.Configuration;

namespace FacadeAccountCreation.Core.Services.ComplianceScheme;

public class ComplianceSchemeService(
    HttpClient httpClient,
    ILogger<ComplianceSchemeService> logger,
    ILoggingService loggingService,
    ICorrelationIdProvider correlationIdProvider,
    IConfiguration config)
    : IComplianceSchemeService
{
    private const string XEprUserHeader = "X-EPR-User";

    public async Task<HttpResponseMessage> GetAllComplianceSchemesAsync()
    {
        var endpoint = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("Get").Value}";
        
        try
        {
            logger.LogInformation($"Attempting to call {nameof(GetAllComplianceSchemesAsync)}");
            return await httpClient.GetAsync(endpoint);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to call {nameof(GetAllComplianceSchemesAsync)}");
            throw;
        }
    }
    
    public async Task<HttpResponseMessage> GetComplianceSchemeForProducerAsync(Guid organisationId, Guid userOid)
    {
        var endpoint = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemeForProducer").Value}";
        
        try
        {
            logger.LogInformation("Attempting to get the compliance scheme for the organisation id : '{OrganisationId}'", organisationId);
            
            return await httpClient.GetAsync( $"{endpoint}?organisationId={organisationId}&userOid={userOid}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get compliance scheme for the organisation id: '{OrganisationId}'", organisationId);
            throw;
        }
    }

    public async Task<HttpResponseMessage> GetComplianceSchemeMembersAsync(Guid userId, Guid organisationId, Guid selectedSchemeId, string? query, int pageSize, int page, bool hideNoSubsidiaries)
    {
        HttpResponseMessage result;

        var endpointConfigValue = config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemeMembers").Value;
        var uriBuilder = new UriBuilder(string.Format(endpointConfigValue, organisationId, selectedSchemeId, pageSize, page, query, hideNoSubsidiaries));

        var endpoint = uriBuilder.Host + uriBuilder.Path + uriBuilder.Query;

        try
        {
            logger.LogInformation("Attempting to get the compliance schemes members for organisation id : '{OrganisationId}'", organisationId);
            // this doesn't look thread safe to me
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
            result = await httpClient.GetAsync(endpoint);

        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get compliance scheme members for organisation id: '{OrganisationId}'", organisationId);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }

        return result;
    }


    public async Task<HttpResponseMessage> GetComplianceSchemesForOperatorAsync(Guid operatorOrganisationId)
    {
        var endpoint = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemesForOperator").Value}";

        try
        {
            logger.LogInformation(
                "Attempting to get the compliance schemes for operator with the organisation id : '{OperatorOrganisationId}'",
                operatorOrganisationId);

            return await httpClient.GetAsync($"{endpoint}?organisationId={operatorOrganisationId}");
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Failed to get compliance schemes for operator with the organisation id: '{OperatorOrganisationId}'",
                operatorOrganisationId);
            throw;
        }
    }

    public async Task<ComplianceSchemeSummary> GetComplianceSchemesSummary(Guid complianceSchemeId, Guid organisationId, Guid userId)
    {
        httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
        httpClient.DefaultRequestHeaders.Add("X-EPR-Organisation", organisationId.ToString());

        var endpoint = string.Format(
            config.GetSection("ComplianceSchemeEndpoints").GetSection("ComplianceSchemeSummaryPath").Value, 
            complianceSchemeId);
            
        var response = await httpClient.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ComplianceSchemeSummary>();
    }

    public async Task<HttpResponseMessage> RemoveComplianceScheme(RemoveComplianceSchemeModel model)
    {
        var endpoint = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("remove").Value}";

        try
        {
            logger.LogInformation("Attempting to remove the selected compliance scheme id '{SelectedSchemeId}' from the backend", model.SelectedSchemeId);
            return await httpClient.PostAsJsonAsync(endpoint, model);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Request failed to remove selected compliance scheme id '{SelectedSchemeId}'", model.SelectedSchemeId);
            throw;
        }
    }
    
    public async Task<HttpResponseMessage> SelectComplianceSchemeAsync(SelectSchemeWithUserModel model)
    {
        var endpoint = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("select").Value}";

        try
        {
            return await httpClient.PostAsJsonAsync(endpoint, model);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Request failed to add selected compliance scheme id '{ModelComplianceSchemeId}'", model.ComplianceSchemeId);

            throw;
        }
    }
    
    public async Task<HttpResponseMessage> UpdateComplianceSchemeAsync(UpdateSchemeWithUserModel model)
    {
        var endpoint = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("update").Value}";

        try
        {
            return await httpClient.PostAsJsonAsync(endpoint, model);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Request failed to update selected compliance scheme id '{ModelComplianceSchemeId}'", model.ComplianceSchemeId);

            throw;
        }
    }

    public async Task<HttpResponseMessage> GetComplianceSchemeMemberDetailsAsync(Guid userId ,Guid organisationId, Guid selectedSchemeId)
    {
        var endpoint = string.Format(config.GetSection("ComplianceSchemeEndpoints").GetSection("GetComplianceSchemeMemberDetails").Value, organisationId, selectedSchemeId);
        
        try
        {
            logger.LogInformation("Attempting to get the compliance scheme for the organisation id : '{OrganisationId}' and selected scheme id : '{SelectedSchemeId}'", organisationId, selectedSchemeId);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
            return await httpClient.GetAsync( $"{endpoint}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get compliance scheme for the organisation id: '{OrganisationId}' and selected scheme id : '{SelectedSchemeId}'", organisationId, selectedSchemeId);
            throw;
        }
    }
    
    public async Task<InfoForSelectedSchemeRemovalResponse> GetInfoForSelectedSchemeRemoval(Guid organisationId, Guid selectedSchemeId, Guid userId)
    {
        var endpointConfigValue =  $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("GetInfoForSelectedSchemeRemoval").Value}";
        var endpoint = string.Format(endpointConfigValue, organisationId, selectedSchemeId);

        try
        {
            logger.LogInformation("Attempting to retrieve details for selected scheme id: '{SelectedSchemeId}'", selectedSchemeId);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
            
            var response  = await httpClient.GetAsync(endpoint);
            
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<InfoForSelectedSchemeRemovalResponse>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to retrieve details for selected scheme id: '{SelectedSchemeId}'", selectedSchemeId);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }

    public async Task<RemoveComplianceSchemeMemberResponse> RemoveComplianceSchemeMember(Guid organisationId, Guid selectedSchemeId, Guid userId, RemoveComplianceSchemeMemberModel model)
    {
        var endpointConfigValue =  $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("RemoveComplianceSchemeMember").Value}";
        var endpoint = string.Format(endpointConfigValue, organisationId, selectedSchemeId);

        try
        {
            logger.LogInformation("Attempting to remove selected scheme id : '{SelectedSchemeId}'", selectedSchemeId);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
            
            var response  = await httpClient.PostAsJsonAsync(endpoint, model);

            // Specific try catch here for logging protective monitoring event
            // as we don't want failure if protective monitoring call fails
            try
            {
                await ProtectiveMonitoringLogAsync(organisationId, selectedSchemeId, userId, correlationIdProvider.GetHttpRequestCorrelationIdOrNew());
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred creating the protective monitoring event");
            }

            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RemoveComplianceSchemeMemberResponse>();
            
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove selected scheme id: '{SelectedSchemeId}'", selectedSchemeId);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }

    private async Task ProtectiveMonitoringLogAsync(Guid organisationId, Guid selectedSchemeId, Guid userId, Guid correlationId)
    {
        await loggingService.SendEventAsync(
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
        var endpoint = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("GetAllReasonsForRemovals").Value}";

        try
        {
            logger.LogInformation($"Attempting to call {nameof(GetAllReasonsForRemovalsAsync)}");
            return await httpClient.GetAsync(endpoint);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to call {nameof(GetAllReasonsForRemovalsAsync)}");
            throw;
        }
    }

    public async Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportComplianceSchemeSubsidiaries(Guid userId, Guid organisationId, Guid complianceSchemeId)
    {
        var endpointConfigValue = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("ExportComplianceSchemeSubsidiaries").Value}";
        var endpoint = string.Format(endpointConfigValue, organisationId, complianceSchemeId);

        try
        {
            logger.LogInformation("Attempting to Export the Compliance Scheme Subsidiaries for Organisation Id : '{OrganisationId}'", organisationId);
            httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());

            var response = await httpClient.GetAsync(endpoint); 

            return await response.Content.ReadFromJsonWithEnumsAsync<List<ExportOrganisationSubsidiariesResponseModel>>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Export the Compliance Scheme Subsidiaries for Organisation Id : '{OrganisationId}'", organisationId);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }
}