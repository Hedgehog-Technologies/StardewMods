# Release Notes

## 3.9.0
Released TBD
- Made Auto Forager enabled state persistent across game sessions
- Fix inconsistent behavior with ignoring trees near mushroom logs
- Give farming xp for farm animal products
- Improve config state between game sessions for dynamically loaded content
- Improve messaging when around pre-launch parsing fails for some items
## 3.8.0
Released 2025-04-16
- Add ability to forage panning spots
- Fix issue caused by generated forageables that would lead to infinitely foraging some items
- Revert previous change to clamp forageable radius
  - It became clear that some power users prefer to have a much larger radius and their computers are able to handle it, so I have reverted this change and instead recommend anyone having issues to disable "Use Player Magnetism" and set a value in Shake Distance that works for your circumstances
## 3.7.2
Released 2025-04-16
- Force clamp forageable radius to at least 2 tiles and at most 10 tiles
- Update Chinese translations
  - Thanks to [Qianguang](https://www.nexusmods.com/users/196008176) for the contribution
## 3.7.1
Released 2025-02-12
- Add Russian translations
  - Thanks to [Ognejar](https://github.com/Ognejar) for the contribution
## 3.7.0
Released 2024-10-25
- Add ability to auto-forage the following minerals
  - Quartz, Earth Crystal, Frozen Tear, Fire Quartz, and Dragon Tooth
## 3.6.2
Released 2024-06-30
- Adds French translations
  - Thanks to [WelshieFrenchie](https://github.com/WelshieFrenchie) for the contribution
## 3.6.1
Released 2024-06-12
- Add config option to elevate some messages to SMAPI console
  - This option is disabled by default
## 3.6.0
Released 2024-05-20
- Cover a missing path that was preventing forageables added directly to Data/Locations via Content Patcher from being Auto Foraged
- Remove Supply Crates and Treasure Chests from being considered potential forageables
- Fix typo in Orange's category
## 3.5.2
Released 2024-05-19
- Fix tappers being delayed by a day when auto foraged from
## 3.5.1
Released 2024-05-18
- Fix picking up artifact and seed spots
## 3.5.0
Released 2024-05-17
- Add automatic integration with [Farm Type Manager](https://www.nexusmods.com/stardewvalley/mods/3231)
  - Content packs should no longer be needed going forward but will continue to work
  - Category names will likely be different as they are now pulled from the FTM content pack instead of the AF content packs
  - Minimum version: **1.20.0**
  - Shoutout to [Esca-MMC](https://github.com/Esca-MMC) for supplying the API
- Add options to forage Mushroom Boxes, Mushroom Logs, and Tappers
- Add Animal products as a category for vanilla animal products
- Add vanilla Fruit forageable toggles to help with the Fruit Bat cave option
## 3.4.3
Released 2024-05-07
- Add additional safeguards around players clicking through title menu quickly
## 3.4.2
Released 2024-05-04
- Fix items incorrectly getting increased quality where they normally wouldn't
## 3.4.1
Released 2024-04-27
- Prevent content packs from overriding known categories of vanilla forageables
- Lower the amount of warning log spew when multiple content packs add the same native item as a forageable
## 3.4.0
Released 2024-04-26
- Add ability to ignore moss-covered trees that are near enough to contribute to a Mushroom Log
- Fix optional tool requirement for harvesting moss to be less intrusive
- Lower the amount some logs were spamming the SMAPI console
## 3.3.0
Released 2024-04-23
- Wild Tree shake items are now supported
- Adds Spanish translations
  - Thanks to [ElviraCroft](https://github.com/ElviraCroft) for the contribution
## 3.2.2
Released 2024-04-17
- Fix exception on launch due to multiple Title Screen init events running simultaneously
- Update config strings to be a bit more explicit what they are looking for
- Add some trace logging for config values
## 3.2.1
Released 2024-04-16
- Fix crash on launch
- Move mod initialization to final init heartbeat to ensure everything is loaded
## 3.2.0
Released 2024-04-16
- Buried forageables are now handled via `Artifact Spot` and `Seed Spot` toggles instead of individually
- Final initialization heartbeat now waits for Title Menu to be interactable
- All forageable toggles are initialized to enabled
  - This is a mitigation until I can find time to figure out a 'Select / Deselect All' config button
- Fixed issue where some objects weren't properly seen as forageable
- Fixed exception thrown by utilizing tools when foraging moss and buried forageables
- Fixed toggling forager with keybinds sometimes not respecting "Is Forager Active?" config setting
  - Config setting was removed to prevent doubling up on sources of truth, forager will always be active on game start
- Add content packs for Wild Flowers and Kombucha of Ferngill
## 3.1.0
Released 2024-04-14
- Added field for content packs to ignore items that may not actually be forageable
- Added content pack for Atelier Wildflour Crops and Forage
## 3.0.3
Released 2024-04-10
- Prevent crash when integrated mods aren't ready within the timeout window
- Bump up wait time for integrated mods
## 3.0.2
Released 2024-04-06
- Bump Integration wait timer from 5s -> 30s to account for larger installed mod counts
## 3.0.1
Released 2024-04-06
- Fix Spring Onions and Ginger not being foraged
## 3.0.0
Released 2024-04-06
- Added [content pack format](./docs/ContentPack.md) to allow for easier extensibility for various content mods
  - Content Packs for 2.2.3 compatibility parity provided as optional files
    - Cornucopia
      - More Crops
      - More Flowers
    - Forage of Ferngill
      - Coastal Forage of Ferngill
      - Fruits and Nuts of Ferngill
      - Mushrooms of Ferngill
      - Roots of Ferngill
    - Lumisteria
      - Serene Meadow
      - Visit Mount Vapius
    - Ridgeside Village
    - Stardew Valley Expanded
- Added automatic integrations with the following mods
  - [Bush Bloom Mod](https://www.nexusmods.com/stardewvalley/mods/15792) - minimum version: 1.1.9
  - [Custom Bush](https://www.nexusmods.com/stardewvalley/mods/20619) - minimum version: 1.0.4
- Add Portuguese translations
  - Thanks to [NARCOAZAZAL](https://www.nexusmods.com/users/200703680) for the provided translation
- Fixed buried forageables not respecting config toggles until game was reopened
## 2.2.3
Released 2024-03-28
- Fix some translations not getting updated on locale change
- Added Chinese translations
  - Thanks to [Krobus](https://www.nexusmods.com/users/127351118) for the provided translation
## 2.2.2
Released 2024-03-27
- Additional error checks and fallbacks when parsing Fruit Trees
## 2.2.1
Released 2024-03-26
- Fixed errors when parsing trees
- Prevent possible future errors from completely halting mod functionality
## 2.2.0
Released 2024-03-26
- Added compatibility with Stardew Valley Expanded forageable items
- Added "Moss" as a forageable option
  - Added option to toggle off requirement of having a tool in inventory for Auto Forager to forage for Moss
- Added partial compatibility for the Cornucopia mod
  - Additional work is needed to added compatibility with the "Custom Bush" mod
- Custom Wild and Fruit trees are now properly recognized by the Auto Forager
- Fixed a potential crash when running alongside the "Marry Morris" mod
## 2.1.0
Released 2024-03-21
- Added ability to forage for truffles found by pigs
- Wild Trees added in 1.6 are now shaken as expected
- Config settings no longer reset on game launch when content patch mods are present
  - Special shoutout to DromedarySpitz, babayagah07, and galedekarios for reporting and helping me investigate this issue
## 2.0.0
Released 2024-03-19
- Rebranded to **AutoForager**
- Extended functionality to include options to forage seasonal items
- Update to SDV 1.6 compatibility
- Update to SMAPI 4.0.0 compatibiliy
## 1.6.1
Released 2024-02-22
- End support for SDV 1.5.6
## 1.6.0
Released Unknown
- Moved to new repository
- Updated to use Khloe Leclair's [Mod Manifest Builder](https://github.com/KhloeLeclair/Stardew-ModManifestBuilder)
## 1.5.2
Released 2023-06-13
- Fix tea bushes from constantly shaking
## 1.5.1
Released Unknown
- Add Chinese localization
    - Translation by: liky123131231 (NexusMods)
## 1.5.0
Released 2022-07-11
- Simplify calculations per game tick
- Add translation language support
- Back-end versioning updates
- Thanks to @atravita-mods for this update
## 1.4.0
Released 2022-05-16
- Added the ability to shake Tea Bushes for their Tea Leaves
## 1.3.2
Released 2021-01-29
- Fixes a NullReferenceException thrown when a second user is joining a split-screen instance
- Updated the way the End-Of-Day messages are built
- Minor backend changes
## 1.3.1
Released 2021-01-27
- Fix for not shaking bushes when current language isn't set to English
- Updated default ShakeDistance from 1 to 2
- Minor backend changes
## 1.3.0
Released 2021-01-25
- Added the ability to specify the number of fruits (1-3) available on Fruit Tree before attempting to auto-shake it
- Minor backend changes
## 1.2.0
Released 2021-01-24
- Swapped config to have separate toggles for regular and fruit trees
- Added a check to ensure a user isn't in a menu when the button(s) for toggling the autoshaker are pressed
- Added some additional "early outs" when checking whether or not a tree or bush should be shaken
## 1.1.0
Released 2021-01-22
- Upgrading MinimumApiVerison to SMAPI 3.9.0
- Swap from old single SButton to new KeybindList for ToggleShaker keybind
    - Anyone who has a config.json file will no longer have to press an alt button to toggle the AutoShaker (unless they change their config.json file manually OR delete it and let it get regenerated the next time they launch Stardew Valley via SMAPI)
## 1.0.0
Released 2021-01-19
- Initial release
- Allows players to automatically shake trees and bushes by moving nearby to them
- Working as of Stardew Valley 1.5.3