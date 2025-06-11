namespace HangmanClient.Util
{
    public class NotificationContent
    {
        public string NotificationTitle { get; set; } = string.Empty;
        public string NotificationMessage { get; set; } = string.Empty;
        public string AcceptButtonText { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public NotificationContent() { }
    }
}
