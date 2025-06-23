using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Person;
using Microsoft.AspNetCore.Authorization;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[AllowAnonymous]
[RequiredScope("account-creation", "account-management")]
[Route("api/persons")]
public class PersonsController(
    IPersonService personService)
    : ControllerBase
{
    [HttpGet("current", Name = "GetCurrent")]
    [ProducesResponseType(typeof(PersonResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCurrent()
    {
        var personResponse = await personService.GetPersonByUserIdAsync(User.UserId());

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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPerson([FromQuery] Guid userId)
    {
        var personResponse = await personService.GetPersonByUserIdAsync(userId);

        return personResponse == null ? NotFound() : Ok(personResponse);
    }

    [HttpGet]
    [Route("all-persons")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersonResponseModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllPerson([FromQuery] Guid userId)
    {
        var personResponse = await personService.GetAllPersonByUserIdAsync(userId);

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
        var personResponse = await personService.GetPersonByExternalIdAsync(externalId);

        return personResponse == null ? NotFound() : Ok(personResponse);
    }

    [HttpGet]
    [Route("person-by-invite-token")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InviteApprovedUserModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonFromInviteToken([FromQuery] string token)
    {
        var personResponse = await personService.GetPersonByInviteToken(token);

        return personResponse == null ? NotFound() : Ok(personResponse);
    }
}
