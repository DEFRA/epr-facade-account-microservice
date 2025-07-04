using FacadeAccountCreation.Core.Models.B2c;
using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.B2c;
using Microsoft.AspNetCore.Authorization;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/b2c")]
public class B2CController(
    ILogger<B2CController> logger,
    IB2CService b2cService)
    : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [Route("user-organisation-ids")]
    [ProducesResponseType(typeof(UserOrganisationsListModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserOrganisationIds(UserOrganisationIdentifiersRequest request)
    {
        try
        {
            var userId = request.ObjectId;
            if (userId == Guid.Empty)
            {
                logger.LogError("Unable to get the {UserId} for the user when attempting to get organisation ids", nameof(userId));
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }

            var response = await b2cService.GetUserOrganisationIds(request);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Fetched the organisations list successfully for the user {UserId}", userId);
                return Ok(await response.Content.ReadFromJsonAsync<UserOrganisationIdentifiersResponse>());
            }

            logger.LogError("Failed to fetch the organisations list for the user {UserId}", userId);
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching the organisations list for the user");
            return HandleError.Handle(e);
        }
    }
}
