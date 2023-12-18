using System.Net;
using System.Net.Http.Json;
using FacadeAccountCreation.Core.Models.AddressLookup;

namespace FacadeAccountCreation.Core.Services.AddressLookup;

public class AddressLookupService : IAddressLookupService
{
    private const string PostcodeEndpoint = "postcodes";
    private const string PostcodeQueryStringKey = "postcode";

    private readonly HttpClient _httpClient;

    public AddressLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AddressLookupResponseDto?> GetAddressLookupResponseAsync(string postcode)
    {
        var url = $"{PostcodeEndpoint}?{PostcodeQueryStringKey}={postcode}";

        var response = await _httpClient.GetAsync(url);

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
