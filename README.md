
# API

## Inscryption API

This plugin is a BepInEx plugin made for Inscryption as an API. This is the de-facto standard API for Inscryption modders.

It can currently create and modify:
- Cards
- Abilities
- Appearance behaviours
- Stat Icons
- Challenges
- Starter Decks
- Regions
- Encounters
- Totem Tops
- Consumable Items
- Gramophone Tracks
- Talking Cards
- And much more!

Additionally, a number of quality-of-life patches from the community are included with each release.

## Installation (automated)
This is the recommended way to install the API on the game.

- Download and install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager) or [r2modman](https://timberborn.thunderstore.io/package/ebkr/r2modman/)
- Click Install with Mod Manager button on top of the [page](https://inscryption.thunderstore.io/package/API_dev/API/)
- Run the game via the mod manager

## Installation (manual)
To install this plugin first you need to install BepInEx as a mod loader for Inscryption. A guide to do this can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html#where-to-download-bepinex). Inscryption needs the 86x (32 bit) mono version.

To install Inscryption API you simply need to copy **InscryptionAPI.dll** from [releases](https://github.com/ScottWilson0903/InscryptionAPI/releases) to **Inscryption/BepInEx/plugins**.

An example Mod utilising this plugin can be found [here](https://github.com/ScottWilson0903/InscryptionExampleMod).

## Modded Save File
With this API installed, an additional 'modded save file' will be created by the game. This file will be found in the 'BepInEx' subdirectory, and contains all save data created by mods that use this API. This file will not automatically be synced to the cloud by Steam.

# Community Patches

The following patches from the Inscryption modding community have been included in this package to improve the overall quality-of-life for modding and compatibility.

## SigilArtPatch by MADH95Mods
Fixes the art for abilities that previously only appeared in Act 2 so they appear correctly in Act 1 and Act 3

## Conduit Attack Fix by MADH95Mods
Fixes the behavior of conduits so they function correctly in Act 1.

## Activated Sigil Fix by MADH95Mods
Allows activated sigils to work correctly in Act 1 and Act 3 by clicking the sigil icon on the card.

## AnthonysLatchFix by AnthonyPython
Fixes latch sigils to work in Act 1

## Sigil Art Fix by Memez4Life
Allows up to 8 sigils to be displayed on Act 1 cards and adds the option to display merged sigil stamps at the bottom of the card instead of over the artwork (see the config files for more information).

## Visually Stackable Sigils by divisionbyz0rro
Combines multiple instances of the same sigil on a single card into a single sigil with a number to free up space on the card.

## Cost Render Fix by Void Slime
Displays hybrid cost cards correctly and makes energy and mox show up on act 1 cards

## Cost Choice Node fix by Void Slime
If energy/mox cards are in the Act 1 pool, energy and mox card choice nodes will be added to the cost choice node in Act 1.

## Sniper Sigil Fix by SpecialAPI
Displays targets for attacks made with the sniper sigil in Act 1.

## Act 1 Sentry Fixes by WhistleWind
Fixes a number of bugs caused by the Sentry ability being used in Act 1.

# Using the API

Inscryption API 2.0 tries to have you use the original game's objects as much as possible. For example, there are no more 'NewCard' and 'CustomCard' objects; instead, you are responsible to create CardInfo objects yourself and add them.
The API does provide a number of helper methods to make this process simpler for you.

For more information, please check out the wiki: https://inscryptionmodding.github.io/InscryptionAPI/wiki/

## Contribution

### How can you help?
Use the plugin and report bugs you find! Ping us on the [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm) server in the api channel with what you find.

### But really, I want to help develop this mod
Great! I'm more than happy to accept help. Either make a pull request or come join us over in the [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm).

### Can I donate?
Donations are totally not needed, this is a passion project before anything else.

## Contributors
Original version by cyantist

Contributors and builders of API 2.0
- divisionbyz0rro
- Eri
- IngoH
- JamesVeug
- julian-perge
- KellyBetty
- SpecialAPI
- Void Slime
- WhistleWind
- Windows10CE
