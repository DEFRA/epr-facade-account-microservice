using FacadeAccountCreation.Core.Models.Notifications;
using FacadeAccountCreation.Core.Services.Notification;

namespace FacadeAccountCreation.API.Controllers;

[Route("api/notifications")]
public class NotificationsController(
    ILogger<NotificationsController> logger,
    INotificationsService notificationsService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<NotificationsResponse>> GetNotifications(
        [BindRequired, FromQuery] string serviceKey,
        [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        try
        {
            var userId = User.UserId();

            var notificationResponse = await notificationsService.GetNotificationsForServiceAsync(userId, organisationId, serviceKey);
            if (notificationResponse == null)
            {
                logger.LogError("No notifications found for user {UserId} in organisation {OrganisationId} for service {ServiceKey}", userId, organisationId, serviceKey);
                return new NoContentResult();
            }
                
            logger.LogInformation("Fetched the notifications for user {UserId} in organisation {OrganisationId} for service {ServiceKey}", userId, organisationId, serviceKey);
            return Ok(notificationResponse);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching the notifications for user {UserId} in organisation {OrganisationId} for service {ServiceKey}", User.UserId(), organisationId, serviceKey);
            return HandleError.Handle(e);
        }
    }
}