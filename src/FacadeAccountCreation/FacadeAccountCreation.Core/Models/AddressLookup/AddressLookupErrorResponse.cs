namespace FacadeAccountCreation.Core.Models.AddressLookup;

public class AddressLookupErrorResponse
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }
    public ErrorDetails? Error { get; set; }
}
