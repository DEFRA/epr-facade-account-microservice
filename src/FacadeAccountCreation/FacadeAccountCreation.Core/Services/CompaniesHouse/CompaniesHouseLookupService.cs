﻿using FacadeAccountCreation.Core.Models.CompaniesHouse;
using System.Net;
using System.Net.Http.Json;

namespace FacadeAccountCreation.Core.Services.CompaniesHouse;

public class CompaniesHouseLookupService : ICompaniesHouseLookupService
{
    private const string CompaniesHouseEndpoint = "CompaniesHouse/companies";

    private readonly HttpClient _httpClient;

    public CompaniesHouseLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CompaniesHouseResponse?> GetCompaniesHouseResponseAsync(string id)
    {
        var response = await _httpClient.GetAsync($"{CompaniesHouseEndpoint}/{id}");
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
