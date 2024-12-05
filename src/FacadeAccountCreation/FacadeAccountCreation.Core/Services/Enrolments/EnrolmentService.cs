using FacadeAccountCreation.Core.Configs;

namespace FacadeAccountCreation.Core.Services.Enrolments;

public class EnrolmentService(
             HttpClient httpClient,
             ILogger<EnrolmentService> logger,
             IOptions<AccountsEndpointsConfig> accountsConfigOptions,
             IOptions<EnrolmentApplicationEndpointsConfig> enrolmentApplicationConfigOptions)
             : IEnrolmentService
{
    private readonly AccountsEndpointsConfig _accountsEndpointsConfig = accountsConfigOptions.Value;
    private readonly EnrolmentApplicationEndpointsConfig _enrolmentApplicationEndpointsConfig = enrolmentApplicationConfigOptions.Value;

    public async Task<HttpResponseMessage?> DeleteUser(DeleteUserModel model)
    {
        return await httpClient.DeleteAsync($"{_accountsEndpointsConfig.DeleteUser}/{model.PersonExternalIdToDelete}?userId={model.LoggedInUserId}&organisationId={model.OrganisationId}&serviceRoleId={model.ServiceRoleId}");
    }

    public async Task<HttpResponseMessage> GetOrganisationPendingApplications(Guid userId, Guid organisationId)
    {
        var url = string.Format($"{_enrolmentApplicationEndpointsConfig.GetOrganisationsApplications}", userId, organisationId);

        logger.LogInformation("Attempting to fetch enrolment applications for organisation {OrganisationId}", organisationId);

        return await httpClient.GetAsync(url);
    }
}
