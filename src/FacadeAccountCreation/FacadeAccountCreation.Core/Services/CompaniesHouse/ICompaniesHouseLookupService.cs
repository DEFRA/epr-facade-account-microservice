﻿namespace FacadeAccountCreation.Core.Services.CompaniesHouse;

public interface ICompaniesHouseLookupService
{
    Task<CompaniesHouseResponse?> GetCompaniesHouseResponseAsync(string id);
}