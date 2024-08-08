namespace FacadeAccountCreation.API.Controllers;

using Core.Models.Person;
using Core.Services.Person;
using Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Net;

[ApiController]
[AllowAnonymous]
[RequiredScope("account-creation", "account-management")]
[Route("api/persons")]
public class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonsController> _logger;

    public PersonsController(
        IPersonService personService,
        ILogger<PersonsController> logger)
    {
        _personService = personService;
        _logger = logger;
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

    //[HttpPut]
    //[Route("update-user-details/{userId}")]
    //[Consumes("application/json")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> PutUserDetailsByUserId(
    //    Guid userId,
    //    [FromBody] UserDetailsUpdateModel? userDetailsDto)
    //{
    //    if (userId == null)
    //    {
    //        _logger.LogError($"UserId was null in the API call");
    //        return HandleError.HandleErrorWithStatusCode(HttpStatusCode.BadRequest);
    //    }

    //    try
    //    {
    //        await _personService.UpdateUserDetailsByUserId(
    //            userId,
    //            userDetailsDto);

    //        return Ok();
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogError($"Error updating the user details for user {userId}");
    //        return HandleError.Handle(e);
    //    }
    //}
}
