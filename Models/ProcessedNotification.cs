namespace NeedSystemAnnouncer.Models;

public class ProcessedNotification
{
    public int Id { get; set; }
    public string NotificationUuid { get; set; } = string.Empty;
    public string ProcessedByServer { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}