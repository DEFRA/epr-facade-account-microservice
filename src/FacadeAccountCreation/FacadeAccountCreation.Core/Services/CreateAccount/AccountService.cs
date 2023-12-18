using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.CreateAccount;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;
using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FacadeAccountCreation.Core.Services.CreateAccount;

public class AccountService : IAccountService
{
    private readonly HttpClient _httpClient;
    private readonly AccountsEndpointsConfig _accountsEndpointsConfig;

    public AccountService(HttpClient httpClient, IOptions<AccountsEndpointsConfig> accountsEndpointsConfigOptions)
    {
        _httpClient = httpClient;
        _accountsEndpointsConfig = accountsEndpointsConfigOptions.Value;
    }

    public async Task<CreateAccountResponse?> AddAccountAsync(AccountWithUserModel accountWithUser )
    {
        var response = await _httpClient.PostAsJsonAsync(_accountsEndpointsConfig.Accounts, accountWithUser);

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
        var response = await _httpClient.GetAsync($"{_accountsEndpointsConfig.Organisations}?companiesHouseNumber={companiesHouseNumber}");

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
        return  await _httpClient.PostAsJsonAsync(_accountsEndpointsConfig.InviteUser, accountInvitation);
    }

    public async Task<HttpResponseMessage> EnrolInvitedUserAsync(EnrolInvitedUserModel enrolInvitedUserModel)
    {
        return await _httpClient.PostAsJsonAsync(_accountsEndpointsConfig.EnrolInvitedUser, enrolInvitedUserModel);
    }
}
