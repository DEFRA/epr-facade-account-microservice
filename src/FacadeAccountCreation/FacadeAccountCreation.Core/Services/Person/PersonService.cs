using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Person;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace FacadeAccountCreation.Core.Services.Person
{
    public class PersonService : IPersonService
    {
        private const string PersonsUri = "api/persons";
        private const string AllPersonsUri = $"{PersonsUri}/allpersons";
        private const string PersonsWithExternalIdUri = "api/persons/person-by-externalId";
        private const string PersonsByInviteToken = "api/persons/person-by-invite-token";
        private readonly HttpClient _httpClient;

        public PersonService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<PersonResponseModel?> GetPersonByUserIdAsync(Guid userId)
        {
            return await GetPersonByUserIdWithUrlAsync(userId, PersonsUri);
        }

        public async Task<PersonResponseModel?> GetAllPersonByUserIdAsync(Guid userId)
        {
            return await GetPersonByUserIdWithUrlAsync(userId, AllPersonsUri);
        }

        public async Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId)
        {
            var response = await _httpClient.GetAsync($"{PersonsWithExternalIdUri}?externalId={externalId}");
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

            return await response.Content.ReadFromJsonWithEnumsAsync<PersonResponseModel>();
        }

        public async Task<InviteApprovedUserModel> GetPersonByInviteToken(string token)
        {
            var response = await _httpClient.GetAsync($"{PersonsByInviteToken}?token={token}");
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
            var serviceRoleId = response.Content.ReadFromJsonWithEnumsAsync<InviteApprovedUserModel>();
            return serviceRoleId.Result;
        }
        private async Task<PersonResponseModel?> GetPersonByUserIdWithUrlAsync(Guid userId, string url)
        {
            var response = await _httpClient.GetAsync($"{url}?userId={userId}");
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonWithEnumsAsync<PersonResponseModel>();
        }
    }
}
