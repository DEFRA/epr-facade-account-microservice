namespace FacadeAccountCreation.Core.Models.AddressLookup;

public class AddressLookupResponseDto
{
    public AddressLookupResponseHeader Header { get; set; } = default!;
    public AddressLookupResponseResult[] Results { get; set; } = default!;
}
