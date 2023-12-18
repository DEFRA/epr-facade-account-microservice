using FacadeAccountCreation.Core.Extensions;
using FacadeAccountCreation.Core.Models.Person;
using System.Net;
using System.Net.Http.Json;
using FacadeAccountCreation.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FacadeAccountCreation.Core.Services.Person
{
    public class PersonService : IPersonService
    {
        private const string PersonsUri = "api/persons";
        private const string PersonsWithExternalIdUri = "api/persons/person-by-externalId";
        private readonly HttpClient _httpClient;

        public PersonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PersonResponseModel?> GetPersonByUserIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"{PersonsUri}?userId={userId}");
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonWithEnumsAsync<PersonResponseModel>();
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
    }
}
