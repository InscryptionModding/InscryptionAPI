## Inscryption Modding Wiki
---
Welcome to the modding wiki!  This document will help familiarise you with modding Inscryption using the Inscryption API.
Here you will find in-depth information on the API's numerous features, both what they do and how you can use them.

For a full list of API classes and members, or are looking for more technical information, you can look at the documentation section.

## Game Tweaks
---
Included with the API are a number of game changes for aiding with multi-Act support and further modding customisation.

The API package also comes with a second DLL consisting of multiple community patches, either fixing bugs or providing QoL changes for the game.

### Card Cost Displays
Cards in Acts 1 and 2 can now display multiple costs at the same time, and cards in Act 1 can now display Energy and Mox costs.

### Energy Drone in Act One/Kaycee's Mod
With the API installed, Act 3's energy management drone can be made available in Act 1 and in Kaycee's Mod. It will appear automatically if any cards with an energy or gem cost are in the Act 1 card pool, and can be forced to appear by modifying the configuration for the API.

The energy and mox displays will appear on the battle scales by default; this can be changed in the configuration file.

You can also force these drones to appear in different sections of the game by overriding the following values:
```c#
using InscryptionCommunityPatch.ResourceManagers;

EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigEnergy = true; // Enables energy
EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigDrone = true; // Makes the drone appear
EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigMox = true; // Enables Mox management
EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigDroneMox = true; // Makes the Mox drone appear
```

Currently, the only zones where these settings will have any effect are CardTemple.Nature (Leshy's cabin) and CardTemple.Undead (Grimora's cabin).

### Bones Display in Act Three / P03 in Kaycee's Mod
With the API installed, a separate bones displayer can be made available in Act 3. It will appear automatically if any cards with a bones cost are in the Act 3 card pool, and can be forced to appear by modifying the configuration for the API. This displayer appears as a TV screen hanging on the resource drone below the Gems module.

You can also force this to be active using code:

```c#
using InscryptionCommunityPatch.ResourceManagers;

Act3BonesDisplayer.ForceBonesDisplayActive = true; // Forces the bones TV screen to be visible in act 3.
```

If the bones TV screen is active, a bolt will also be dropped on top of each card that dies in-game (the same way that bone tokens are dropped on top of cards that die in Leshy's cabin).

### DeathShield Ability Behaviour
The API changes how DeathShield (aka Nano Armour/Armoured) functions, with the ability now being attached to a custom ability behaviour  'APIDeathShield' that inherits from DamageShieldBehaviour (more info further below).

## Extra Alternate Portraits
---
In Inscryption, there are some situations where a card's portrait is changed.
SteelTrap, for example, changes the base card's portrait to the 'closed trap' portrait, and Mud Turtle switches its portrait upon losing its shield.
However, these cases are very limited; SteelTrap changes *all* cards to the closed trap portrait, even if it's not on the vanilla trap card;
and only Mud Turtle can change its portrait upon losing its shield.

So the API changes this. Each added CardInfo can now be assigned custom sprites specific to the effects of SteelTrap and losing a shield, using SetSteelTrapPortrait() and SetBrokenShieldPortrait() respectively.
These are stored separately from a card's base portrait and alternate portrait, giving you greater freedom in what cards you can make.

|Name                       |Description                                                                            |Setter Methods             |
|---------------------------|---------------------------------------------------------------------------------------|---------------------------|
|PixelAlternatePortrait     |The portrait used when calling SwitchToAlternatePortrait in Act 2.                     |SetPixelAlternatePortrait  |
|SteelTrapPortrait          |The portrait used when the Steel Trap sigil activates.                                 |SetSteelTrapPortrait, SetEmissiveSteelTrapPortrait, SetPixelSteelTrapPortrait  |
|BrokenShieldPortrait       |The portrait used when this card has lost all of its shields.                          |SetBrokenShieldPortrait, SetEmissiveBrokenShieldPortrait, SetPixelBrokenShieldPortrait |
|SacrificablePortrait       |The portrait used when this card is on the board and the player is choosing sacrifices.|SetSacrificablePortrait, SetEmissiveSacrificablePortrait, SetPixelSacrificablePortrait |

## Part2Modular
The API adds a custom AbilityMetaCategory called Part2Modular, accessible from the AbilityManager.

This metacategory is used in the community patches to determine what sigils the Amorphous sigil can become while in Act 2, but is otherwise free for you to use.

A number of vanilla sigils have been marked with this metacategory by default.

## Core Features
---
### Extending Enumerations
The base game uses a number of hard-coded lists, called 'Enumerations' or 'Enums', to manage behaviors. For example, the ability "Brittle" is assigned to a card using the enumerated value Ability.Brittle. We can expand these lists, but it requires care, and it is managed by the GuidManager class. This handles the creation of new enumerations and making sure those are handled consistently across mods.

Lets say that you want to create a new story event. These are managed by the enumeration StoryEvent. To create a new story event, you should use this pattern to create a single static reference to that new value:

```c#
public static readonly StoryEvent MyEvent = GuidManager.GetEnumValue<StoryEvent>(MyPlugin.guid, "MyNewStoryEvent");
```

GuidManager requires you to give it the GUID of your plugin as well as a friendly name for the value you want to create (the plugin GUID is required to avoid issues if multiple mods try to create a new value with the same name).

If you want to get a value that was created by another mod (for example: you want to make a card that uses an ability created by another mod), you can follow this exact same pattern. You just need to know the plugin GUID for the mod that it is contained in:

```c#
public static readonly Ability OtherAbility = GuidManager.GetEnumValue<Ability>("other.mod.plugin.guid", "Ability Name");
```

All of these values are stored in the modded save file.

### Custom Game Save Data
If your mod needs to save data, the ModdedSaveManager class is here to help. There are two chunks of extra save data that you can access here: 'SaveData' (which persists across runs) and 'RunState' (which is reset on every run). Note that these require you to pass in a GUID, which should be your mod's plugin GUID, and an arbitrary key, which you can select for each property to you want to save.

The easiest way to use these helpers is to map them behind static properties, like so:

```c#
public static int NumberOfItems
{
    get { return ModdedSaveManager.SaveData.GetValueAsInt(Plugin.PluginGuid, "NumberOfItems"); }
    set { ModdedSaveManager.SaveData.SetValue(Plugin.PluginGuid, "NumberOfItems", value); }
}
```

When written like this, the static property "NumberOfItems" now automatically syncs to the save file.