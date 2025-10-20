using CounterStrikeSharp.API.Modules.Cvars;
using System.Net;

namespace NeedSystemAnnouncer.Utils;

public static class ServerHelper
{
    private static string? _cachedServerAddress;
    private static readonly object _cacheLock = new();

    private static string GetServerAddressFromConVar()
    {
        try
        {
            string? ip = ConVar.Find("ip")?.StringValue;
            string? port = ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString();

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(port))
                return string.Empty;

            if (!IPAddress.TryParse(ip, out _))
                return string.Empty;

            return $"{ip}:{port}";
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[NeedSystem-Announcer] Error getting server address from ConVar: {ex.Message}");
            Console.ResetColor();
            return string.Empty;
        }
    }

    public static string GetCachedServerAddress()
    {
        lock (_cacheLock)
        {
            return _cachedServerAddress ?? string.Empty;
        }
    }

    public static void UpdateCachedServerAddress()
    {
        string serverAddress = GetServerAddressFromConVar();

        lock (_cacheLock)
        {
            _cachedServerAddress = serverAddress;
        }

        if (!string.IsNullOrEmpty(serverAddress))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[NeedSystem-Announcer] Server address cached: {serverAddress}");
            Console.ResetColor();
        }
    }
}