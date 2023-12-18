using FacadeAccountCreation.Core.Models.Notifications;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace FacadeAccountCreation.Core.Services.Connection;

public class NotificationsService : INotificationsService
{
    private const string NotificationsUri = "api/notifications";
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationsService> _logger;

    public NotificationsService(HttpClient httpClient, ILogger<NotificationsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<NotificationsResponse?> GetNotificationsForServiceAsync(Guid userId, Guid organisationId, string serviceKey)
    {
        _logger.LogInformation("Attempting to get the notifications for userId {userId} in organisation {organisationId} for service {serviceKey}", userId, organisationId, serviceKey);

        _httpClient.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
        _httpClient.DefaultRequestHeaders.Add("X-EPR-Organisation", organisationId.ToString());

        var response = await _httpClient.GetAsync($"{NotificationsUri}?serviceKey={serviceKey}");
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<NotificationsResponse>();
    }
}
