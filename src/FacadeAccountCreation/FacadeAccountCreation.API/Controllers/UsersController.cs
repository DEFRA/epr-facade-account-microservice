using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.User;

namespace FacadeAccountCreation.API.Controllers;

[ApiController]
[Route("api/user-accounts")]
public class UsersController(
    ILogger<UsersController> logger,
    IUserService userService,
    IMessagingService messagingService)
    : ControllerBase
{
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
                logger.LogError("Unable to get the OId for the user when attempting to get organisation details");
                return Problem("UserId not available", statusCode: StatusCodes.Status500InternalServerError);
            }
            var response = await userService.GetUserOrganisations(userId);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Fetched the organisations list successfully for the user {UserId}", userId);
                return Ok(await response.Content.ReadFromJsonAsync<UserOrganisationsListModel>());
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
            var response = await userService.UpdatePersonalDetailsAsync(userId, organisationId, serviceKey, updateUserDetailsRequest);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<UpdateUserDetailsResponse>();
                if (responseContent.HasApprovedOrDelegatedUserDetailsSentForApproval && responseContent.ChangeHistory != null)
                {
                    try
                    {
                        var ch = responseContent.ChangeHistory;
                        var notifyEmailInput = new UserDetailsChangeNotificationEmailInput
                        {
                              Nation = ch.Nation,
                              ContactEmailAddress = ch.EmailAddress,
                              ContactTelephone = ch.Telephone,
                              OrganisationName = ch.OrganisationName ?? "",
                              OrganisationNumber = ch.OrganisationReferenceNumber?.ToReferenceNumberFormat(),
                              NewFirstName = ch.NewValues.FirstName,
                              NewLastName = ch.NewValues.LastName,
                              NewJobTitle = ch.NewValues.JobTitle ?? "",
                              OldFirstName = ch.OldValues.FirstName,
                              OldLastName = ch.OldValues.LastName,
                              OldJobTitle = ch.OldValues.JobTitle ?? "",
                        };

                     var notificationId =   messagingService.SendUserDetailChangeRequestEmailToRegulator(notifyEmailInput);

                        logger.LogInformation("UserDetailChangeRequest Notification email {NotificationId} to regulator sent successfully for the user {UserId} from organisation {OrganisationId}", notificationId, userId, organisationId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "failed to send update user details request email notification to regulator for user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'.", userId, organisationId, serviceKey);
                    }
                }

                logger.LogInformation("Update personal details successfully for the user {UserId} from organisation {OrganisationId}", userId, organisationId);

                return Ok(responseContent);
            }

            logger.LogError("failed to update personal details for the user {UserId} from organisation {OrganisationId}", userId, organisationId);
            return HandleError.HandleErrorWithStatusCode(response.StatusCode);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "failed to update personal details for the user {UserId} from organisation {OrganisationId} of service '{ServiceKey}'.",
                userId, organisationId, serviceKey);
            return HandleError.Handle(exception);
        }
    }
}
