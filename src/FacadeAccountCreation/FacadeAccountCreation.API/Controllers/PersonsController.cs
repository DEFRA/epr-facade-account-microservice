using Microsoft.AspNetCore.Authorization;

namespace FacadeAccountCreation.API.Controllers;

using Extensions;
using Core.Services.Person;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Core.Models.Person;

[ApiController]
[AllowAnonymous]
[RequiredScope("account-creation", "account-management")]
[Route("api/persons")]
public class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;

    public PersonsController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpGet("current", Name = "GetCurrent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCurrent()
    {
        var personResponse = await _personService.GetPersonByUserIdAsync(User.UserId());
        
        if (personResponse == null)
        {
            return NoContent();
        }
    
        return Ok(personResponse);
    }

    [HttpGet]
    [Route("")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersonResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPerson([FromQuery] Guid userId)
    {
        var personResponse = await _personService.GetPersonByUserIdAsync(userId);

        return personResponse == null ? NotFound() : Ok(personResponse);
    }
    
    [HttpGet]
    [Route("person-by-externalId")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersonResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonFromExternalId([FromQuery] Guid externalId)
    {
        var personResponse = await _personService.GetPersonByExternalIdAsync(externalId);

        return personResponse == null ? NotFound() : Ok(personResponse);
    }
    
    [HttpGet]
    [Route("person-by-invite-token")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersonResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonFromInviteToken([FromQuery] string token)
    {
        var personResponse = await _personService.GetPersonByInviteToken(token);

        return personResponse == null ? NotFound() : Ok(personResponse);
    }
}
