using FacadeAccountCreation.Core.Models.AddressLookup;

namespace FacadeAccountCreation.Core.Services.AddressLookup;

public interface IAddressLookupService
{
    Task<AddressLookupResponseDto?> GetAddressLookupResponseAsync(string postcode);
}