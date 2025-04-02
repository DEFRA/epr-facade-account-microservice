using System.Collections.ObjectModel;
using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Exceptions;

namespace FacadeAccountCreation.Core.Services.CreateAccount;

public class AccountService(HttpClient httpClient, IOptions<AccountsEndpointsConfig> accountsEndpointsConfigOptions)
    : IAccountService
{
    private readonly AccountsEndpointsConfig _accountsEndpointsConfig = accountsEndpointsConfigOptions.Value;

    public async Task<CreateAccountResponse?> AddAccountAsync(AccountWithUserModel accountWithUser )
    {
        var response = await httpClient.PostAsJsonAsync(_accountsEndpointsConfig.Accounts, accountWithUser);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }

        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>();

        return result;
    }

    //todo: should we send a AccountWithUserModel with empty entities, or just accept a per
    //todo: have here, or create a ReprocessorExporterAccountService?
    //todo: do we need to return Task<HttpResponseMessage> ?
    //public async Task<CreateAccountResponse?> AddReprocessorExporterAccountAsync(AccountWithUserModel accountWithUser)
    //public async Task AddReprocessorExporterAccountAsync(AccountWithUserModel accountWithUser)
    public async Task AddReprocessorExporterAccountAsync(ReprocessorExporterAccountModel account)
    {
        var response = await httpClient.PostAsJsonAsync(_accountsEndpointsConfig.ReprocessorExporterAccounts, account);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();
        }

        //response.EnsureSuccessStatusCode();

        //var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>();

        //return result;
    }

    public async Task<CreateAccountResponse?> AddApprovedUserAccountAsync(AccountModel approvedUser )
    {
        
        var response = await httpClient.PostAsJsonAsync(_accountsEndpointsConfig.ApprovedUserAccounts, approvedUser);
        
        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }

        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>();

        return result;
    }
    
    public async Task<IReadOnlyCollection<OrganisationResponseModel>?> GetOrganisationsByCompanyHouseNumberAsync(string companiesHouseNumber)
    {
        var response = await httpClient.GetAsync($"{_accountsEndpointsConfig.Organisations}?companiesHouseNumber={companiesHouseNumber}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }

        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonWithEnumsAsync<List<OrganisationResponseModel>>();

        return new ReadOnlyCollection<OrganisationResponseModel>(result!);
    }
    
    public async Task<HttpResponseMessage> SaveInviteAsync(AccountInvitationModel accountInvitation)
    {
        return  await httpClient.PostAsJsonAsync(_accountsEndpointsConfig.InviteUser, accountInvitation);
    }

    public async Task<HttpResponseMessage> EnrolInvitedUserAsync(EnrolInvitedUserModel enrolInvitedUserModel)
    {
        return await httpClient.PostAsJsonAsync(_accountsEndpointsConfig.EnrolInvitedUser, enrolInvitedUserModel);
    }
}
