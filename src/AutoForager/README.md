﻿**AutoForager** (previously AutoShaker) is an open-source mod for [Stardew Valley](https://stardewvalley.net) that allows players to automatically forage items simply by moving near them.

## Documentation
### Overview
This mod checks for:
- Bushes that are currently blooming with berries or tea leaves
    - NOTE: This includes Golden Walnut bushes
- Fruit trees that currently have fruit on them
- Trees that have a seed available to be shaken down
    - NOTE: This includes trees with hazelnuts, coconuts, and golden coconuts
- Forageables throughout Stardew Valley

### Config
You can find a breakdown of the config values [here](./docs/config.md)

### Extensibility
- Custom Trees and Fruit Trees should automatically get picked up and recognized by the AutoForager
- If you are a mod maker working on custom Forageable items, to have your item regonized by the AutoForager all you need to do is add the context tag `forage_item` to the item definition

### Translation
&nbsp;     | No Translation  | Partial Translation  | Full Translation
:--------- | :-------------: | :------------------: | :---------------:
Chinese    | ✔              | ❌                   | ❌
French     | ✔              | ❌                   | ❌
German     | ✔              | ❌                   | ❌
Hungarian  | ✔              | ❌                   | ❌
Italian    | ✔              | ❌                   | ❌
Japanese   | ✔              | ❌                   | ❌
Korean     | ✔              | ❌                   | ❌
Polish     | ✔              | ❌                   | ❌
Portuguese | ✔              | ❌                   | ❌
Russian    | ✔              | ❌                   | ❌
Spanish    | ✔              | ❌                   | ❌
Thai       | ✔              | ❌                   | ❌
Turkish    | ✔              | ❌                   | ❌
Ukrainian  | ✔              | ❌                   | ❌

### Install
1. Install the latest version of [SMAPI](https://smapi.io)
    1. [Nexus Mirror](https://www.nexusmods.com/stardewvalley/mods/2400)
    2. [GitHub Mirror](https://github.com/Pathoschild/SMAPI/releases)
2. *OPTIONAL* Install the latest version of [Generic Mod Config Menu](https://spacechase0.com/mods/stardew-valley/generic-mod-config-menu/)
    1. [Nexus Mirror](https://www.nexusmods.com/stardewvalley/mods/5098)
3. Install this mod by unzipping the mod folder into 'Stardew Valley/Mods'
4. Launch the game using SMAPI

### Compatibility
- Compatible with...
    - Stardew Valley 1.6 or later
    - SMAPI 4.0.0 or later
- No known mod conflicts
    - If you find one, please feel free to notify me here or on the [Nexus Mod](https://www.nexusmods.com/stardewvalley/mods/7736) site

## Limitations
### Solo + Multiplayer
- This mod is player specific, each player that wants to utilize it must have it installed

## Releases
Releases can be found on [GitHub](https://github.com/Hedgehog-Technologies/StardewMods/releases) and on the [Nexus Mod](https://www.nexusmods.com/stardewvalley/mods/7736) site
### 2.0.0
- Rebranded to **AutoForager**
- Extended functionality to include options to forage seasonal items
- Update to SDV 1.6 compatibility
- Update to SMAPI 4.0.0 compatibiliy
### 1.6.0
- Moved to new repository
- Updated to use Khloe Leclair's [Mod Manifest Builder](https://github.com/KhloeLeclair/Stardew-ModManifestBuilder)
### 1.5.2
- Fix tea bushes from constantly shaking
### 1.5.1
- Add Chinese localization
    - Translation by: liky123131231 (NexusMods)
### 1.5.0
- Simplify calculations per game tick
- Add translation language support
- Back-end versioning updates
- Thanks to @atravita-mods for this update
### 1.4.0
- Added the ability to shake Tea Bushes for their Tea Leaves
### 1.3.2
- Fixes a NullReferenceException thrown when a second user is joining a split-screen instance
- Updated the way the End-Of-Day messages are built
- Minor backend changes
### 1.3.1
- Fix for not shaking bushes when current language isn't set to English
- Updated default ShakeDistance from 1 to 2
- Minor backend changes
### 1.3.0
- Added the ability to specify the number of fruits (1-3) available on Fruit Tree before attempting to auto-shake it
- Minor backend changes
### 1.2.0
- Swapped config to have separate toggles for regular and fruit trees
- Added a check to ensure a user isn't in a menu when the button(s) for toggling the autoshaker are pressed
- Added some additional "early outs" when checking whether or not a tree or bush should be shaken
### 1.1.0
- Upgrading MinimumApiVerison to SMAPI 3.9.0
- Swap from old single SButton to new KeybindList for ToggleShaker keybind
    - Anyone who has a config.json file will no longer have to press an alt button to toggle the AutoShaker (unless they change their config.json file manually OR delete it and let it get regenerated the next time they launch Stardew Valley via SMAPI)
### 1.0.0
- Initial release
- Allows players to automatically shake trees and bushes by moving nearby to them
- Working as of Stardew Valley 1.5.3