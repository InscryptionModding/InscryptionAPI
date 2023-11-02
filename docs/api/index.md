# API Documentation
Hello, and welcome to the documentation section of the Inscryption API!

This section documents the various classes and members of the API, providing information on what they do.

This is heavily work-in-progress so some information may be missing!

# Custom Field and Properties
A table of various properties and fields the API adds for modders to use.

## Extra Alternate Portraits
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

## Extended Properties
The API implements a system of custom properties that you can apply to cards, abilities, and card modification info's.
For more information on extensions properties go to [this section of the wiki](https://inscryptionmodding.github.io/InscryptionAPI/wiki/index.html#custom-card-properties).

Some extended properties are reserved by the API for certain uses.
The following are some extension properties you can use for your cards.

If you're using C# you can set these properties using their respective setter method, and can retrieve these properties with the appropritate getter method.
For JsonLoader users, these properties can be accessed using the same method as accessing any other extended property.

**NOTE THAT THE NAMES ARE CASE-SENSITIVE.**

|Property Name          |Affected Type  |Value Type |Description                                                                            |Extension Method       |
|-----------------------|---------------|-----------|---------------------------------------------------------------------------------------|-----------------------|
|TriggersOncePerStack   |AbilityInfo    |Boolean    |If the ability should trigger twice when the card evolves.                             |SetTriggersOncePerStack|
|HideSingleStacks       |AbilityInfo    |Boolean    |If making an ability hidden should hide all of an ability's stacks or only one per.    |SetHideSingleStacks    |
|AffectedByTidalLock    |CardInfo       |Boolean    |If the card should be killed by the effect of Tidal Lock.                              |SetAffectedByTidalLock |
|TransformerCardId		|CardInfo		|String		|The name of the card this card will transform into when it has the Transformer sigil.  |SetTransformerCardId   |

## Part2Modular
The API adds a custom AbilityMetaCategory called Part2Modular, accessible from the AbilityManager.

This metacategory is used in the community patches to determine what sigils the Amorphous sigil can become while in Act 2, but is otherwise free for you to use.

A number of vanilla sigils have been marked with this metacategory by default.

## Secrets
Shhh, work-in-progress.
[test](https://inscryptionmodding.github.io/InscryptionAPI/api/test.html)
[test2](https://inscryptionmodding.github.io/InscryptionAPI/api/test2.html)