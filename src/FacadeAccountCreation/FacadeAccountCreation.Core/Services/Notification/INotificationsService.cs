using FacadeAccountCreation.Core.Models.Notifications;

namespace FacadeAccountCreation.Core.Services.Notification;

public interface INotificationsService
{
    Task<NotificationsResponse?> GetNotificationsForServiceAsync(Guid userId, Guid organisationId, string serviceKey);
}