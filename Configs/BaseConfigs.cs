using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace NeedSystemAnnouncer.Configs;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("AnnouncementSettings")]
    public AnnouncementSettings Announcement { get; set; } = new();

    [JsonPropertyName("ServerSettings")]
    public ServerSettings Server { get; set; } = new();

    [JsonPropertyName("Database")]
    public DatabaseConfig Database { get; set; } = new();
}