namespace FacadeAccountCreation.Core.Models.Notifications
{
    public class NotificationsResponse
    {
        public List<Notification> Notifications { get; set; }
    }

    public class Notification
    {
        public string Type { get; set; }
        public ICollection<KeyValuePair<string, string>> Data { get; set; }
    }
}