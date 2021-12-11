
# API
## Inscryption API made by Cyantist

This plugin is a BepInEx plugin made for Inscryption as an API.
It can currently:
- Create custom cards and inject them into the card pool
- Modify existing cards in the card pool
- Create custom abilities and inject them into the ability pool
- Reference abilities between mods
- Create custom regions
- Enable energy

## Installation (automated)
This is the recommended way to install the API on the game.

- Download and install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager) or [r2modman](https://timberborn.thunderstore.io/package/ebkr/r2modman/)
- Click Install with Mod Manager button on top of the [page](https://inscryption.thunderstore.io/package/API_dev/API/)
- Run the game via the mod manager

## Installation (manual)
To install this plugin first you need to install BepInEx as a mod loader for Inscryption. A guide to do this can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html#where-to-download-bepinex). Inscryption needs the 86x (32 bit) mono version.

To install Inscryption API you simply need to copy **API.dll** from [releases](https://github.com/ScottWilson0903/InscryptionAPI/releases) to **Inscryption/BepInEx/plugins**.

An example Mod utilising this plugin can be found [here](https://github.com/ScottWilson0903/InscryptionExampleMod).

## Debugging
The easiest way to check if the plugin is working properly or to debug an error is to enable the console. This can be done by changing
```
[Logging.Console]
\## Enables showing a console for log output.
\# Setting type: Boolean
\# Default value: false
Enabled = false
```
to
```
[Logging.Console]
\## Enables showing a console for log output.
\# Setting type: Boolean
\# Default value: false
Enabled = true
```
in **Inscryption/BepInEx/Config/BepInEx/cfg**
___
If you want help debugging you can find me on the [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm) or on [Daniel Mullins Discord](https://discord.com/invite/danielmullinsgames) as Cyantist.

## Development
At the moment I am working on:

 - Adding comments
 - Documentation
 - Custom special abilities

The next planned features for this plugin are:

 - Extending the loader to handle and load custom ~~abilities,~~ boons and items.

## Building This Project
To build this project, you must add a GameFolder.props file inside the same directory that contains your Dependencies.props file.

Inside the GameFolder.props file, it should look something like this:

```
<Project>
  <PropertyGroup>
    <GameFolder>PATH_TO_GAME</GameFolder>
  </PropertyGroup>
</Project>
```

You'd obviously replace `PATH_TO_GAME` with the path to where Inscription.exe is located. For example, this would be `C:/Program Files (x86)/Steam/steamapps/common/Inscryption`

## Contribution
### How can you help?
Use the plugin and report bugs you find! Lots of traits won't be designed to work well together and may cause bugs or crashes. At the very least we can document this. Ideally we can create generic patches for them.
### But really, I want to help develop this mod
Great! I'm more than happy to accept help. Either make a pull request or come join us over in the [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm).
### Can I donate?
Donations are totally not needed, this is a passion project before anything else. If you do still want to donate though, you can buy me a coffee on [ko-fi](https://ko-fi.com/madcyantist).

## Contributors
- Windows10CE
