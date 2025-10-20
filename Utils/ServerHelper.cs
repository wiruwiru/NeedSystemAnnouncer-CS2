using CounterStrikeSharp.API.Modules.Cvars;
using System.Net;

namespace NeedSystemAnnouncer.Utils;

public static class ServerHelper
{
    public static string GetCurrentServerAddress()
    {
        string? ip = ConVar.Find("ip")?.StringValue;
        string? port = ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString();

        if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(port))
            return string.Empty;

        if (!IPAddress.TryParse(ip, out _))
            return string.Empty;

        return $"{ip}:{port}";
    }
}