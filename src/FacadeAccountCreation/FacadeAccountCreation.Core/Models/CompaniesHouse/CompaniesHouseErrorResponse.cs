using System.Net;

namespace FacadeAccountCreation.Core.Models.CompaniesHouse;

public class CompaniesHouseErrorResponse
{
    public InnerExceptionResponse? InnerException { get; init; }
}

public class InnerExceptionResponse
{
    public HttpStatusCode? StatusCode => Code != null ? (HttpStatusCode)int.Parse(Code) : null;

    public string? Code { get; init; }
}
