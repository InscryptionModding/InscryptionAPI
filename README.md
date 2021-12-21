
# API
## Inscryption API made by Cyantist

This plugin is a BepInEx plugin made for Inscryption as an API.
It can currently:
- Create custom cards and inject them into the card pool
- Modify existing cards in the card pool
- Create custom card backgrounds
- Create custom abilities and inject them into the ability pool
- Reference abilities between mods
- Create custom regions and encounters
- Create talking cards and add dialogues
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

## Using the API

### Custom Save Game Data
If your mod needs to save data, the ModdedSaveManager class is here to help. There are two chunks of extra save data that you can access here: 'SaveData' (which persists across runs) and 'RunState' (which is reset on every run). Note that these require you to pass in a GUID, which should be your mod's plugin GUID, and an arbitrary key, which you can select for each property to you want to save.

The easiest way to use these helpers is to map them behind static properties, like so:

```
public static int NumberOfItems
{
    get { return ModdedSaveManager.SaveData.GetValueAsInt(Plugin.PluginGuid, "NumberOfItems"); }
    set { ModdedSaveManager.SaveData.SetValue(Plugin.PluginGuid, "NumberOfItems", value); }
}
```

When written like this, the static property "NumberOfItems" now automatically syncs to the save file.

### Adding new Challenges
The API supports adding new Challenges as part of Kaycee's Mod using the ChallengeManager class. You can add a new Challenge using ChallengeManager.Add, either by passing in a new AscensionChallengeInfo object or by passing the individual properties of your challenge (which will construct the information object for you). This will make your challenge automatically appear in the challenge selection screen.

If you use the overload of Add that takes an AscensionChallengeInfo object, note that the "challengeType" field of type AscensionChallenge is completely irrelevant. This is an enumerated value, and it will be set for you by the ChallengeManager to ensure there is no collision with other challenges created by other mods. As such, you need to save the ID that is returned by the Add method of ChallengeManager so that you can reference it later.

If your challenge can stack, set the stackable flag to 'true' when adding your challenge. This will cause it to appear twice in the challenge selection screen. Yes, this means stackable challenges are currently limited to a max of two.

You will be responsible to write all of the necessary patches for your challenge to function. When writing those patches, you *must* make sure that the challenge is active before you do anything. This is entirely up to you - there is nothing in the API that can detect this for you.

You should also make sure to alert the user whenever your challenge has triggered some change in the game. The ChallengeActivationUI class (from DiskCardGame) has a helper to alert the user.

```
private static AscensionChallenge ID = ChallengeManager.Add
    (
        Plugin.PluginGuid,
        "Dummy Challenge",
        "This challenge doesn't do anything",
        15,
        Resources.Load<Texture2D>("Path/To/Texture")
    );

[HarmonyPatch(...)]
public static void DoSomething()
{
    if (AscensionSaveData.Data.ChallengeIsActive(ID))
    {
        ChallengeActivationUI.Instance.ShowActivation(ID);
        // Do some actual stuff
    }
}
```

### Adding Custom Screens to Kaycee's Mod

This API supports adding new screens to Kaycee's Mod that execute before a run starts. New screens can use the AscensionScreenSort attribute to influence their sort order. Custom screens will execute in the following order:

1. Starter Decks
2. Select Challenges
3. Custom Screens
    1. Requires Start
    2. Prefers Start
    3. No Preference (default)
    4. Prefers End
    5. Requires End
4. Start Run

To create a custom screen, you need to write a special screen behavior class that inherits from AscensionRunSetupScreenBase (which in turn inherits from MonoBehavior). You will be required to override the following:

- headerText: The displayed title on the screen
- showCardDisplayer: Set this to return true if you want the panel on your screen that shows card information
- showCardPanel: Set this to return true if you want the scrollable panel on your screen that shows selectable cards

There is also a virtual method called InitializeScreen which you should use to build the content of your screen.

In general, you are responsible for doing all the hard work of building your screen. However, all the boilerplate content is built for you. You will automatically be given the continue and back buttons, the header (which shows the current challenge level), the footer (which displays changes as the challenge level changes), and the title of your screen. You can also optionally be given a scrollable card selection panel and card information displayer panel if you need them (using the properties shown above).

Once you've written the custom screen class, you need to register it with AscensionScreenManager like so:

```
AscensionScreenManager.RegisterScreen<MyCustomScreen>();
```

## Development
At the moment I am working on:

 - Adding comments
 - Documentation
 - Custom special abilities

The next planned features for this plugin are:

 - Extending the loader to handle and load custom ~~abilities,~~ boons and items.

## Contribution
### How can you help?
Use the plugin and report bugs you find! Lots of traits won't be designed to work well together and may cause bugs or crashes. At the very least we can document this. Ideally we can create generic patches for them.
### But really, I want to help develop this mod
Great! I'm more than happy to accept help. Either make a pull request or come join us over in the [Inscryption Modding Discord](https://discord.gg/QrJEF5Denm).
### Can I donate?
Donations are totally not needed, this is a passion project before anything else. If you do still want to donate though, you can buy me a coffee on [ko-fi](https://ko-fi.com/madcyantist).

## Contributors
- divisionbyz0rro
- IngoH
- JamesVeug
- julian-perge
- Windows10CE
