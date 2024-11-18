using FacadeAccountCreation.API.Constants;
using FacadeAccountCreation.Core.Models.AddressLookup;
using FacadeAccountCreation.Core.Services.AddressLookup;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[RequiredScope("account-creation")]
[Route("api/address-lookup")]
public class AddressLookupController(IAddressLookupService addressLookupService) : ControllerBase
{
    [HttpGet(Name = "AddressLookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<AddressLookupResponseDto>> Get(
        [FromQuery][RegularExpression(Regexs.PostcodeRegex, ErrorMessage = "Invalid post code")]
        string postcode)
    {
        var response = await addressLookupService.GetAddressLookupResponseAsync(postcode);
        if (response == null)
        {
            return NoContent();
        }

        return Ok(response);
    }
}
