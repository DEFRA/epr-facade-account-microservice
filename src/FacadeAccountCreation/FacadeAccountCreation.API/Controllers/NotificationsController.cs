using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.Shared;
using FacadeAccountCreation.Core.Models.Notifications;
using FacadeAccountCreation.Core.Services.Connection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace FacadeAccountCreation.API.Controllers
{
    [Route("api/notifications")]
    public class NotificationsController : Controller
    {
        private readonly INotificationsService _notificationsService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(ILogger<NotificationsController> logger, INotificationsService notificationsService)
        {
            _logger = logger;
            _notificationsService = notificationsService;
        }

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

                var notificationResponse = await _notificationsService.GetNotificationsForServiceAsync(userId, organisationId, serviceKey);
                if (notificationResponse == null)
                {
                    _logger.LogError("No notifications found for user {userId} in organisation {organisationId} for service {serviceKey}", userId, organisationId, serviceKey);
                    return new NoContentResult();
                }
                
                _logger.LogInformation("Fetched the notifications for user {userId} in organisation {organisationId} for service {serviceKey}", userId, organisationId, serviceKey);
                return Ok(notificationResponse);
            }
            catch (Exception e)
            {
                _logger.LogError("Error fetching the notifications for user {userId} in organisation {organisationId} for service {serviceKey}", User.UserId(), organisationId, serviceKey);
                return HandleError.Handle(e);
            }
        }
    }
}