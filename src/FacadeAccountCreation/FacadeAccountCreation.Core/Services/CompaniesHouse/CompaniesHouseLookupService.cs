using FacadeAccountCreation.Core.Constants;
using Microsoft.FeatureManagement;

namespace FacadeAccountCreation.Core.Services.CompaniesHouse;

public class CompaniesHouseLookupService(
    HttpClient httpClient,
    IFeatureManager featureManager) : ICompaniesHouseLookupService
{
    private string CompaniesHouseEndpoint { get; } = 
        featureManager.IsEnabledAsync(FeatureFlags.UseBoomiOAuth).GetAwaiter().GetResult()
            ? "companies"
            : "CompaniesHouse/companies";

    public async Task<CompaniesHouseResponse?> GetCompaniesHouseResponseAsync(string id)
    {
        var path = $"{CompaniesHouseEndpoint}/{Uri.EscapeDataString(id)}";

        var response = await httpClient.GetAsync(path);

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
 