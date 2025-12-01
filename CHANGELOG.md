# 1.1.7

### CHANGES

- Added back the config option to enable the tester overlay
- Changed lobby menu command fix to use alternate method
- Cleaned up player name prefix logic

# 1.1.6

### CHANGES

- Added `/lobbymenu` to return to the lobby menu

# 1.1.5

### CHANGES

- Added a config option for whether the vanilla debug console is useable
- Added a config option for whether the vanilla debug commands are useable
- Updated the README.md

# 1.1.4

### CHANGES

- Added some useful tips for the debug console to the README
- Made the splash screen "done" logic still run when the splash screen is automatically skipped

# 1.1.3

### CHANGES

- Support for REPO v0.3.0
  - Enabled the debug console which is accessible via the grave key (`)

# 1.1.2

### CHANGES

- Added config option for an "Improved Layout" for the main menu. This requires you to have [MenuLib](https://thunderstore.io/c/repo/p/nickklmao/MenuLib/) enabled. The main differences with it enabled are:
  - Region selection defaults to the last selected region and is accessible via a button (instead of automatically showing each time you try to play multiplayer)
  - Clicking "Host Game" on the main menu allows you to host a private/public save file
  - Clicking "Join Game" on the main menu takes you straight to the server list
  - Added a button to refresh the server list
- Disabled some commands in debug builds

# 1.1.1

### CHANGES

- Disabled tester overlay on the tester branch

# 1.1.0

### CHANGES

- Updated some links in the README.md
- Made the cache for name prefixes only include players in your current lobby instead of everyone

### FIXES

- Fixed chat commands using the closest level point to any player instead of the closest level point to the local player

# 1.0.9

### FIXES

- Fixed being unable to spawn enemy and surplus valuables in multiplayer

# 1.0.8

### CHANGES

- Creating a save file for a public lobby will now use the server name as the default save file name
- Loading a save file for a public lobby will now use the save file name as the default server name

### FIXES

- Fixed loading a save file in a public lobby using the previous save file instead of the selected one

# 1.0.7

### NEW

- Added steam rich presence grouping

### CHANGES

- Made public lobby save files enabled by default

### FIXES

- Improved stability of hosting and joining public lobbies (public lobby save logic now only runs when required)

# 1.0.6

### CHANGES

- Changed saves menu header for multiplayer to show whether it's private/public multiplayer
- Disabled text-to-speech for successful commands

### FIXES

- Fixed hosting a public lobby showing an infinite loading screen on some occasions

# 1.0.5

### NEW

- Added `/setname [string]` to rename the current save file
- Added more info to the README.md (such as stating which features are only used by the host)
- Added `/setscene random` which makes it do a random gameplay level

### CHANGES

- Changed `/level` to `/setscene` (to reduce confusion with `/setlevel`)
- Made `/setcash` automatically reload the scene if in the shop (or in a gameplay level if there is an extraction in-progress)
- Made `/setlevel` automatically reload the scene if in a gameplay level or the shop
- When hosting a public lobby using a save file it will no longer give the confirmation popup before the server name input popup (since the server name input prompt is basically a confirmation)

### FIXES

- Fixed cursor disappearing when exiting server name input popup
- Fixed `/valuable` not working in multiplayer
- Fixed pasting into chat not working on some occasions

# 1.0.4

### NEW

- Added config option to skip moon phase animation
- Added config option to skip splash screen
- Added config option to enable save files for public lobbies

### FIXES

- Fixed being able to open chat in the main menu
- Fixed singleplayer deaths taking you to the lobby menu (when it's not enabled in the config)

# 1.0.3

### NEW

- Added missing thing to v1.0.2 changelog

### FIXES

- Fixed pasting into chat bypassing the max chat message length

# 1.0.2

### NEW

- Added `/item [`[string](https://1a3.uk/games/repo/diffs/?tab=4&tabItems=0)`]`
- Added `/valuable [`[string](https://1a3.uk/games/repo/diffs/?tab=4&tabItems=1)`]`
- Added ability to paste into chat

### FIXES

- Fixed name prefixes sometimes not being able to be set after the changes in 1.0.1

# 1.0.1

### CHANGES

- Small tweaks to name prefixes

### FIXES

- Fixed chat commands not working
