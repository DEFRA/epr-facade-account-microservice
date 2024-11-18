using FacadeAccountCreation.Core.Models.AddressLookup;

namespace FacadeAccountCreation.Core.Services.AddressLookup;

public class AddressLookupService(HttpClient httpClient) : IAddressLookupService
{
    private const string PostcodeEndpoint = "postcodes";
    private const string PostcodeQueryStringKey = "postcode";

    public async Task<AddressLookupResponseDto?> GetAddressLookupResponseAsync(string postcode)
    {
        var url = $"{PostcodeEndpoint}?{PostcodeQueryStringKey}={postcode}";

        var response = await httpClient.GetAsync(url);

        switch (response.StatusCode)
        {
            case HttpStatusCode.NoContent:
                return null;
            case HttpStatusCode.BadRequest:
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<AddressLookupErrorResponse>();
                if (errorResponse?.Error?.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    return null;
                }
                break;
            }
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AddressLookupResponseDto>();
    }
}
