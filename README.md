# Terraria Manhunt Server

A modified dedicated server application for Terraria designed to automate the "Runner vs. Hunters" game mode. This implementation functions entirely server-side; players can join using standard, unmodified Terraria clients.

## Features

### Game State Automation

* **Lobby Phase:** Upon joining, players are permanently inflicted with the Cursed debuff to prevent pre-game item usage. World time is frozen at 8:15 AM & NPC spawns are disabled until the match begins.
* **Runner Selection:** Any player can initiate the match by using Ocram's Razor (Mechdusa Summon) while not on team yellow. This designates the Runner, locks everyone's teams, allows NPC spawns for the Runner, and begins the countdown.
* **Countdown:** Hunters are inflicted with the Webbed (frozen) debuff during the countdown; duration scales based on the number of active Hunters.
* **Match Start:** Once the countdown concludes, all movement restrictions are lifted, PvP is strictly enforced, and NPC spawns are re-enabled for the Hunters.

### Mechanics

* **Auto-Team Assignment:** Hunters are automatically forced to Team 4 (Yellow). The Runner may select any team.
* **Auto-PvP:** PvP is automatically enabled and locked for all participants once the match begins.
* **Day 3 Alert:** A global broadcast is sent when the 3rd in-game day begins.
* **Steam Integration:** Supports Steam's native "Host & Play" architecture, including background window hiding for seamless hosting.

## Admin Commands

The server includes custom console commands for game management.

| Command | Arguments | Description |
| --- | --- | --- |
| `i` | `[player/@a] [item_id] [amt] [prefix]` | Grants items to a specific player or all players (`@a`). |
| `delay` | `[seconds]` | Sets the countdown delay multiplier (seconds per Hunter). |
| `time` | `[hh:mm am/pm]` | Sets the world time. |
| `kick` | `[player/@a] [reason]` | Disconnects a player from the server. |
| `ban` | `[player/@a] [reason]` | Bans a player from the server. |

## Usage

### Method 1: Dedicated Server (Public IP)

Use this method for standard dedicated hosting where players connect via your public IP address.

1. Navigate to the installation directory.
2. Execute **`startserver.bat`**.
3. Follow the console prompts to select the world and port (Default: 7777).

### Method 2: Steam Hosting

Use this method to host via Steam (similar to "Host & Play").

1. Ensure Steam is running.
2. Run **`start-server-steam-friends.bat`**.
3. Select the world and settings.
4. Launch your Terraria client and connect via `127.0.0.1` (localhost).
5. Friends can join via the Steam Friends list ("Join Game").

*Note: For public hosting, ensure port 7777 (TCP/UDP) is forwarded on your router.*
