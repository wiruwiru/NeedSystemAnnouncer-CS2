using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;

using NeedSystemAnnouncer.Services;
using NeedSystemAnnouncer.Configs;
using NeedSystemAnnouncer.Utils;

namespace NeedSystemAnnouncer;

[MinimumApiVersion(342)]
public class NeedSystemAnnouncerBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    private DatabaseService? _databaseService;
    private AnnouncementService? _announcementService;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _checkTimer;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _cleanupTimer;

    public override string ModuleName => "NeedSystem-Announcer";
    public override string ModuleVersion => "0.0.3";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Shows the latest NeedSystem notification in-game.";

    public required BaseConfigs Config { get; set; }

    public override void Load(bool hotReload)
    {
        InitializeServices();

        Server.NextFrame(() =>
        {
            ServerHelper.UpdateCachedServerAddress();
        });

        StartAnnouncementTimer();
        StartCleanupTimer();
        _ = InitializeDatabaseAsync();
    }

    public void OnConfigParsed(BaseConfigs config)
    {
        Config = config;
        InitializeServices();

        Server.NextFrame(() =>
        {
            ServerHelper.UpdateCachedServerAddress();
        });

        StartAnnouncementTimer();
        StartCleanupTimer();
        _ = InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            if (_databaseService != null)
            {
                await _databaseService.InitializeDatabase();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[NeedSystem-Announcer] Database initialized successfully!");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[NeedSystem-Announcer] Failed to initialize database: {ex.Message}");
            Console.ResetColor();
        }
    }

    private void InitializeServices()
    {
        _databaseService = new DatabaseService(Config.Database, Config.Announcement.EnableDebug);
        _announcementService = new AnnouncementService(_databaseService, Config.Announcement, this);
    }

    private void StartAnnouncementTimer()
    {
        if (_checkTimer != null)
        {
            _checkTimer.Kill();
        }

        _checkTimer = AddTimer(Config.Announcement.CheckIntervalSeconds, () =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_announcementService != null && _databaseService != null)
                    {
                        await _announcementService.CheckAndAnnounceNewNotifications();
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[NeedSystem-Announcer] Timer error: {ex.Message}");
                    Console.ResetColor();
                }
            });
        }, TimerFlags.REPEAT);
    }

    private void StartCleanupTimer()
    {
        if (_cleanupTimer != null)
        {
            _cleanupTimer.Kill();
        }

        _cleanupTimer = AddTimer(86400.0f, () =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_databaseService != null)
                    {
                        await _databaseService.CleanOldProcessedRecords(Config.Announcement.CleanOldRecordsDays);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[NeedSystem-Announcer] Cleanup timer error: {ex.Message}");
                    Console.ResetColor();
                }
            });
        }, TimerFlags.REPEAT);
    }

    public override void Unload(bool hotReload)
    {
        if (_checkTimer != null)
        {
            _checkTimer.Kill();
        }

        if (_cleanupTimer != null)
        {
            _cleanupTimer.Kill();
        }
    }
}