using CounterStrikeSharp.API;
using NeedSystemAnnouncer.Configs;
using NeedSystemAnnouncer.Utils;

namespace NeedSystemAnnouncer.Services;

public class AnnouncementService
{
    private readonly DatabaseService _databaseService;
    private readonly AnnouncementSettings _settings;
    private readonly NeedSystemAnnouncerBase _plugin;
    private readonly Queue<DateTime> _recentAnnouncements = new();
    private readonly object _announcementLock = new();

    public AnnouncementService(DatabaseService databaseService, AnnouncementSettings settings, NeedSystemAnnouncerBase plugin)
    {
        _databaseService = databaseService;
        _settings = settings;
        _plugin = plugin;
    }

    public async Task CheckAndAnnounceNewNotifications()
    {
        try
        {
            // Usar la versión en caché que es segura para threads
            string currentServerAddress = ServerHelper.GetCachedServerAddress();

            if (string.IsNullOrEmpty(currentServerAddress))
            {
                LogWarning("Could not determine current server address from cache");
                return;
            }

            if (!CanAnnounce())
            {
                if (_settings.EnableDebug)
                    LogWarning("Rate limit reached, skipping announcement check");
                return;
            }

            var notification = await _databaseService.GetLatestUnprocessedNotification(
                currentServerAddress,
                _settings.AnnounceOwnServer);

            if (notification == null)
            {
                return;
            }

            // Capturar la dirección del servidor actual para usarla en el NextFrame
            string serverAddress = currentServerAddress;

            Server.NextFrame(() =>
            {
                try
                {
                    string message = FormatAnnouncementMessage(notification);
                    Server.PrintToChatAll(message);
                    LogInfo($"Announced notification from {notification.ServerAddress} - Map: {notification.MapName}");

                    lock (_announcementLock)
                    {
                        _recentAnnouncements.Enqueue(DateTime.Now);
                    }

                    // Marcar como procesado de forma asíncrona después del anuncio
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _databaseService.MarkNotificationAsProcessed(notification.Uuid, serverAddress);
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error marking notification as processed: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    LogError($"Error announcing on main thread: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            LogError($"Error checking and announcing notifications: {ex.Message}");
        }
    }

    private bool CanAnnounce()
    {
        lock (_announcementLock)
        {
            var oneMinuteAgo = DateTime.Now.AddMinutes(-1);

            while (_recentAnnouncements.Count > 0 && _recentAnnouncements.Peek() < oneMinuteAgo)
            {
                _recentAnnouncements.Dequeue();
            }

            return _recentAnnouncements.Count < _settings.MaxAnnouncementsPerMinute;
        }
    }

    private string FormatAnnouncementMessage(Models.NotificationRecord notification)
    {
        return $"{_plugin.Localizer["Prefix"]} {_plugin.Localizer["notification.players_needed",
            notification.ServerAddress,
            notification.MapName,
            notification.ConnectedPlayers,
            notification.MaxPlayers,
            notification.RequestedBy]}";
    }

    private void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[NeedSystem-Announcer] {message}");
        Console.ResetColor();
    }

    private void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[NeedSystem-Announcer] {message}");
        Console.ResetColor();
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[NeedSystem-Announcer] {message}");
        Console.ResetColor();
    }
}