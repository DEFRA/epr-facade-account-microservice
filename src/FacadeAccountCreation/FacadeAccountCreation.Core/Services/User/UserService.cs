using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FacadeAccountCreation.Core.Services.User;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _config;

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
}
