using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Services.CompaniesHouse;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[RequiredScope("account-creation")]
[Route("api/companies-house")]
public class CompaniesHouseController(
    ICompaniesHouseLookupService companiesHouseLookupService,
    IAccountService accountService)
    : ControllerBase
{
    [HttpGet(Name = "CompanyLookup")]
    [ProducesResponseType(typeof(CompaniesHouseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<CompaniesHouseResponse>> Get([Required] string id)
    {
        var companyResponse = await companiesHouseLookupService.GetCompaniesHouseResponseAsync(id);
        
        if (companyResponse == null)
        {
            return NoContent();
        }

        var organisationResponse = await accountService.GetOrganisationsByCompanyHouseNumberAsync(id);

        if (organisationResponse != null && organisationResponse.Count > 0)
        {
            companyResponse.AccountCreatedOn = organisationResponse.OrderBy(o => o.CreatedOn).First().CreatedOn;
        }

        return Ok(companyResponse);
    }
}
