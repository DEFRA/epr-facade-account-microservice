using FacadeAccountCreation.Core.Models.Notifications;

namespace FacadeAccountCreation.Core.Services.Connection
{
    public interface INotificationsService
    {
        Task<NotificationsResponse?> GetNotificationsForServiceAsync(Guid userId, Guid organisationId, string serviceKey);
    }
}