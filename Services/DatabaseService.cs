using MySqlConnector;
using NeedSystemAnnouncer.Configs;
using NeedSystemAnnouncer.Models;

namespace NeedSystemAnnouncer.Services;

public class DatabaseService
{
    private readonly DatabaseConfig _config;
    private readonly bool _verboseLogging;
    private DateTime _lastConnectionCheck = DateTime.MinValue;
    private readonly TimeSpan _connectionCheckInterval = TimeSpan.FromMinutes(5);

    public DatabaseService(DatabaseConfig config, bool verboseLogging = false)
    {
        _config = config;
        _verboseLogging = verboseLogging;
    }

    public async Task InitializeDatabase()
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            await CreateProcessedNotificationsTable(connection);
            LogInfo("Database connection established and need_processed table created");
        }
        catch (Exception ex)
        {
            LogError($"Failed to initialize database: {ex.Message}");
            throw;
        }
    }

    private async Task CreateProcessedNotificationsTable(MySqlConnection connection)
    {
        var createTableQuery = @"
            CREATE TABLE IF NOT EXISTS need_processed (
                id INT AUTO_INCREMENT PRIMARY KEY,
                notification_uuid VARCHAR(36) NOT NULL,
                processed_by_server VARCHAR(64) NOT NULL,
                processed_at DATETIME NOT NULL,
                UNIQUE KEY unique_notification_server (notification_uuid, processed_by_server),
                INDEX idx_notification_uuid (notification_uuid),
                INDEX idx_processed_by (processed_by_server),
                INDEX idx_processed_at (processed_at)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

        using var cmd = new MySqlCommand(createTableQuery, connection);
        await cmd.ExecuteNonQueryAsync();

        if (_verboseLogging)
            LogInfo("Processed notifications table created/verified");
    }

    public async Task<bool> CheckDatabaseConnection()
    {
        if (DateTime.Now - _lastConnectionCheck < _connectionCheckInterval)
            return true;

        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            await connection.PingAsync();
            _lastConnectionCheck = DateTime.Now;

            if (_verboseLogging)
                LogInfo("Database connection healthy");

            return true;
        }
        catch (Exception ex)
        {
            LogError($"Database connection check failed: {ex.Message}");
            return false;
        }
    }

    public async Task<NotificationRecord?> GetLatestUnprocessedNotification(string currentServerAddress, bool announceOwnServer)
    {
        try
        {
            if (!await CheckDatabaseConnection())
                return null;

            using var connection = GetConnection();
            await connection.OpenAsync();

            var whereClause = announceOwnServer ? "" : "AND n.server_address != @currentServer";

            var query = $@"
                SELECT n.uuid, n.server_address, n.connected_players, n.max_players, 
                       n.map_name, n.timestamp, n.requested_by
                FROM need_notifications n
                LEFT JOIN need_processed p 
                    ON n.uuid = p.notification_uuid 
                    AND p.processed_by_server = @currentServer
                WHERE p.id IS NULL 
                    {whereClause}
                ORDER BY n.timestamp DESC
                LIMIT 1";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@currentServer", currentServerAddress);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var notification = new NotificationRecord
                {
                    Uuid = reader.GetString("uuid"),
                    ServerAddress = reader.GetString("server_address"),
                    ConnectedPlayers = reader.GetInt32("connected_players"),
                    MaxPlayers = reader.GetInt32("max_players"),
                    MapName = reader.GetString("map_name"),
                    Timestamp = reader.GetDateTime("timestamp"),
                    RequestedBy = reader.GetString("requested_by")
                };

                if (_verboseLogging)
                    LogInfo($"Found unprocessed notification: {notification.Uuid} from {notification.ServerAddress}");

                return notification;
            }

            return null;
        }
        catch (Exception ex)
        {
            LogError($"Error getting latest unprocessed notification: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> MarkNotificationAsProcessed(string notificationUuid, string serverAddress)
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var insertQuery = @"
                INSERT INTO need_processed 
                (notification_uuid, processed_by_server, processed_at)
                VALUES (@uuid, @server, @processedAt)
                ON DUPLICATE KEY UPDATE processed_at = @processedAt";

            using var cmd = new MySqlCommand(insertQuery, connection);
            cmd.Parameters.AddWithValue("@uuid", notificationUuid);
            cmd.Parameters.AddWithValue("@server", serverAddress);
            cmd.Parameters.AddWithValue("@processedAt", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();

            if (_verboseLogging)
                LogInfo($"Notification {notificationUuid} marked as processed by {serverAddress}");

            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error marking notification as processed: {ex.Message}");
            return false;
        }
    }

    public async Task<int> CleanOldProcessedRecords(int daysToKeep)
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var deleteQuery = @"
                DELETE FROM need_processed 
                WHERE processed_at < DATE_SUB(NOW(), INTERVAL @days DAY)";

            using var cmd = new MySqlCommand(deleteQuery, connection);
            cmd.Parameters.AddWithValue("@days", daysToKeep);

            int deletedRows = await cmd.ExecuteNonQueryAsync();

            if (deletedRows > 0)
                LogInfo($"Cleaned {deletedRows} old processed records (older than {daysToKeep} days)");

            return deletedRows;
        }
        catch (Exception ex)
        {
            LogError($"Error cleaning old processed records: {ex.Message}");
            return 0;
        }
    }

    private MySqlConnection GetConnection()
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = _config.Host,
            Port = _config.Port,
            UserID = _config.User,
            Database = _config.DatabaseName,
            Password = _config.Password,
            Pooling = true,
            SslMode = MySqlSslMode.Preferred,
            ConnectionTimeout = (uint)_config.ConnectionTimeout,
            MinimumPoolSize = 0,
            MaximumPoolSize = 10
        };

        return new MySqlConnection(builder.ConnectionString);
    }

    private void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[NeedSystem-Announcer Database] {message}");
        Console.ResetColor();
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[NeedSystem-Announcer Database] {message}");
        Console.ResetColor();
    }
}