## Getting Started: Modding
---
Modding Inscryption requires a knowledge of coding in C# or a similar programming language such as Java, and in many cases an understanding of how to patch the game using HarmonyPatch, which you can find the docs for [here](https://github.com/BepInEx/HarmonyX/wiki) for HarmonyX, and [here](https://harmony.pardeike.net/v2/articles/intro.html) for the underlying basic Harmony.

If you're unfamiliar with any of this, or just want to create cards and sigils, you can use [JSONLoader](https://inscryption.thunderstore.io/package/MADH95Mods/JSONCardLoader/), if you wish to avoid learning C#. 

If your up for the task, a good entry point to learning C# is [this tutorial by freeCodeCamp.org](https://www.youtube.com/watch?v=GhQdlIFylQ8). It's around a 4 hour video, we'd advise you to tackle it in portions. One deviation from the video however, we are going to advise you to grab a thing called [JetBrains Rider](https://www.jetbrains.com/rider/), you'll specifically want the `Free for non-commerical use` version. If your familiar with the UX of [IntelliJ Idea](https://www.jetbrains.com/idea/) for Java based modding, than it should feel familiar to you.

If at any point your confused while doing the tutorial, feel free to ask for help in [the discord](https://discord.gg/qEqtF29ZEt) specifically, the channel entitled `programming-help` will be your friend. Mind you help will not be immediate, but we will try and get help to you as soon as we are able.

### Modding with C#

First things first, in order to even make a mod for the game you will need a few things setup. First off if you haven't installed it yet, we reccomend getting a tool called [JetBrains Rider](https://www.jetbrains.com/rider/). This guide will follow the layout of this IDE. If you still want to use [Visual Studio](https://visualstudio.microsoft.com/) instead, no one is going to stop you, there will be a foldout below this guide with the setup for that IDE, note its a little outdated.

#### JetBrains Rider Based Project Setups

<details open>
<summary>
Click me to close
</summary>

Now that you have your IDE, we'll start by making our Project.

1. Open up the IDE, than click on `New`, followed by `New Solution`.
2. One the menu pops up select `Class Library` from the Left-Side panel.
3. Next we want to fill in some fields
    1. Where it says `Solution Name`, update the associated field to the name of your mod.
    2. Next to that field theres a little thing that says `format`, click the drop down and change it to `sln`. 
    3. Where it says `Target Framework`, select the dropdown and change it to `v4.7.2`. If it is not present, that means you don't have the SDK or Framework installed. 
        1. First ensure the [Net10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) is installed, you'll want the `SDK` associated with your OS and Architecture. Note you will want to download an `Installer` not a `Binary`.
        2. Now ensure the [Netframework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) is installed, you'll want the `Offline Installer` under `Build Apps` called the `Developer Pack`.
    4. Ensure the `Language` is set to `C#`, and the Type is set to `Class Library`.
4. Now press `Create`.

Now we have your basic project setup, if you see a pop up about Microsoft Defender affecting the IDE click `Exclude folders`, this will allow the IDE to do its job much quicker than it would be able to otherwise.

Alright now we'll need to make sure you have the NuGet Package sources you'll need for success. The following steps will guide you through the process.

1. Right-Click on the `Solution`, hover over `Open In`, than select `Explorer`.
2. Once open you should see the File Explorer opened to a folder containing an `sln` file with a name matching your Project's. 
3. In this folder create a new text file, than open it in a text editor. Now enter the following into the text file:
    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <packageSources>
        <add key="BepInEx" value="https://nuget.bepinex.dev/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
        <add key="ts-nuget" value="https://nuget.windows10ce.com/nuget/v3/index.json" protocolVersion="3" />
      </packageSources>
    </configuration>
    ```
4. Save the file, and rename it to `NuGet.Config`.
5. If you don't see the sources in your IDE, try following this short guide and try again: [File Explorer (Windows); JSONLoader Wiki](https://github.com/Chaosyr/JSONCardLoaderWiki/wiki/JSON-and-CSV-Loader-API-Documentation-%E2%80%90-JSON-Inscrybing#file-explorer-windows). Simply put, your windows might not of overwrote the file extension because your extensions are hidden by default.

Now that we have this setup when you hit `All enabled feeds` under the `NuGet` menu found in the bottom of the left-hand toolbar, you should see 3 feeds, labeled `BepInEx`, `nuget.org`, and `ts-nuget`. Also ensure `Prerelease` is ticked on.

Next up, press `Discover` under the `Packages` tab of the `NuGet` menu. Search up the following packages in the table below, than install the version specified to the solution, also ensure the source matches.

| Package Name           |  Version   |      Source |
| :--------------------- | :--------: | ----------: |
| `BepInEx.Analyzers`    |   1.0.8    |   `BepInEx` |
| `BepInEx.Core`         |   5.4.21   |   `BepInEx` |
| `HarmonyX`             |   2.10.2   | `nuget.org` |
| `Inscryption.GameLibs` | v1.9.0-r.0 |   `BepInEx` |
| `UnityEngine.Modules`  | v2019.4.24 |   `BepInEx` |
| `API_dev-API`          |   2.24.1   |  `ts-nuget` |

If you need any other dependencies that are mods on Thunderstore, search up its package identifier in the NuGet Search, for example `Nevernamed-Nevernameds_Sigilarium`. You can get the package identifier by going to the mods page on the Thunderstore Website, such as: [Nevernameds Sigilarium](https://thunderstore.io/c/inscryption/p/Nevernamed/Nevernameds_Sigilarium/). Beneath the Download, Like and Report Buttons, there will be a string of text resembling `Team-Name-Version`. You'll want to copy that string and remove the version part of it, that is what you will put in the NuGet Search.

</details>

#### Visual Studio Based Project Setups

<details>
<summary>
Click me to expand
</summary>

Now that you have your IDE, do the following to begin with. Your project's target framework needs to be `netstandard4.7.2`. Once your project's created, go to `Project > Manage NuGet Packages`. Click the dropdown menu for `Package source` and check that `BepInEx`, `nuget.org` and `ts-nuget` are there.

If `BepInEx`, `nuget.org` or `ts-nuget` aren't an available source, we need to add them. There are two methods you can use to address this, the first of which is entirely within Visual Studio, the second of which is through file, which may be less complicated.

<details>
<summary>
Via Package Source Configuration
</summary>
For each missing source you will want to do the following:

1. Click on the `Gear` icon next to the package source selector. 
2. Click the large green `Plus`.
3. Set the fields to match what is shown in the below list:
    - To add `BepInEx`, change the name to `BepInEx` and the source link to `https://nuget.bepinex.dev/v3/index.json`. 
    - To add `nuget.org`, change the name to `nuget.org` and the source link to `https://api.nuget.org/v3/index.json`. 
    - To add `ts-nuget`, change the name to `ts-nuget` and the source link to `https://nuget.windows10ce.com/nuget/v3/index.json`.

</details>

<details>
<summary>
Via File
</summary>

1. Right-Click the `Solution` found at the top of the `Solution Explorer`.
2. Click `Open Folder in File Explorer`.
3. Once open you should see the File Explorer opened to a folder containing an `sln` file with a name matching your Project's. 
4. In this folder create a new text file, than open it in a text editor. Now enter the following into the text file:
    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <packageSources>
        <add key="BepInEx" value="https://nuget.bepinex.dev/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
        <add key="ts-nuget" value="https://nuget.windows10ce.com/nuget/v3/index.json" protocolVersion="3" />
      </packageSources>
    </configuration>
    ```
5. Save the file, and rename it to `NuGet.Config`.
6. If you don't see the sources in your IDE, try following this short guide and try again: [File Explorer (Windows); JSONLoader Wiki](https://github.com/Chaosyr/JSONCardLoaderWiki/wiki/JSON-and-CSV-Loader-API-Documentation-%E2%80%90-JSON-Inscrybing#file-explorer-windows). Simply put, your windows might not of overwrote the file extension because your extensions are hidden by default. If that fails try the `Via Package Source Configuration` method.

</details>

Now that we have this setup change the package source to  `All`. Also ensure `Prerelease` is ticked on.

Next up, press `Browse` in the menu. Search up the following packages in the table below, than install the version specified to the solution, also ensure the source matches.

| Package Name           |  Version   |      Source |
| :--------------------- | :--------: | ----------: |
| `BepInEx.Analyzers`    |   1.0.8    |   `BepInEx` |
| `BepInEx.Core`         |   5.4.21   |   `BepInEx` |
| `HarmonyX`             |   2.10.2   | `nuget.org` |
| `Inscryption.GameLibs` | v1.9.0-r.0 |   `BepInEx` |
| `UnityEngine.Modules`  | v2019.4.24 |   `BepInEx` |
| `API_dev-API`          |   2.24.1   |  `ts-nuget` |

If you need any other dependencies that are mods on Thunderstore, search up its package identifier in the NuGet Search, for example `Nevernamed-Nevernameds_Sigilarium`. You can get the package identifier by going to the mods page on the Thunderstore Website, such as: [Nevernameds Sigilarium](https://thunderstore.io/c/inscryption/p/Nevernamed/Nevernameds_Sigilarium/). Beneath the Download, Like and Report Buttons, there will be a string of text resembling `Team-Name-Version`. You'll want to copy that string and remove the version part of it, that is what you will put in the NuGet Search.

</details>

You are now ready to begin creating your mod! Some resources are provided below for you to use, including an example mod to look at for examples. Otherwise, continue reading this wiki.