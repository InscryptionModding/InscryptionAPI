
# CardLoaderPlugin
## Inscryption Card Loader Plugin made by Cyantist

This plugin is a BepInEx plugin made for Inscryption to create custom cards and inject them into the card pool.
One example card is provided in the Plugin class with method name AddBears.

## Installation
To install this plugin first you need to install BepInEx as a mod loader for Inscryption. A guide to do this can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html#where-to-download-bepinex). Inscryption needs the 86x (32 bit) mono version.

To install CardLoaderPlugin with just the default example card included "Eight Fucking Bears!", you simply need to copy **CardLoaderPlugin.dll**, and the **Artwork** folder from **bin/Debug/netstandard2.0** to a **CardLoader** folder inside **Inscryption/BepInEx/plugins**.

To generate your own cards, you will need to either pass in an already created **CardInfo** object to **Plugin.AddCard**, or you will need to pass all the required and optional parameters to **Plugin.AddCard** as done in **Plugin.AddBears**. Any png files should be added to the **Artwork** folder and should be 114x94 pixels. The newly compiled **CardLoaderPlugin.dll** should be installed exactly the same way as above.

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
If you want help debugging you can find me on [Daniel Mullins Discord](https://discord.com/invite/danielmullinsgames) as Cyantist.

## Development
At the moment I am working on:

 - Cleaning the plugins code up
 - Adding comments
 - Improving the parameter handle order and names
 - Documentation

The next planned features for this plugin are:

 - Better handling for more complex card traits such as evolve parameters
 - Loading cards from .asset files or maybe even json files
 - Extending the loader to handle and load custom abilities, boons and items.

Future planned features for this plugin include:

 - Loading cards and other assets without recompiling the plugin
 - A gui for card creation
 - Automatic installation

## Contribution
### How can you help?
Use the plugin and report bugs you find! Lots of traits won't be designed to work well together and may cause bugs or crashes. At the very least we can document this. Ideally we can create generic patches for them.
### But really, I want to help develop this mod
Great! I'm more than happy to accept help. Either make a pull request or come join us over in Daniel Mullins Discord, you can normally find me in the thread Modding in #inscribe-datamine but hopefully we'll have a dedicated channel soon

