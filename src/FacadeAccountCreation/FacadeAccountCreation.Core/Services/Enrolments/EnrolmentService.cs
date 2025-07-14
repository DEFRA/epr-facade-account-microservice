using FacadeAccountCreation.Core.Configs;

namespace FacadeAccountCreation.Core.Services.Enrolments;

public class EnrolmentService(HttpClient httpClient, IOptions<AccountsEndpointsConfig> accountsConfigOptions)
    : IEnrolmentService
{
    private readonly AccountsEndpointsConfig _accountsEndpointsConfig = accountsConfigOptions.Value;

    public async Task<HttpResponseMessage?> DeleteUser(DeleteUserModel model)
    {
        return await httpClient.DeleteAsync($"{_accountsEndpointsConfig.DeleteUser}/{model.PersonExternalIdToDelete}?userId={model.LoggedInUserId}&organisationId={model.OrganisationId}&serviceRoleId={model.ServiceRoleId}");
    }

    public async Task<HttpResponseMessage?> DeletePersonConnectionAndEnrolment(DeleteUserModel model)
    {
        return await httpClient.DeleteAsync($"{_accountsEndpointsConfig.DeletePersonConnectionAndEnrolment}/{model.PersonExternalIdToDelete}?userId={model.LoggedInUserId}&organisationId={model.OrganisationId}&enrolmentId={model.EnrolmentId}");
    }
}
