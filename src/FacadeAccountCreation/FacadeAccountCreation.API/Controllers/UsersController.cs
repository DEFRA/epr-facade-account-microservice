using Azure;
using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/user-accounts")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;

    public UsersController(ILogger<UsersController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrganisation()
    {
        try
        {
            var userId = User.UserId();
            if (userId == Guid.Empty)
            {
                _logger.LogError($"Unable to get the OId for the user when attempting to get organisation details");
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }
            var response = await _userService.GetUserOrganisations(userId);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Fetched the organisations list successfully for the user {userId}", userId);
                return Ok(await response.Content.ReadFromJsonAsync<UserOrganisationsListModel>());
            }
            else
            {
                _logger.LogError("Failed to fetch the organisations list for the user {userId}", userId);
                return HandleError.HandleErrorWithStatusCode(response.StatusCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching the organisations list for the user");
            return HandleError.Handle(e);
        }
    }

    [HttpPut]
    [Consumes("application/json")]
    [Route("personal-details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePersonalDetails(
           [BindRequired, FromBody] UserDetailsUpdateModel updateUserDetailsRequest,
           [BindRequired, FromQuery] string serviceKey,
           [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        var userId = User.UserId();
        try
        {
            var response = await _userService.UpdatePersonalDetailsAsync(userId, organisationId, serviceKey, updateUserDetailsRequest);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Update personal details successfully for the user {userId} from organisation {organisationId}", userId, organisationId);
                return Ok(await response.Content.ReadFromJsonAsync<UpdateUserDetailsResponse>());
            }
            else
            {
                _logger.LogError("failed to update personal details for the user {userId} from organisation {organisationId}", userId, organisationId);
                return HandleError.HandleErrorWithStatusCode(response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("failed to update personal details for the user {userId} from organisation {organisationId} of service '{serviceKey}'.",
                userId, organisationId, serviceKey);
            return HandleError.Handle(exception);
        }
    }
}
