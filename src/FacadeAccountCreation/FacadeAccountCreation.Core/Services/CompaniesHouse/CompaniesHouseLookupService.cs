namespace FacadeAccountCreation.Core.Services.CompaniesHouse;

public class CompaniesHouseLookupService(HttpClient httpClient) : ICompaniesHouseLookupService
{
    private const string CompaniesHouseEndpoint = "CompaniesHouse/companies";

    public async Task<CompaniesHouseResponse?> GetCompaniesHouseResponseAsync(string id)
    {
        var uriBuilder = new UriBuilder($"{CompaniesHouseEndpoint}/{Uri.EscapeDataString(id)}");
        var endpoint = "CompaniesHouse" + uriBuilder.Path;

        var response = await httpClient.GetAsync(endpoint);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {        
            var errorResponse = await response.Content.ReadFromJsonAsync<CompaniesHouseErrorResponse>();
            if (errorResponse?.InnerException?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CompaniesHouseResponse>();
    }
}
