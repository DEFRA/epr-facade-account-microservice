using System.Net;

namespace FacadeAccountCreation.Core.Models.CompaniesHouse;

public class CompaniesHouseErrorResponse
{
    public InnerExceptionResponse? InnerException { get; init; }
}
