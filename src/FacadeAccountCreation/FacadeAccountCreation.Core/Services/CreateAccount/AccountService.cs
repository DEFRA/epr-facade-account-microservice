using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Exceptions;
using System;
using System.Collections.ObjectModel;

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

    public async Task AddReprocessorExporterAccountAsync(
        ReprocessorExporterAccountWithUserModel accountWithUser,
        string serviceKey)
    {
        var response = await PostAsJsonAsyncWithAuditHeaders(
            $"{_accountsEndpointsConfig.ReprocessorExporterAccounts}?serviceKey={serviceKey}",
            accountWithUser,
            accountWithUser.User.UserId!.Value);

        if (!response.IsSuccessStatusCode)
        {
            ProblemDetails? problemDetails = null;
            try
            {
                problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            }
            catch (JsonException e)
            {
                // if the response isn't a valid ProblemDetails, either this exception is thrown,
                // or in some circumstances, null is returned.
                // we handle both scenarios next
            }

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();
        }
    }

    /// <summary>
    /// Thread-safe version of PostAsJsonAsync that adds out-of-bounds audit headers.
    /// (Other code in the solution adds headers in a thread unsafe manner.)
    /// </summary>
    private Task<HttpResponseMessage> PostAsJsonAsyncWithAuditHeaders<TValue>(
        string requestUri,
        TValue value,
        Guid userId)
    {
        const string xEprUserHeader = "X-EPR-User";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value)
        };

        request.Headers.Add(xEprUserHeader, userId.ToString());

        return httpClient.SendAsync(request);
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
