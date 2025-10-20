using System.Text.Json.Serialization;

namespace NeedSystemAnnouncer.Configs;

public class ServerSettings
{
    [JsonPropertyName("GetIPandPORTautomatic")]
    public bool GetIPandPORTautomatic { get; set; } = true;

    [JsonPropertyName("IPandPORT")]
    public string IPandPORT { get; set; } = "";
}