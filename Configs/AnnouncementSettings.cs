using System.Text.Json.Serialization;

namespace NeedSystemAnnouncer.Configs;

public class AnnouncementSettings
{
    [JsonPropertyName("AnnounceOwnServer")]
    public bool AnnounceOwnServer { get; set; } = false;

    [JsonPropertyName("CheckIntervalSeconds")]
    public float CheckIntervalSeconds { get; set; } = 30.0f;

    [JsonPropertyName("MaxAnnouncementsPerMinute")]
    public int MaxAnnouncementsPerMinute { get; set; } = 5;

    [JsonPropertyName("CleanOldRecordsDays")]
    public int CleanOldRecordsDays { get; set; } = 7;

    [JsonPropertyName("EnableDebug")]
    public bool EnableDebug { get; set; } = false;
}