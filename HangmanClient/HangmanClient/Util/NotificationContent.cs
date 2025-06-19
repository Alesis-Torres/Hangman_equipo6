namespace HangmanClient.Util
{
    public class NotificationContent
    {
        public string NotificationTitle { get; set; } = string.Empty;
        public string NotificationMessage { get; set; } = string.Empty;
        public string AcceptButtonText { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationContent() { }
    }
}
