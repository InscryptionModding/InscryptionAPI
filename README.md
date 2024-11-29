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
- Slot Modifications
- Talking Cards
- Custom Costs
- Custom Rulebook Pages
- Rulebook Redirects/Hyperlinks
- And much more!

Additionally, a number of quality-of-life patches from the community are included with each release.

### Installing with a Mod Manager
1. Download and install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager), [Gale](https://thunderstore.io/c/inscryption/p/Kesomannen/GaleModManager/) or [r2modman](https://thunderstore.io/c/inscryption/p/ebkr/r2modman/).
2. Click the **Install with Mod Manager** button on the top of [BepInEx's](https://thunderstore.io/package/download/BepInEx/BepInExPack_Inscryption/5.4.1902/) page.
3. Run the game via the mod manager.

If you have issues with ModmManagers head to one of these discords;

* **Thunderstore/R2ModMan Support Discord:** [Here](https://discord.gg/Fbz54kQAxg)
* **Gale Mod Manager Support Discord:** [Here](https://discord.gg/sfuWXRfeTt)

### Installing Manually
1. Install [BepInEx](https://thunderstore.io/package/download/BepInEx/BepInExPack_Inscryption/5.4.1902/) by pressing 'Manual Download' and extract the contents into a folder. **Do not extract into the game folder!**
2. Move the contents of the 'BepInExPack_Inscryption' folder into the game folder (where the game executable is).
3. Run the game. If everything was done correctly, you will see the BepInEx console appear on your desktop. Close the game after it finishes loading.
4. Install [MonoModLoader](https://inscryption.thunderstore.io/package/BepInEx/MonoMod_Loader_Inscryption/) and extract the contents into a folder.
5. Move the contents of the 'patchers' folder into 'BepInEx/patchers' (If any of the mentioned BepInEx folders don't exist, just create them).
6. Install [Inscryption API](https://inscryption.thunderstore.io/package/API_dev/API/) and extract the contents into a folder.
7. Move the contents of the 'plugins' folder into 'BepInEx/plugins' and the contents of the 'monomod' folder into the 'BepInEx/monomod' folder.
8. Run the game again. If everything runs correctly, a message will appear in the console telling you that the API was loaded.

### Installing on the Steam Deck
1. Download [r2modman](https://thunderstore.io/c/inscryption/p/ebkr/r2modman/) on the Steam Deck’s Desktop Mode and open it from its download using its `AppImage` file.
2. Download the mods you plan on using and their dependencies..
3. Go to the setting of the profile you are using for the mods and click `Browse Profile Folder`.
4. Copy the BepInEx folder, then go to Steam and open Inscryption's Properties menu
5. Go to `Installed Files` click `Browse` to open the folder containing Inscryption's local files; paste the BepInEx folder there.
6. Enter Gaming Mode and check 'Force the use of a specific Steam Play compatibility tool' in the Properties menu under `Compatibility`.
7. Go to the launch parameters and enter `WINEDLLOVERRIDES=“winhttp.dll=n,b” %command%`.
8. Open Inscryption. If everything was done correctly, you should see a console appear on your screen.

### Mac & Linux
1. Follow the steps here first: <https://docs.bepinex.dev/articles/user_guide/installation/index.html>
2. Next do steps 4-8 of the Manual Installation
3. Your game should be setup for inscryption modding now

If you have any issues with Mac/Linux, Steam Deck, or Manual head over to the discord for this game:

* **Inscryption Modding Discord:** [Here](https://discord.gg/ZQPvfKEpwM)

An example mod utilising this plugin can be found [here](https://github.com/debugman18/InscryptionExampleMod),
and the modding wiki and documentation can be found [here](https://inscryptionmodding.github.io/InscryptionAPI/wiki/index.html).

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
Allows up to 8 sigils to be displayed on cards and adds the option to display merged sigil stamps at the bottom of the card instead of over the artwork (see the config files for more information).

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

## Multi-Act Sigil Compatibility Fixes by WhistleWind
Fixes a number of sigils to be usable in all Acts. Sigils include: Mental Gemnastics, Tidal Lock, Hoarder, Vessel Printer, Amorphous, Handy.

## Fledgling Sigil Fixes by WhistleWind
Fixes Fledgling in Act 2 to show the correct number of turns until a card evolves, up to 3. Also changes its description to show the correct number of turns.

## OverridePixelAbilityIcons by WhistleWind
Fixes the OverrideAbilityIcon method to work in Act 2.

## Temporary Mod Fixes by WhistleWind
- Fixes issues related to using temporary mods in Act 2 or to add custom decals

# Using the API

Inscryption API 2.0 tries to have you use the original game's objects as much as possible. For example, there are no more 'NewCard' and 'CustomCard' objects; instead, you are responsible to create CardInfo objects yourself and add them.

The API does provide a number of helper methods to make this process simpler for you.

For more information, please check out the wiki: [Here](https://inscryptionmodding.github.io/InscryptionAPI/wiki/), and if you need any help with anything related to the API send a message over in [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm).

## Contribution

### How can you help?
Use the plugin and report bugs you find! Ping us on the [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm) server in the api channel with what you find.

### But really, I want to help develop this mod!
Great! We're more than happy to accept help. Either make a pull request to the API's [GitHub page](https://github.com/InscryptionModding/InscryptionAPI) or come join us over in the [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm).

### Can I donate?
Donations are totally not needed, this is a passion project before anything else.

## Contributors
Original version by cyantist

Contributors and builders of API 2.0:
- BobbyShmurner
- divisionbyz0rro
- Eri
- IngoH
- JamesVeug
- julian-perge
- KellyBetty
- Nevernamed
- SpecialAPI
- Void Slime
- WhistleWind
- Windows10CE
- Keks307
- ThinCreator3483