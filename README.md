# Siege Profile Manager

A native Windows app (C# / Avalonia) that works together with a Logitech G HUB Lua script
(`siege_profile_manager.lua`) for Rainbow Six Siege **training-range use**.

No AutoHotkey, no pasting configs: the app writes a live config file that the Lua script loads by itself.

## What it does
- **Operators page:** pick an operator, weapons and attachments; saved loadouts and favourites.
- **Live HUD:** current operator, slot (keys 1/2), attachments, link status.
- **Coach:** learns recoil from screen measurements while you spray, always on, remembers per loadout.
- **Updates:** Settings → *Check for updates* downloads the newest exe from `app/dist`.

## Hotkeys
| Key | Action |
|---|---|
| F6 | copy diagnostics + coach log to the clipboard |
| F7 | show / hide the window |
| F9 | show / hide the HUD |
| F12 | pause / resume coach learning |

## Install
Download `app/dist/SiegeProfileManager.exe`, put it somewhere writable (for example `%LOCALAPPDATA%\SiegeProfileManager`) and run it.
Paste `siege_profile_manager.lua` into your G HUB script once; after that the app updates it live.

## Build
```
dotnet publish app/App/SPM.App.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```
Tests: `app/Tests` (`dotnet run -c Release -- <copy of the lua>`).
