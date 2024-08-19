using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.Core.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/user-accounts")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;
    private readonly IMessagingService _messagingService;

    public UsersController(
        ILogger<UsersController> logger,
        IUserService userService,
        IMessagingService messagingService)
    {
        _logger = logger;
        _userService = userService;
        _messagingService = messagingService;
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
       [BindRequired, FromBody] UpdateUserDetailsRequest updateUserDetailsRequest,
       [BindRequired, FromQuery] string serviceKey,
       [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        var userId = User.UserId();
        try
        {
            var response = await _userService.UpdatePersonalDetailsAsync(userId, organisationId, serviceKey, updateUserDetailsRequest);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<UpdateUserDetailsResponse>();
                if (responseContent.HasApprovedOrDelegatedUserDetailsSentForApproval && responseContent.ChangeHistory != null)
                {
                    try
                    {
                        var ch = responseContent.ChangeHistory;
                        var notifyEmailInput = new UserDetailsChangeNotificationEmailInput()
                        {
                              Nation = ch.Nation,
                              ExternalIdReference = ch.ExternalId,
                              ContactEmailAddress = ch.EmailAddress,
                              ContactTelephone = ch.Telephone,
                              OrganisationName = ch.OrganisationName,
                              OrganisationNumber = ch.OrganisationNumber,
                              NewFirstName = ch.NewValues.FirstName,
                              NewLastName = ch.NewValues.LastName,
                              NewJobTitle = ch.NewValues.JobTitle,
                              OldFirstName = ch.OldValues.FirstName,
                              OldLastName = ch.OldValues.LastName,
                              OldJobTitle = ch.OldValues.JobTitle
                        };

                     var notificationId =   _messagingService.SendUserDetailChangeRequestEmailToRegulator(notifyEmailInput);

                        _logger.LogInformation("UserDetailChangeRequest Notification email {notificationId} to regulator sent successfully for the user {userId} from organisation {organisationId}", notificationId, userId, organisationId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "failed to send update user details request email notification to regulator for user {userId} from organisation {organisationId} of service '{serviceKey}'.", userId, organisationId, serviceKey);
                    }
                }

                _logger.LogInformation("Update personal details successfully for the user {userId} from organisation {organisationId}", userId, organisationId);

                return Ok(responseContent);
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
