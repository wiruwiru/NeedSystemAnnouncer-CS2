# NeedSystem-Announcer CS2

> [!WARNING]
> This project is currently under active development and in a very early alpha stage. It is still being tested and may contain bugs or unexpected behavior. Use at your own risk, or wait for a more stable version with a more developed implementation.

NeedSystem-Announcer works alongside the main NeedSystem plugin to create a multi-server notification system. When a player uses the `!need` command on one server, all other servers connected to the same database will display an in-game announcement.

## How It Works
1. A player uses `!need` on Server A (via NeedSystem plugin)
2. The notification is stored in the database
3. NeedSystem-Announcer on Server B, C, etc. periodically checks for new notifications
4. Each server announces the notification to its players once
5. The notification is marked as processed to prevent duplicates

## Requirements
- [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) (Minimum API Version: 342)
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
- MySQL/MariaDB database (shared with NeedSystem plugin)
- [NeedSystem](https://github.com/wiruwiru/NeedSystem-CS2/releases) plugin installed on at least one server

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

2. Download [NeedSystem-Announcer.zip](https://github.com/wiruwiru/NeedSystem-Announcer/releases) from the releases section.

3. Unzip the archive and upload it to the game server

4. Start the server and wait for the config.json file to be generated.

5. Complete the configuration file with the parameters of your choice.

## Configuration

### Announcement Settings
| Parameter | Description | Default | Required |
| :------- | :------- | :------- | :------- |
| `CheckIntervalSeconds` | How often (in seconds) the plugin checks for new notifications | `30.0` | **YES** |
| `AnnounceOwnServer` | Whether to announce notifications from this server | `false` | **YES** |

### Server Settings
| Parameter | Description | Default | Required |
| :------- | :------- | :------- | :------- |
| `GetIPandPORTautomatic` | Automatically detect server IP:PORT | `true` | **YES** |
| `IPandPORT` | Manual server IP:PORT (used if automatic detection fails) | `""` | **NO*** |

**Required only if `GetIPandPORTautomatic` is set to `false` or automatic detection fails*

### Database Settings
| Parameter | Description | Default | Required |
| :------- | :------- | :------- | :------- |
| `Host` | MySQL database host address | `"localhost"` | **YES** |
| `Port` | MySQL database port | `3306` | **YES** |
| `User` | MySQL database username | `""` | **YES** |
| `Password` | MySQL database password | `""` | **YES** |
| `DatabaseName` | Name of the database | `""` | **YES** |

> **Important:** The database must be the same one used by the NeedSystem plugin
> **Note:** The `need_notifications` table must already exist (created by NeedSystem plugin)

## Configuration Example
```json
{
  "AnnouncementSettings": {
    "CheckIntervalSeconds": 30.0,
    "AnnounceOwnServer": false
  },
  "ServerSettings": {
    "GetIPandPORTautomatic": true,
    "IPandPORT": ""
  },
  "Database": {
    "Host": "localhost",
    "Port": 3306,
    "User": "your_db_user",
    "Password": "your_db_password",
    "DatabaseName": "your_database"
  }
}
```

### Customizing Messages
You can edit the localization files to customize the announcement messages. The language used depends on your CounterStrikeSharp `core.json` settings.

#### Example (en.json):
```json
{
  "Prefix": "[{GOLD}NEED{BLUE}SYSTEM{WHITE}]",
  "notification.players_needed": "{Gold}[NEED]{Default} Players needed on {Green}{0}{Default} | Map: {Blue}{1}{Default} | Players: {Orange}{2}/{3}{Default}"
}
```

#### Placeholders:
- `{0}` - Server address (IP:PORT)
- `{1}` - Map name
- `{2}` - Connected players
- `{3}` - Max players