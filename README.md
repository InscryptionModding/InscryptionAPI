
# API

## Inscryption API

This plugin is a BepInEx plugin made for Inscryption as an API. This is the de-facto standard API for Inscryption modders.

It can currently create and modify:
- Cards
- Abilities
- Appearance behaviours
- Stat icons
- Challenges
- Starter Decks
- Regions
- Encounters
- And much more!

Additionally, a number of quality-of-life patches from the community have been included in this release.

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

## Visually Stackable Sigils by DivisionByZ0rro
Combines multiple instances of the same sigil on a single card into a single sigil with a number to free up space on the card.

## Cost Render Fix by Void Slime
Displays hybrid cost cards correctly and makes energy and mox show up on act 1 cards

## Cost choice node fix by Void Slime
If energy/mox cards are in the Act 1 pool, energy and mox card choice nodes will be added to the cost choice node in ACt 1.

# Using the API

Inscryption API 2.0 tries to have you use the original game's objects as much as possible. For example, there are no more 'NewCard' and 'CustomCard' objects; instead, you are responsible to create CardInfo objects yourself and add them.
The API does provide a number of helper methods to make this process simpler for you.

## Tweaks

### Energy Drone in Act One/Kaycee's Mod

With the API installed, the energy management drone can be made available in Act 1 and in Kaycee's Mod. It will appear automatically if any cards with an energy or gem cost are in the Act 1 card pool, and can be forced to appear by modifying the configuration for the API.

### Card cost display

Cards in Act 1 can now display multiple costs at the same time, and can display gem and energy cost in addition to blood and bones cost.

Cards in Act 2 can now display multiple costs at the same time.

## Core Features

### Extending Enumerations

The base game uses a number of hard-coded lists, called 'Enumerations' or 'Enums,' to manage behaviors. For example, the ability "Brittle" is assigned to a card using the enumerated value Ability.Brittle. We can expand these lists, but it requires care, and it is managed by the GuidManager class. This handles the creation of new enumerations and making sure those are handled consistently across mods.

Lets say that you want to create a new story event. These are managed by the enumeration StoryEvent. To create a new story event, you should use this pattern to create a single static reference to that new value:

```c#
public static readonly StoryEvent MyEvent = GuidManager.GetEnumValue<StoryEvent>(MyPlugin.guid, "MyNewStoryEvent");
```

GuidManager requires you to give it the guid of your plugin as well as a friendly name for the value you want to create (the plugin guid is required to avoid any issues if multiple mods try to create a new value with the same name).

If you want to get a value that was created by another mod (for example: you want to make a card that uses an ability created by another mod), you can follow this exact same pattern. You just need to know the plugin guid for the mod that it is contained in:

```c#
public static readonly Ability OtherAbility = GuidManager.GetEnumValue<Ability>("other.mod.plugin.guid", "Ability Name");
```

All of these values are stored in the modded save file.

### Custom Save Game Data
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

## Cards and Abilities

### Card Management

Card management is handled through InscryptionAPI.Card.CardManager. You can simply call CardManager.Add with a CardInfo object and that card will immediately be added to the card pool:

```c#
CardInfo myCard = ...;
CardManager.Add(myCard); // Boom: done
```

You can create CardInfo objects however you want. However, there are some helper methods available to simplify this process for you.
The most import of these is CardManager.New(name, displayName, attack, health, optional description) which creates a new card and adds it for you automatically:

```c#
CardInfo myCard = CardManager.New("example_card", "Sample Card", 2, 2, "This is just a sample card");
```

From here, you can modify the card as you wish, and the changes will stay synced with the game:
```c#
CardInfo myCard = CardManager.New("example_card", "Sample Card", 2, 2, "This is just a sample card");
myCard.cost = 2;
```

However, there are also a number of extension methods you can chain together to perform a number of common tasks when creating a new card. Here is an example of them in action, followed by a full list:

```c#
CardInfo myCard = CardManager.New("example_card", "Sample Card", 2, 2, "This is just a sample card")
    .SetDefaultPart1Card()
    .SetCost(bloodCost: 1, bonesCost: 2)
    .SetPortrait("/art/sample_portrait.png", "/art/sample_emissive_portrait.png")
    .AddAbilities(Ability.BuffEnemies, Ability.TailOnHit)
    .SetTail("Geck", "/art/sample_lost_tail.png")
    .SetRare();
```

The following card extensions are available:
- **SetPortrait:** Assigns the cards primary art, and optionally its emissive portrait as well. You can supply Texture2D directly, or supply a path to the card's art.
- **SetEmissivePortrait:** If a card already has a portrait and you just want to modify it's emissive portrait, you can use this. Note that this will throw an exception if the card does not have a portrait already.
- **SetAltPortrait:** Assigns the card's alternate portrait
- **SetPixelPortrait:** Assigns the card's pixel portrait (for GBC mode)
- **SetCost:** Sets the cost for the card
- **SetDefaultPart1Card:** Sets all of the metadata necessary to make this card playable in Part 1 (Leshy's cabin).
- **SetGBCPlayable:** Sets all of the metadata necessary to make this card playable in Part 2.
- **SetDefaultPart3Card:** Sets all of the metadata necessary to make this card playable in Part 3 (P03's cabin).
- **SetRare:** Sets all of the metadata ncessary to make this card look and play as Rare.
- **SetTerrain:** Sets all of the metadata necessary to make this card look and play as terrain
- **SetTail:** Creates tail parameters. Note that you must also add the TailOnHit ability for this to do anything
- **SetIceCube:** Creates ice cube parameters. Note that you must also add the IceCube ability for this to do anything
- **SetEvolve:** Creates evolve parameters. Note that you must also add the Evolve ability for this to do anything
- **AddAbilities:** Add any number of abilities to the card. This will add duplicates.
- **AddAppearances:** Add any number of appearance behaviors to the card. No duplicates will be added.
- **AddMetaCategories:** Add any number of metacategories to the card. No duplicates will be added.
- **AddTraits:** Add any number of traits to the card. No duplicates will be added.
- **AddTribes:** Add any number of tribes to the card. No duplicates will be added.
- **AddSpecialAbilities:** Add any number of special abilities to the card. No duplicates will be added.

### Evolve, Tail, Ice Cube, and delayed loading
It's possible that at the time your card is built, the card that you want to evolve into has not been built yet. You can use the event handler to delay building the evolve/icecube/tail parameters of your card, or you can use the extension methods above which will handle that for you.

However, note that if you use these extension methods to build these parameters, and the card does not exist yet, the parameters will come back null. You will not see the evolve parameters until you add the evolution to the card list **and** you get a fresh copy of the card from CardLoader (as would happen in game).

```c#
CardManager.New("Base", "Base Card", 2, 2).SetEvolve("Evolve Card", 1); // "Evolve Card" hasn't been built yet
Plugin.Log.LogInfo(CardLoader.GetCardByName("Base").evolveParams == null); // TRUE!

CardInfo myEvolveCard = CardManager.New("Evolve", "Evolve Card", 2, 5);

Plugin.Log.LogInfo(CardLoader.GetCardByName("Base").evolveParams == null); // FALSE!
```

### Editing existing cards

If you want to edit a card that comes with the base game, you can simply find that card in the BaseGameCards list in CardManager, then edit it directly:

```c#
CardInfo card = CardManager.BaseGameCards.CardByName("Porcupine");
card.AddTraits(Trait.KillsSurvivors);
```

There is also an advanced editing pattern that you can use to not only edit base game cards, but also potentially edit cards that might be added by other mods. To do this, you will add an event handler to the CardManager.ModifyCardList event. This handler must accept a list of CardInfo objects and return a list of CardInfo objects. In that handlers, look for the cards you want to modify and modify them there.

In this example, we want to make all cards that have either the Touch of Death or Sharp Quills ability to also gain the trait "Kills Survivors":

```c#
CardManager.ModifyCardList += delegate(List<CardInfo> cards)
{
    foreach (CardInfo card in cards.Where(c => c.HasAbility(Ability.Sharp) || c.HasAbility(Ability.Deathtouch)))
        card.AddTraits(Trait.KillsSurvivors);

    return cards;
};
```

By doing this, you can ensure that not on all of the base game cards get modified, but also all other cards added by other mods.

### Custom card properties

The API allows you to add custom properties to a card, and then retrieve them for use inside of abilities. In the same way that you can use Evolve parameters to make the evolve ability work, or the Ice Cube parameters to make the IceCube ability work, this can allow you to set custom parameters to make your custom abilities work.

```c#

CardInfo sample = CardLoader.CardByName("MyCustomCard");
sample.SetExtendedProperty("CustomPropertyName", "CustomPropertyValue");

string propValue = sample.GetExtendedProperty("CustomPropertyName");
```

### Ability Management

Abilities are unfortunately a little more difficult to manage than cards. First of all, they have an attached 'AbilityBehaviour' type which you must implement. Second, the texture for the ability is not actually stored on the AbilityInfo object itself; it is managed separately (bizarrely, the pixel ability icon *is* on the AbilityInfo object, but we won't get into all that).

Regardless, the API will help you manage all of this with the helpful AbilityManager class. Simply create an AbilityInfo object, and then call AbilityManager.Add with that info object, the texture for the icon, and the type that implements the ability.

Abilities inherit from DiskCardGame.AbilityBehaviour

```c#
AbilityInfo myinfo = ...;
Texture2D myTexture = ...;
AbilityManager.Add(MyPlugin.guid, myInfo, typeof(MyAbilityType), myTexture);
```

You can also use the AbilityManager.New method to simplify some of this process:

```c#
AbilityInfo myInfo = AbilityManager.New(MyPlugin.guid, "Ability Name", "Ability Description", typeof(MyAbilityType), "/art/my_icon.png");
```

And there are also extension methods to help you here as well:

```c#
AbilityManager.New(MyPlugin.guid, "Ability Name", "Ability Description", typeof(MyAbilityType), "/art/my_icon.png")
    .SetDefaultPart1Ability()
    .SetPixelIcon("/art/my_pixel_icon.png");
```

- **SetCustomFlippedTexture:** Use this to give the ability a custom texture when it belongs to the opponent.
- **SetPixelIcon:** Use this to set the texture used when rendered as a GBC card.
- **AddMetaCategories:** Adds any number of metacategories to the ability. Will not duplicate.
- **SetDefaultPart1Ability:** Makes this appear in the part 1 rulebook and randomly appear on mid-tier trader cards and totems.
- **SetDefaultPart3Ability:** Makes this appear in the part 3 rulebook and be valid for create-a-card (make sure your power level is accurate here!)

#### How abilities are programmed
Abilities require an instance of AbilityInfo that contains the information about the ability, but they also require you to write your own class that inherits from AbilityBehaviour and describes how the ability functions.

AbilityBehaviour contains a *lot* of virtual methods. For each event that can happen during a battle, there will be a 'RespondsToXXX' and an 'OnXXX' method that you need to override. The purpose of the 'RespondsToXXX' is to indicate if your ability cares about that event - you must return True in that method for the ability to fire. Then, to actually make the ability function, you need to implement your custom behavior in the 'OnXXX' method.

See this example from the base game:

```c#
public class Sharp : AbilityBehaviour
{
    public override Ability Ability => Ability.Sharp;

    public override bool RespondsToTakeDamage(PlayableCard source) => source != null && source.Health > 0;

    public override IEnumerator OnTakeDamage(PlayableCard source)
    {
        yield return base.PreSuccessfulTriggerSequence();
        base.Card.Anim.StrongNegationEffect();
        yield return new WaitForSeconds(0.55f);
        yield return source.TakeDamage(1, base.Card);
        yield return base.LearnAbility(0.4f);
        yield break;
    }
}
```

### Special Stat Icons

Think of these like abilities for your stats (like the Ant power or Bell power from the vanilla game). You need to create a StatIconInfo object and build a type that inherits from VariableStatBehaviour in order to implement a special stat. By now, the pattern used by this API should be apparent.

Special stat icons inherit from DiskCardGame.VariableStatBehaviour

```c#
StatIconInfo myinfo = ...;
SpecialStatIconManager.Add(MyPlugin.guid, myInfo, typeof(MyStatBehaviour));
```

**or**

```c#
SpecialStatIconManager.Add(MyPlugin.guid, "Stat", "My special stat", typeof(MyStatBehaviour))
    .SetIcon("/art/special_stat_icon.png")
    .SetPixelIcon("/art/special_pixel_stat_icon.png")
    .SetDefaultPart1Ability()
    .SetDefaultPart3Ability();
```

Because StatIconInfo is so simple, there aren't very many helpers for it.

### How Stat Icons are programmed
Stat icons require an instance of StatIconInfo that contains the information about the ability, but they also require you to write your own class that inherits from VariableStatBehaviour and describes how the ability functions.

When you implement a variable stat behavior, you need to implement the abstract method GetStateValues. This method returns an array of integers: the value at index 0 is the variable attack power, and the value at index 1 is the variable health.

Here is an example from the base game:

```c#
public class BellProximity : VariableStatBehaviour
{
    protected override SpecialStatIcon IconType => SpecialStatIcon.Bell;

    protected override int[] GetStatValues()
    {
        int num = BoardManager.Instance.PlayerSlotsCopy.Count - base.PlayableCard.Slot.Index;
        int[] array = new int[2];
        array[0] = num;
        return array;
    }
}
```

### Special Triggered Abilities

Special triggered abilities are a lot like regular abilities; however, they are 'invisible' to the player (that is, they do not have icons or rulebook entries). As such, the API for these is very simple. You simply need to provide your plugin guid, the name of the ability, and the type implementing the ability, and you will be given back a wrapper containing the ID of your newly created special triggered ability.

Special triggered abilities inherit from DiskCardGame.SpecialCardBehaviour

```c#
public readonly static SpecialTriggeredAbility MyAbilityID = SpecialTriggeredAbilityManager.Add(MyPlugin.guid, "Special Ability", typeof(MySpecialTriggeredAbility)).Id;
```

And now MyAbilityID can be added to CardInfo objects.

#### How special triggered abilities are programmed
Special abilities are the same as regular abilities, except they do not have a metadata object associated with them (because they are not described or documented for the player) and the inherit from SpecialCardBehaviour instead of AbilityBehaviour.

Here is an example from the base game:

```c#
public class TrapSpawner : SpecialCardBehaviour
{
    public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => base.PlayableCard.OnBoard;

    public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
    {
        yield return new WaitForSeconds(0.35f);
        yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("Trap"), base.PlayableCard.Slot, 0.1f, true);
        yield return new WaitForSeconds(0.35f);
        yield break;
    }
}
```

### Card Appearance Behaviours

These behave the same as special triggered abilities from the perspective of the API.

Special triggered abilities inherit from DiskCardGame.CardAppearanceBehaviour

```c#
public readonly static CardAppearanceBehaviour.Appearance MyAppearanceID = CardAppearanceBehaviourManager.Add(MyPlugin.guid, "Special Appearance", typeof(MyAppearanceBehaviour)).Id;
```

#### How appearance behaviours are programmed
Appearance behaviours implement CardAppearanceBehaviour. There is an abstract method called ApplyAppearance that you must implement - here you override the default appearance of the card. There are also three other virtual methods: ResetAppearance, OnCardAddedToDeck, and OnPreRenderCard that give other hooks in which you can change the card's appearance.

Here is an example from the base game:

```c#
public class RedEmission : CardAppearanceBehaviour
{
    public override void ApplyAppearance()
    {
        base.Card.RenderInfo.forceEmissivePortrait = true;
        base.Card.StatsLayer.SetEmissionColor(GameColors.Instance.glowRed);
    }

    public override void ResetAppearance()
    {
        base.Card.RenderInfo.forceEmissivePortrait = false;
        base.Card.StatsLayer.SetEmissionColor(GameColors.Instance.glowSeafoam);
    }
}
```

## Custom Maps and Encounters

Unlike abilities, encounters are encoded into the game's data through a combination of enumerations and strings. For example, Opponents are enumerated by the enumeration Opponent.Type, but special sequencers and unique AI rules are represented as strings buried in each encounters data.

To create a custom encounter (for example, a custom boss fight), you will need some combination of opponents, special sequencers, and AI.

### Special Sequencers

Special sequencers are essentially 'global abilities;' they listen to the same triggers that cards do and can execute code based on these triggers (such as whenever cards are played or die, at the start of each turn, etc). 

While you can inherit directly from SpecialBattleSequencer, there is a hierarchy of existing special sequencers that you may wish to inherit from depending upon your use case. You can use dnSpy to see all of them, but these are three you should be specifically aware of:

- **SpecialBattleSequencer**: The base class for all special sequencers. Use this by default.
- **BossBattleSequencer**: Used for boss battles
- **Part1BossBattleSequencer**: Use for boss battles in Act 1 (Angler, Prospector, Trapper/Trader, and Leshy)

Special sequencers are set directly on the NodeData instance corresponding to the map node that the player touches to start the battle. This is done using a string value corresponding to the sequencer. To set this yourself, use the SpecialSequenceManager class in the API:

```c#
public class MyCustomBattleSequencer : Part1BossBattleSequencer
{
    public static readonly string ID = SpecialSequenceManager.Add(Plugin.PluginGuid, "MySequencer", typeof(MyCustomBattleSequencer));
}
```

### Opponents

In the context of this API, think of opponents as basically just bosses. There is a enumeration of Opponents called Opponent.Type that refers to the opponent. This name can be confusing in some IDEs, as they may simply show the parameter type as Type, which should not be confused with the System.Type type.

Like everything else, Opponents are classes that you have to write that are responsible for handling the weird things that happen during battle. Depending upon which type of opponent you are buiding, you will need to use one of the following base classes:

- **Opponent**: The base class for all Opponents. You should probably never inherit from this one directly.
- **Part1Opponent**: Used for battles in Leshy's cabin
- **Part1BossOpponent**: Used for boss battles in Leshy's cabin
- **PixelOpponent**: Used for battles in the GBC game
- **PixelBossOpponent**: Used for boss battles in the GBC game
- **Part3Opponent**: Used for battles in Botopia
- **Part3BossOpponent**: Used for boss battles in Botopia

Custom opponents will need a custom Special Sequencer as a companion (please, don't ask why we need Special Sequencers and Opponents and why they couldn't just be the same thing). So in this case, the following code snippet shows how to create an opponent that is linked to its special sequencer:

```c#
public class MyBossOpponent : Part1BossOpponent
{
    public static readonly Opponent.Type ID = OpponentManager.Add(Plugin.PluginGuid, "MyBossOpponent", MyCustomBattleSequencer.ID, typeof(MyBossOpponent)).Id;
}
```

Note that the third parameter in OpponentManager.Add is a string, and that string is the same ID that was set in the code snippet for the special sequencer in the previous example.

### AI

In most cases, you will probably not need to create a custom AI. By default, the game will look at the cards that the computer is preparing to play and then use brute force to test every possible slot that the computer could queue those slots for. It will then simulate a full turn of the game for each of those positions and determine which one has the best outcome. There are very few exceptions to this rule.

As an example, one exception is the Prospector boss fight, where the computer will always play the Pack Mule in slot 0 and a coyote in slot 1. It's important that these cards always get played in this position, so the game has a custom AI that detects this part of the game and overrides the default AI.

If you want to use a custom AI, you need to build a class that inherits from DiskCardGame.AI and overrides the virtual method SelectSlotsForCards. That one method is where you implement your custom AI logic.

To register a new AI with the game, use the AIManager class in the API, and keep a reference to the string ID that you receive back:

```c#
public class MyCustomAI : AI
{
    public static readonly string ID = AIManager.Add(Plugin.PluginGuid, "MyAI", typeof(MyCustomAI)).Id;

    public override List<CardSlot> SelectSlotsForCards(List<CardInfo> cards, CardSlot[] slots)
    {
        // Do stuff here
    }
}
```

To actually *use* your new AI, you need to set it inside of a special sequencer; specifically in the BuildCustomEncounter method:

```c#
public class MyCustomBattleSequencer : Part1BossBattleSequencer
{
    public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
    {
        EncounterData data = base.BuildCustomEncounter(nodeData);
        data.aiId = MyCustomAI.ID;
        return data;
    }
}
```

### Adding new nodes to the map

If you want to add a new node to the game map, you need to be prepared to implement potentially complex game logic inside of a "sequencer" class. Whenever the game moves to a map node, it hands control of the game over to a sequencer, which is responsible for creating game objects, animating objects, manipulating the deck, or whatever else it is that you want your custom node to do. Once done, the sequencer hands control of the game back over the main game loop. This sequencer is where you will be spending most of your time programming, testing, and debugging. It is highly recommended that you check out the example mod and/or decompile the base game's sequencers to get a feel for how to do this.

Once you have created a sequencer for your event, you also need to create an animation for your node to appear on the map. This animation is an array of precisely four textures, each of which should be 49x49 pixels in size. You need to register the sequencer and the associated texture array with the API. This is done using the NodeManager.Add generic method:

```c#
NodeManager.Add<MyCustomSequencer>(
    new Texture2D[] {
        TextureHelper.GetImageAsTexture("animated_node_1.png", typeof(MyCustomSequencer).Assembly),
        TextureHelper.GetImageAsTexture("animated_node_2.png", typeof(MyCustomSequencer).Assembly),
        TextureHelper.GetImageAsTexture("animated_node_3.png", typeof(MyCustomSequencer).Assembly),
        TextureHelper.GetImageAsTexture("animated_node_4.png", typeof(MyCustomSequencer).Assembly)
    },
    NodeManager.NodePosition.SpecialEventRandom
);
```

Note the final parameter to "Add" is a flag that tells the API where the node should appear. The NodeManager.NodePosition enumeration is a flag-style enumeration, which means that multiple values can be combined using the bitwise-or operator (|). So, for example, if you want the node to appear randomly throughout the map but also to be forced to appear directly before the boss, you can use this combination:

```c#
NodeManager.NodePosition.SpecialEventRandom | NodeManager.NodePosition.PreBoss
```

The values of NodePosition are:

- **NotGenerated:** This node will not be autogenerated on the map, but can still be manually added (for example, if you are manually manipulating Act 3 maps).
- **Act1Available:** This node can appear in Act 1 maps, not just in Kaycee's Mod maps. Without this set, the custom node will only ever appear in a Kaycee's Mod run of Act 1.
- **MapStart:** This node will be forced to appear at the start of each map, unless the node fails a prerequisite condition.
- **CardChoiceRandom:** This node will appear in the pool of random events that happen right after a battle.
- **SpecialEventRandom:** This node will appear in the pool of random events that happen right before a battle.
- **PreBoss:** This node will be forced to appear right before the boss of each map, unless the node fails a prerequisite condition
- **PostBoss:** This node will be forced to appear right after the boss of each map, unless the node fails a prerequisite condition

#### Conditional map nodes

Map nodes can implement logic to determine whether or not they should appear. To do this, you need to implement a custom 'node data' object which inherits from CustomNodeData. There is a virtual method called Initialize that you should override. Here, you can add prerequisite conditions and forced generation conditions; the former will prevent a node from generating if any condition returns false, and the latter will force the node to generate if any condition returns true.

You can add two types of conditions: simple, or map-aware. A simple condition takes no parameters and returns true or false based on whatever parameters you like (for example, if a certain ascension challenge is active or not). A map-aware condition takes two parameters: the first is the y-value of the node's position, and the second is a list of all map nodes that have previously been generated on this instnace of the map.

Here is a code sample:

```c#
public class MySpecialNodeData : CustomNodeData
{
    public override void Initialize()
    {
        // This node can only generate if the BaseDifficulty challenge is active
        this.AddGenerationPrerequisite(() => AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.BaseDifficulty));
        
        // This node is forced to generate if more than one BaseDifficulty challenge is active
        // and a deck trial node has been placed on the map already
        this.AddForceGenerationCondition((y, nodes) => AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty) > 1 
                                                            && nodes.Exists(n => n is DeckTrialNodeData));
    }
}
```

If you are using your own node data, you have to register it at the same time as your custom sequencer:

```c#
NodeManager.Add<MyCustomSequencer, MySpecialNodeData>(
    new Texture2D[] {
        TextureHelper.GetImageAsTexture("animated_node_1.png", typeof(MyCustomSequencer).Assembly),
        TextureHelper.GetImageAsTexture("animated_node_2.png", typeof(MyCustomSequencer).Assembly),
        TextureHelper.GetImageAsTexture("animated_node_3.png", typeof(MyCustomSequencer).Assembly),
        TextureHelper.GetImageAsTexture("animated_node_4.png", typeof(MyCustomSequencer).Assembly)
    },
    NodeManager.NodePosition.SpecialEventRandom
);
```

## Ascension (Kaycee's Mod)

### Adding new Challenges
The API supports adding new Challenges as part of Kaycee's Mod using the ChallengeManager class. You can add a new Challenge using ChallengeManager.Add, either by passing in a new AscensionChallengeInfo object or by passing the individual properties of your challenge (which will construct the information object for you). This will make your challenge automatically appear in the challenge selection screen.

If you use the overload of Add that takes an AscensionChallengeInfo object, note that the "challengeType" field of type AscensionChallenge is completely irrelevant. This is an enumerated value, and it will be set for you by the ChallengeManager to ensure there is no collision with other challenges created by other mods. As such, you need to save the ID that is returned by the Add method of ChallengeManager so that you can reference it later.

If your challenge can stack, set the stackable flag to 'true' when adding your challenge. This will cause it to appear twice in the challenge selection screen. Yes, this means stackable challenges are currently limited to a max of two.

You will be responsible to write all of the necessary patches for your challenge to function. When writing those patches, you *must* make sure that the challenge is active before you do anything. This is entirely up to you - there is nothing in the API that can detect this for you.

You should also make sure to alert the user whenever your challenge has triggered some change in the game. The ChallengeActivationUI class (from DiskCardGame) has a helper to alert the user.

```c#
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

### Adding new Starter Decks

Starter decks are relatively simple. They simply need a title, an icon, and a set of three cards. You can also optionally add an "unlock level" which prevents your starter deck from being unlocked until the player reaches a certain challenge level. This defaults to 0, which means that the starter deck will always be unlocked.

What if you want to make a starter deck with cards from another mod? What if you aren't 100% sure that those cards are loaded at the time that your starter deck is loaded? You can solve this manually by creating a dependency, which forces BepInEx to load the other mod first. However, if that mod uses JSON Loader, you need to create a dependency on JSON Loader instead, which could (in some very bizarre situations) create a situation where you both need to load before and after JSON Loader. Rather than sort out all of these possible scenarios one-by-one, you can instead create a starter deck with a set of three card names (strings) instead of three CardInfo objects. This will create a "delayed loading" scenario where the actual starter deck info won't be built until right before the Starter Deck screen loads.

Both patterns are shown here:

```c#
StarterDeckInfo myDeck = ScriptableObject.CreateInstance<StarterDeckInfo>();
myDeck.title = "Pelts";
myDeck.iconSprite = TextureHelper.GetImageAsSprite("art/pelts_deck_icon.png", TextureHelper.SpriteType.StarterDeckIcon);
myDeck.cards = new () { CardLoader.GetCardByName("PeltWolf"), CardLoader.GetCardByName("PeltHare"), CardLoader.GetCardByName("PeltHare") };

StarterDeckManager.Add(MyPlugin.Guid, myDeck);
```

```c#
StarterDeckManager.New(MyPlugin.Guid, "Pelts", "art/pelts_deck_icon.png", new string[] { "PeltWolf", "PeltHare", "PeltHare"});
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

```c#
AscensionScreenManager.RegisterScreen<MyCustomScreen>();
```

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
- Windows10CE
- Void Slime