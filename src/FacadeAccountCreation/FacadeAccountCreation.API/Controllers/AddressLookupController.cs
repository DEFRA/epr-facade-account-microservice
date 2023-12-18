using FacadeAccountCreation.Core.Services.AddressLookup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.ComponentModel.DataAnnotations;
using FacadeAccountCreation.Core.Models.AddressLookup;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[RequiredScope("account-creation")]
[Route("api/address-lookup")]
public class AddressLookupController : ControllerBase
{
    private readonly IAddressLookupService _addressLookupService;

    public AddressLookupController(
        IAddressLookupService addressLookupService)
    {
        _addressLookupService = addressLookupService;
    }

    [HttpGet(Name = "AddressLookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<AddressLookupResponseDto>> Get(
        [FromQuery][RegularExpression(Constants.Regexs.PostcodeRegex, ErrorMessage = "Invalid post code")]
        string postcode)
    {
        var response = await _addressLookupService.GetAddressLookupResponseAsync(postcode);
        if (response == null)
        {
            return NoContent();
        }

        return Ok(response);
    }
}
