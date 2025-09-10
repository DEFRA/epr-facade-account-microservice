using System.Text;
using Microsoft.Extensions.Configuration;

namespace FacadeAccountCreation.Core.Services.User;

public class UserService(
    HttpClient httpClient,
    ILogger<UserService> logger,
    IConfiguration config)
    : IUserService
{
    private const string XEprOrganisationHeader = "X-EPR-Organisation";
    private const string XEprUserHeader = "X-EPR-User";


    public async Task<HttpResponseMessage> GetUserOrganisations(Guid userId)
    {
        var url = $"{config.GetSection("ComplianceSchemeEndpoints").GetSection("GetUserOrganisations").Value}?userId={userId}";

        logger.LogInformation("Attempting to fetch the organisations for user id '{UserId}' from the backend", userId);
        return await httpClient.GetAsync(url);
    }

        
        
    public async Task<HttpResponseMessage> GetUserIdByPersonId(Guid personId)
    {
        var endpoint = config.GetValue<string>("ComplianceSchemeEndpoints:GetUserIdByPersonId");
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("The 'GetUserIdByPersonId' endpoint is not configured.");
        }

        var url = $"{endpoint}?personId={personId}";

        logger.LogInformation("Attempting to fetch user id for personId '{PersonId}' from the backend", personId);

        return await httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> UpdatePersonalDetailsAsync(
    Guid userId, Guid organisationId, string serviceKey, UpdateUserDetailsRequest userDetailsUpdateModelRequest)
    {
        httpClient.DefaultRequestHeaders.Add(XEprUserHeader, userId.ToString());
        httpClient.DefaultRequestHeaders.Add(XEprOrganisationHeader, organisationId.ToString());

        var requestUri = $"{config.GetSection("UserDetailsEndpoints").GetSection("UpdateUserDetails").Value}?serviceKey={serviceKey}";

        var requestContent = new StringContent(JsonSerializer.Serialize(userDetailsUpdateModelRequest), Encoding.UTF8, "application/json");

        var response = await httpClient.PutAsync(requestUri, requestContent);

        response.EnsureSuccessStatusCode();

        return response;
    }
}
