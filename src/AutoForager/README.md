**Auto Forager** (previously AutoShaker) is an open-source mod for [Stardew Valley](https://stardewvalley.net) that allows players to automatically forage items simply by moving near them.

![](https://i.imgur.com/beGLdhy.gif)

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
- If you are a mod maker working on custom Forageable items, to have your item recognized by the AutoForager all you need to do is add the context tag `forage_item` to the item definition
  - Alternatively, you can create a [content pack](./docs/ContentPack.md) that can also help to categorize your custom forageables / trees

### Translation
&nbsp;     | No Translation  | Partial Translation  | Full Translation  | Translated By
:--------: | :-------------: | :------------------: | :---------------: | :------------:
Chinese    | ✔              | ✔                    | ❌                | [Krobus](https://www.nexusmods.com/users/127351118), [Qianguang](https://www.nexusmods.com/users/196008176)
French     | ✔              | ✔                    | ❌                 | [WelshieFrenchie](https://github.com/WelshieFrenchie)
German     | ✔              | ❌                   | ❌                | n/a
Hungarian  | ✔              | ❌                   | ❌                | n/a
Italian    | ✔              | ❌                   | ❌                | n/a
Japanese   | ✔              | ❌                   | ❌                | n/a
Korean     | ✔              | ❌                   | ❌                | n/a
Polish     | ✔              | ❌                   | ❌                | n/a
Portuguese | ✔              | ✔                    | ❌                | [NARCOAZAZAL](https://www.nexusmods.com/users/200703680)
Russian    | ✔              | ✔                   | ❌                | [Ognejar](https://github.com/Ognejar)
Spanish    | ✔              | ✔                    | ❌                | [ElviraCroft](https://github.com/ElviraCroft)
Thai       | ✔              | ❌                   | ❌                | n/a
Turkish    | ✔              | ❌                   | ❌                | n/a
Ukrainian  | ✔              | ❌                   | ❌                | n/a

### Install
1. Install the latest version of [SMAPI](https://smapi.io)
  1. [Nexus Mirror](https://www.nexusmods.com/stardewvalley/mods/2400)
  2. [CurseForge Mirror](https://www.curseforge.com/stardewvalley/utility/smapi)
  3. [GitHub Mirror](https://github.com/Pathoschild/SMAPI/releases)
2. *Optional but recommended* Install the latest version of [Generic Mod Config Menu](https://spacechase0.com/mods/stardew-valley/generic-mod-config-menu/)
  1. [Nexus Mirror](https://www.nexusmods.com/stardewvalley/mods/5098)
  2. [CurseForge Mirror](https://www.curseforge.com/stardewvalley/mods/generic-mod-config-menu)
3. Install this mod by unzipping the mod folder into 'Stardew Valley/Mods'
4. Launch the game using SMAPI

### Compatibility
- Compatible with...
  - Stardew Valley 1.6 or later
  - SMAPI 4.0.0 or later
- Automatic Integrations
  - [Bush Bloom Mod](https://www.nexusmods.com/stardewvalley/mods/15792)
    - Minimum version: **1.1.9**
  - [Custom Bush](https://www.nexusmods.com/stardewvalley/mods/20619)
    - Minimum version: **1.0.4**
  - [Farm Type Manager](https://www.nexusmods.com/stardewvalley/mods/3231)
    - Minimum version: **1.20.0**
- No known mod conflicts
  - If you find one, please feel free to notify me here on Github, on the [Nexus Mod](https://www.nexusmods.com/stardewvalley/mods/7736) site, or on the [CurseForge](https://www.curseforge.com/stardewvalley/mods/auto-forager) site.

## Limitations
### Solo + Multiplayer
- This mod is player specific, each player that wants to utilize it must have it installed

## Releases
Releases can be found on [GitHub](https://github.com/Hedgehog-Technologies/StardewMods/releases), on the [Nexus Mod](https://www.nexusmods.com/stardewvalley/mods/7736) site, and on the [CurseForge](https://www.curseforge.com/stardewvalley/mods/auto-forager) site.
### Release Notes
You can find the release notes [here](./docs/release-notes.md)
