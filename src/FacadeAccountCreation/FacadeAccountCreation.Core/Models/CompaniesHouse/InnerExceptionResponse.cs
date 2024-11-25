namespace FacadeAccountCreation.Core.Models.CompaniesHouse;

[ExcludeFromCodeCoverage]
public class InnerExceptionResponse
{
    public HttpStatusCode? StatusCode => Code != null ? (HttpStatusCode)int.Parse(Code) : null;

    public string? Code { get; init; }
}
