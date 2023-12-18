using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Models.Enrolments;
using Microsoft.Extensions.Options;

namespace FacadeAccountCreation.Core.Services.Enrolments;

public class EnrolmentService : IEnrolmentService
{
    private readonly HttpClient _httpClient;
    private readonly AccountsEndpointsConfig _accountsEndpointsConfig;

    public EnrolmentService(HttpClient httpClient, IOptions<AccountsEndpointsConfig> accountsConfigOptions)
    {
        _httpClient = httpClient;
        _accountsEndpointsConfig = accountsConfigOptions.Value;
    }
    
    public async Task<HttpResponseMessage?> DeleteUser(DeleteUserModel model)
    {
        return await _httpClient.DeleteAsync($"{_accountsEndpointsConfig.DeleteUser}/{model.PersonExternalIdToDelete}?userId={model.LoggedInUserId}&organisationId={model.OrganisationId}&serviceRoleId={model.ServiceRoleId}");
    }
}
