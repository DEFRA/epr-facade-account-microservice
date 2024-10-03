using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Models.DelegatedPerson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacadeAccountCreation.Core.Services.Connection;

public class RoleManagementService : IRoleManagementService
{
    private readonly HttpClient _httpClient;
    private readonly ConnectionsEndpointsConfig _connectionsEndpoints;
    private readonly ILogger<RoleManagementService> _logger;
    private const string XEprOrganisationHeader = "X-EPR-Organisation";
    private const string XEprUserHeader = "X-EPR-User";
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public RoleManagementService(HttpClient httpClient, ILogger<RoleManagementService> logger, IOptions<ConnectionsEndpointsConfig> connectionsEndpointsConfig)
    {
        _httpClient = httpClient;
        _logger = logger;
        _connectionsEndpoints= connectionsEndpointsConfig.Value;
        _jsonSerializerOptions = new JsonSerializerOptions();
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<ConnectionPersonModel> GetPerson(Guid connectionId, string serviceKey, Guid userId, Guid organisationId)
    {
        _logger.LogInformation("Attempting to get the connection person for the connection id : '{connectionId}'", connectionId);

        var uriBuilder = new UriBuilder(string.Format(_connectionsEndpoints.Person, connectionId, serviceKey));

        string endpoint = uriBuilder.Host + uriBuilder.Path + uriBuilder.Query;

        return await GetFromEndpoint<ConnectionPersonModel>(endpoint, userId, organisationId);
    }

    public async Task<ConnectionWithEnrolmentsModel> GetEnrolments(Guid connectionId, string serviceKey, Guid userId, Guid organisationId)
    {
        _logger.LogInformation("Attempting to get the connection enrolments for the connection id : '{connectionId}'", connectionId);

        var uriBuilder = new UriBuilder(string.Format(_connectionsEndpoints.Enrolments, connectionId, serviceKey));

        string endpoint = uriBuilder.Host + uriBuilder.Path + uriBuilder.Query;

       return await GetFromEndpoint<ConnectionWithEnrolmentsModel>(endpoint, userId, organisationId);
    }

    public async Task<UpdatePersonRoleResponse> UpdatePersonRole(Guid connectionId, Guid userId, Guid organisationId, string serviceKey, UpdatePersonRoleRequest updateRequest)
    {
        _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
        _httpClient.DefaultRequestHeaders.Add(XEprOrganisationHeader, organisationId.ToString());

        string requestUri = $"api/connections/{connectionId}/roles?serviceKey={serviceKey}";

        var response = await PutAsJsonAsync(userId, organisationId, requestUri, updateRequest);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonWithEnumsAsync<UpdatePersonRoleResponse>();
    }

    public async Task<HttpResponseMessage> NominateToDelegatedPerson(Guid connectionId, Guid userId, Guid organisationId, string serviceKey, DelegatedPersonNominationRequest nominationRequest)
    {
        _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
        _httpClient.DefaultRequestHeaders.Add(XEprOrganisationHeader, organisationId.ToString());

        string requestUri = $"api/connections/{connectionId}/delegated-person-nomination?serviceKey={serviceKey}";

        var requestContent = new StringContent(JsonSerializer.Serialize(nominationRequest, _jsonSerializerOptions), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(requestUri, requestContent);

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> AcceptNominationToDelegatedPerson(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey, AcceptNominationRequest acceptNominationRequest)
    {
        _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
        _httpClient.DefaultRequestHeaders.Add(XEprOrganisationHeader, organisationId.ToString());

        string requestUri = $"api/enrolments/{enrolmentId}/delegated-person-acceptance?serviceKey={serviceKey}";

        var requestContent = new StringContent(JsonSerializer.Serialize(acceptNominationRequest, _jsonSerializerOptions), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(requestUri, requestContent);

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> AcceptNominationForApprovedPerson(Guid enrolmentId, 
        Guid userId, Guid organisationId, string serviceKey, AcceptNominationApprovedPersonRequest acceptNominationRequest)
    {
        _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
        _httpClient.DefaultRequestHeaders.Add(XEprOrganisationHeader, organisationId.ToString());

        string requestUri = $"api/enrolments/{enrolmentId}/approved-person-acceptance?serviceKey={serviceKey}";

        var requestContent = new StringContent(JsonSerializer.Serialize(acceptNominationRequest, _jsonSerializerOptions), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(requestUri, requestContent);

        response.EnsureSuccessStatusCode();

        return response;
    }


    public async Task<DelegatedPersonNominatorModel> GetDelegatedPersonNominator(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey)
    {
        _httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
        _httpClient.DefaultRequestHeaders.Add("X-EPR-Organisation", organisationId.ToString());
        
        var response = await _httpClient.GetAsync($"api/enrolments/{enrolmentId}/delegated-person-nominator?serviceKey={serviceKey}");
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonWithEnumsAsync<DelegatedPersonNominatorModel>();
    }

    private async Task<T> GetFromEndpoint<T>(string endPointUrl, Guid userId, Guid organisationId) where T : class
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
        _httpClient.DefaultRequestHeaders.Add(XEprOrganisationHeader, organisationId.ToString());
        var response = await _httpClient.GetAsync(endPointUrl);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonWithEnumsAsync<T>();
    }

    private async Task<HttpResponseMessage> PutAsJsonAsync<T>(Guid userId, Guid organisationId, string endpoint, T body)
    {
        HttpRequestMessage request = new(HttpMethod.Put, new Uri(endpoint, UriKind.RelativeOrAbsolute))
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.Add(XEprOrganisationHeader, organisationId.ToString());
        request.Headers.Add(XEprUserHeader, userId.ToString());

        _httpClient.DefaultRequestHeaders.Clear();
        
        var response = await _httpClient.SendAsync(request, CancellationToken.None);

        return response;
    }
}
