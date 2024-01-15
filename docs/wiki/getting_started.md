## Getting Started: Installation
---
To begin, we'll go over how to install BepInEx, the framework all Inscryption mods use.  This is a necessary step to playing modded Inscryption, so be sure to follow this carefully.

### Installing with a Mod Manager
This is the recommended way to install BepInEx to the game.

1. Download and install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager) or [r2modman](https://Timberborn.thunderstore.io/package/ebkr/r2modman/).
2. Click the **Install with Mod Manager** button on the top of [BepInEx's](https://thunderstore.io/package/download/BepInEx/BepInExPack_Inscryption/5.4.1902/) page.
3. Run the game via the mod manager.

### Installing Manually
1. Install [BepInEx](https://thunderstore.io/package/download/BepInEx/BepInExPack_Inscryption/5.4.1902/) by pressing 'Manual Download' and extract the contents into a folder. **Do not extract into the game folder!**
2. Move the contents of the 'BepInExPack_Inscryption' folder into the game folder (where the game executable is).
3. Run the game. If everything was done correctly, you will see the BepInEx console appear on your desktop. Close the game after it finishes loading.
4. Install [MonoModLoader](https://inscryption.thunderstore.io/package/BepInEx/MonoMod_Loader_Inscryption/) and extract the contents into a folder.
5. Move the contents of the 'patchers' folder into 'BepInEx/patchers' (If any of the mentioned BepInEx folders don't exist, just create them).
6. Install [Inscryption API](https://inscryption.thunderstore.io/package/API_dev/API/) and extract the contents into a folder.
7. Move the contents of the 'plugins' folder into 'BepInEx/plugins' and the contents of the 'monomod' folder into the 'BepInEx/monomod' folder.
8. Run the game again. If everything runs correctly, a message will appear in the console telling you that the API was loaded.

### Installing on the Steam Deck:
1. Download [r2modman](https://Timberborn.thunderstore.io/package/ebkr/r2modman/) on the Steam Deck’s Desktop Mode and open it from its download using its `AppImage` file.
2. Download the mods you plan on using and their dependencies..
3. Go to the setting of the profile you are using for the mods and click `Browse Profile Folder`.
4. Copy the BepInEx folder, then go to Steam and open Inscryption's Properties menu
5. Go to `Installed Files` click `Browse` to open the folder containing Inscryption's local files; paste the BepInEx folder there.
6. Enter Gaming Mode and check 'Force the use of a specific Steam Play compatibility tool' in the Properties menu under `Compatibility`.
7. Go to the launch parameters and enter `WINEDLLOVERRIDES=“winhttp.dll=n,b” %command%`.
8. Open Inscryption. If everything was done correctly, you should see a console appear on your screen.

## Getting Started: Modding
---
Modding Inscryption requires a knowledge of coding in C#, and in many cases an understanding of how to patch the game using HarmonyPatch.

If you're unfamiliar with any of this, or just want to create cards and sigils, you can use [JSONLoader](https://inscryption.thunderstore.io/package/MADH95Mods/JSONCardLoader/). 

### Modding with JSONLoader
 JSONLoader is a versatile mode that provides a more beginner-friendly way of creating new cards and abilities for Inscryption using JSON syntax, which is much simpler than C#.

JSONLoader's documentation can be found [here](https://github.com/MADH95/JSONLoader).

A video tutorial covering how to use JSONLoader can be found [here](https://www.youtube.com/watch?v=grTSkpI4U7g).

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