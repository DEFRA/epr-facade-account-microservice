using FacadeAccountCreation.Core.Models.Notifications;

namespace FacadeAccountCreation.Core.Services.Notification;

public class NotificationsService(HttpClient httpClient, ILogger<NotificationsService> logger)
    : INotificationsService
{
    private const string NotificationsUri = "api/notifications";

    public async Task<NotificationsResponse?> GetNotificationsForServiceAsync(Guid userId, Guid organisationId, string serviceKey)
    {
        logger.LogInformation("Attempting to get the notifications for userId {UserId} in organisation {OrganisationId} for service {ServiceKey}", userId, organisationId, serviceKey);

        httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
        httpClient.DefaultRequestHeaders.Add("X-EPR-Organisation", organisationId.ToString());

        var response = await httpClient.GetAsync($"{NotificationsUri}?serviceKey={serviceKey}");
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<NotificationsResponse>();
    }
}
