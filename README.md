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

## Getting Started: Installation
---
To begin, we'll go over how to install BepInEx, the framework all Inscryption mods use.  This is a necessary step to playing modded Inscryption, so be sure to follow this carefully.

**FOR LINUX AND STEAMDECK:** Ensure that the game is set to run using `Proton` in he game settings on steam. Should be a setting like this: `Change launch behaviour` -> `Proton`.

### Installing with a Mod Manager
1. Download and install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager), [Gale](https://thunderstore.io/c/inscryption/p/Kesomannen/GaleModManager/) or [r2modman](https://thunderstore.io/c/inscryption/p/ebkr/r2modman/).
2. Click the **Install with Mod Manager** button on the top of [BepInEx's](https://thunderstore.io/c/inscryption/p/BepInEx/BepInExPack_Inscryption/) page.
3. Run the game via the mod manager.

If you have issues with Mod Managers head to one of these discords;

* **Thunderstore Support Discord:** [Here](https://discord.gg/Fbz54kQAxg)
* **R2ModMan Support Discord:** [Here](https://discord.gg/R85wjqa4WN)
* **Gale Mod Manager Support Discord:** [Here](https://discord.gg/sfuWXRfeTt)

### Installing Manually
1. Install [BepInEx](https://thunderstore.io/package/download/BepInEx/BepInExPack_Inscryption/5.4.2305/) by pressing `Manual Download` and extract the contents into a folder. **Do not extract into the game folder!**
2. Move the contents of the `BepInExPack_Inscryption` folder into the game folder (where the game executable is; usually found here: `C:\Program Files (x86)\Steam\steamapps\common\Inscryption`).
3. Run the game. If everything was done correctly, you will see the BepInEx console appear on your desktop. Close the game after it finishes loading.
4. Install [MonoModLoader](https://inscryption.thunderstore.io/package/BepInEx/MonoMod_Loader_Inscryption/) and extract the contents into a folder.
5. Move the contents of the `patchers` folder into `BepInEx/patchers` (If any of the mentioned BepInEx folders don't exist, just create them).
6. Install [Inscryption API](https://inscryption.thunderstore.io/package/API_dev/API/) and extract the contents into a folder.
7. Move the contents of the `plugins` folder into `BepInEx/plugins` and the contents of the `monomod` folder into the `BepInEx/monomod` folder.
8. Run the game again. If everything runs correctly, a message will appear in the console telling you that the API was loaded.
9. For any additional mods create a new subfolder, it can be called anything and extract the zips archive into it and if there is a `BepInEx` folder within the zip instead drop the contents of that folder into the `BepInEx` root for the modding instance. EX;
    ```
    BepInEx // These go within the BepInEx root folder
    |-- config
    |-- patchers
    |-- plugins
    |-- monomod
    |-- core
    plugins // Files within go into the created plugin subfolder that was created for the mod
    |-- Art
    |-- Scripts
    |-- MyMod.dll
    manifest.json // Ignorable, but if kept, goes in plugin subfolder
    README.md // Ignorable, but if kept, goes in plugin subfolder
    CHANGELOG.md // Ignorable, but if kept, goes in plugin subfolder
    icon.png // Ignorable, but if kept, goes in plugin subfolder
    ```
10. Run the game once more and everything should be correct and working.

### Installing Manually (XBOX Game-Pass)
1. Install [BepInEx](https://thunderstore.io/package/download/BepInEx/BepInExPack_Inscryption/5.4.2305/) by pressing `Manual Download` and extract the contents into a folder. **Do not extract into the game folder!**
2. Move the contents of the `BepInExPack_Inscryption` folder into the game folder (where the game executable is; usually found here: `C:\XboxGames\Inscryption\Content`).
3. Install the following package: [XboxSilencio](https://thunderstore.io/c/inscryption/p/CORE_API_TEAM/GamepassSilencio/), this is a package which aims to solve frictions with XBOX Gamepass specific installations, such as HueyFS related errors.
4. Run the game. If everything was done correctly, you will see the BepInEx console appear on your desktop. Close the game after it finishes loading.
5. Install [MonoModLoader](https://inscryption.thunderstore.io/package/BepInEx/MonoMod_Loader_Inscryption/) and extract the contents into a folder.
6. Move the contents of the `patchers` folder into `BepInEx/patchers` (If any of the mentioned BepInEx folders don't exist, just create them).
7. Install [Inscryption API](https://inscryption.thunderstore.io/package/API_dev/API/) and extract the contents into a folder.
8. Move the contents of the `plugins` folder into `BepInEx/plugins` and the contents of the `monomod` folder into the `BepInEx/monomod` folder.
9. Run the game again. If everything runs correctly, a message will appear in the console telling you that the API was loaded.
10. For any additional mods create a new subfolder, it can be called anything and extract the zips archive into it and if there is a `BepInEx` folder within the zip instead drop the contents of that folder into the `BepInEx` root for the modding instance. EX;
    ```
    BepInEx // These go within the BepInEx root folder
    |-- config
    |-- patchers
    |-- plugins
    |-- monomod
    |-- core
    plugins // Files within go into the created plugin subfolder that was created for the mod
    |-- Art
    |-- Scripts
    |-- MyMod.dll
    manifest.json // Ignorable, but if kept, goes in plugin subfolder
    README.md // Ignorable, but if kept, goes in plugin subfolder
    CHANGELOG.md // Ignorable, but if kept, goes in plugin subfolder
    icon.png // Ignorable, but if kept, goes in plugin subfolder
    ```
11. Run the game once more and everything should be correct and working.

### Installing on the Steam Deck
1. Download [r2modman](https://thunderstore.io/c/inscryption/p/ebkr/r2modman/) on the Steam Deck's Desktop Mode and open it from its download using its `AppImage` file.
2. Download the mods you plan on using and their dependencies.
3. Go to the setting of the profile you are using for the mods and click `Browse Profile Folder`.
4. Copy the BepInEx folder, then go to Steam and open Inscryption's Properties menu
5. Go to `Installed Files` click `Browse` to open the folder containing Inscryption's local files; paste the BepInEx folder there.
6. Enter Gaming Mode and check 'Force the use of a specific Steam Play compatibility tool' in the Properties menu under `Compatibility`.
7. Go to the launch parameters and enter `WINEDLLOVERRIDES="winhttp.dll=n,b" %command%`.
8. Open Inscryption. If everything was done correctly, you should see a console appear on your screen.

### Mac & Linux
1. Follow the steps here first: <https://docs.bepinex.dev/articles/user_guide/installation/index.html>
2. Next do steps 4-10 of the Manual Installation
3. Your game should be setup for inscryption modding now

If you have any issues with Mac/Linux, Steam Deck, or Manual head over to the discord for this game:

* **Inscryption Modding Discord:** [Here](https://discord.gg/ZQPvfKEpwM)

## Getting Started: Modding
---
Modding Inscryption requires a knowledge of coding in C#, and in many cases an understanding of how to patch the game using HarmonyPatch.

If you're unfamiliar with any of this, or just want to create cards and sigils, you can use [JSONLoader](https://inscryption.thunderstore.io/package/MADH95Mods/JSONCardLoader/). 

### Modding with JSONLoader
 JSONLoader is a versatile mode that provides a more beginner-friendly way of creating new cards and abilities for Inscryption using JSON syntax, which is much simpler than C#.

JSONLoader's documentation can be found [here](https://github.com/MADH95/JSONLoader).

A video tutorial covering how to use JSONLoader in a basic form can be found [here](https://www.youtube.com/watch?v=grTSkpI4U7g).

### Modding with C#
To begin modding with C#, you will need to create a new C# project using a code editor.
We recommend and assume you're using Microsoft's Visual Studio.

Your project's target framework needs to be `netstandard2.0`.

Once your project's created, go to `Project > Manage NuGet Packages`.
Click the dropdown menu for 'Package source' and check that 'BepInEx' and 'nuget' is there.

If BepInEx or nuget aren't an available source, we need to add them.
To add a new package source, click on the gear icon next to the package source selector, then click the large green plus-sign.

To add BepInEx, change the name to 'BepInEx' and the source link to 'https://nuget.bepinex.dev/v3/index.json'.
To add nuget, change the name to 'nuget' and the source link to 'https://nuget.windows10ce.com/nuget/v3/index.json'.

Change the package source to 'All' then click 'Browse'.
We want to install the following packages (**Make sure the version numbers match!**):
- BepInEx.Analyzers v1.0.8
- BepInEx.Core v5.4.19
- HarmonyX v2.9.0
- Inscryption.GameLibs v1.9.0-r.0
- UnityEngine.Modules v2019.4.24

You will also need to add the API as a reference.
There are a couple ways to do this, detailed below; whichever way you choose to do this, you'll also need to need to reference `InscryptionAPI.dll`, which should be in your BepInEx plugins folder; copy this path for future use.

To do so, go to your 'BepInEx/plugins' folder and copy the folder path.
Then, navigate to `Project > Add Project Reference` and click 'Browse'.
Copy the folder path and add 'InscryptionAPI.dll' as a reference.
You can do this for other mods' .dll files if you want to reference them as a mod dependency (a separate mod that your mod relies on to work).

An alternative method to adding the API (and other mods) as a reference is to use NuGet packages by adding 'https://nuget.bepinex.dev/v3/index.json' as a package source, and then adding 'API_dev-API' as a reference.

With all this, you are now ready to begin creating your mod!
Some resources are provided below for you to use, including an example mod to look at for examples.
Otherwise, continue reading this wiki.

### Modding Resources
[Inscryption Modding Discord](https://discord.gg/QrJEF5Denm)

[BepInEx documentation](https://docs.bepinex.dev/)

[Harmony patching article](https://harmony.pardeike.net/articles/patching.html)

[Example Mod using C#](https://github.com/debugman18/InscryptionExampleMod)

[Vanilla and Modded Enumerations](https://github.com/SaxbyMod/SabyModEnums)

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
- Creator/Chaosyr/SaxbyMod/The Stoat Lord/ThinCreator3483
- stillrinny
- R3b00t3d-kawaii