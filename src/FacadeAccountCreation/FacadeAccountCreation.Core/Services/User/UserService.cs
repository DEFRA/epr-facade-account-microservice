using FacadeAccountCreation.Core.Models.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using FacadeAccountCreation.Core.Models.User;

namespace FacadeAccountCreation.Core.Services.User;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _config;
    private const string XEprOrganisationHeader = "X-EPR-Organisation";
    private const string XEprUserHeader = "X-EPR-User";

    public UserService(
        HttpClient httpClient,
        ILogger<UserService> logger,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    public async Task<HttpResponseMessage> GetUserOrganisations(Guid userId)
    {
        var url = $"{_config.GetSection("ComplianceSchemeEndpoints").GetSection("GetUserOrganisations").Value}?userId={userId}";
        
        _logger.LogInformation("Attempting to fetch the organisations for user id '{userId}' from the backend", userId);
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> UpdatePersonalDetailsAsync(
    Guid userId, Guid organisationId, string serviceKey, UserDetailsUpdateModel userDetailsUpdateModelRequest)
    {
        _httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
        _httpClient.DefaultRequestHeaders.Add(XEprOrganisationHeader, organisationId.ToString());

        string requestUri = $"{_config.GetSection("UserDetailsEndpoints").GetSection("UpdateUserDetails").Value}?serviceKey={serviceKey}";

        var requestContent = new StringContent(JsonSerializer.Serialize(userDetailsUpdateModelRequest), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(requestUri, requestContent);

        response.EnsureSuccessStatusCode();

        return response;
    }
}
