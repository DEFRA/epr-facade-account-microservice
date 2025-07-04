using FacadeAccountCreation.Core.Models.B2c;
using FacadeAccountCreation.Core.Services.B2c;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace FacadeAccountCreation.Core.Services.B2C;

public class B2CService(
    HttpClient httpClient,
    ILogger<B2CService> logger,
    IConfiguration config)
    : IB2CService
{
    public async Task<HttpResponseMessage> GetUserOrganisationIds(UserOrganisationIdentifiersRequest request)
    {
        var endpoint = $"{config.GetSection("B2cEndpoints").GetSection("GetUserOrganisationIds").Value}";
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("The 'GetUserOrganisationIds' endpoint is not configured.");
        }

        logger.LogInformation("Attempting to fetch the organisation ids for user id '{UserId}' from the backend", request.ObjectId);

        var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(endpoint, requestContent);

        response.EnsureSuccessStatusCode();

        return response;
    }
}
