using System.Text.Json.Serialization;

namespace NeedSystemAnnouncer.Configs;

public class DatabaseConfig
{
    [JsonPropertyName("Host")]
    public string Host { get; set; } = "localhost";

    [JsonPropertyName("Port")]
    public uint Port { get; set; } = 3306;

    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("Password")]
    public string Password { get; set; } = "";

    [JsonPropertyName("DatabaseName")]
    public string DatabaseName { get; set; } = "";

    [JsonPropertyName("ConnectionTimeout")]
    public int ConnectionTimeout { get; set; } = 30;
}