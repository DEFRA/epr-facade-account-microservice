using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Services.CompaniesHouse;
using FacadeAccountCreation.Core.Services.CreateAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[RequiredScope("account-creation")]
[Route("api/companies-house")]
public class CompaniesHouseController : ControllerBase
{
    private readonly ICompaniesHouseLookupService _companiesHouseLookupService;
    private readonly IAccountService _accountService;

    public CompaniesHouseController(
        ICompaniesHouseLookupService companiesHouseLookupService,
        IAccountService accountService)
    {
        _companiesHouseLookupService = companiesHouseLookupService;
        _accountService = accountService;
    }

    [HttpGet(Name = "CompanyLookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<CompaniesHouseResponse>> Get([Required] string id)
    {
        var companyResponse = await _companiesHouseLookupService.GetCompaniesHouseResponseAsync(id);
        
        if (companyResponse == null)
        {
            return NoContent();
        }

        var organisationResponse = await _accountService.GetOrganisationsByCompanyHouseNumberAsync(id);

        if (organisationResponse != null && organisationResponse.Count > 0)
        {
            companyResponse.AccountCreatedOn = organisationResponse.OrderBy(o => o.CreatedOn).First().CreatedOn;
        }

        return Ok(companyResponse);
    }
}
