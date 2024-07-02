## Custom Maps and Encounters
---
Unlike abilities, encounters are encoded into the game's data through a combination of enumerations and strings. For example, Opponents are enumerated by the enumeration Opponent.Type, but special sequencers and unique AI rules are represented as strings buried in each encounters data.

To create a custom encounter (for example, a custom boss fight), you will need some combination of opponents, special sequencers, and AI.

### Encounter Turn Plans
When you're creating an encounter, you'll want to also create a turn plan that determines what cards are played during the encounter.

The API adds a number of methods to make this process easier for you.

In this example, we will create a turn plan that is 14 turns long with the following card sequence:
Turn 1 - Play 2 Bonehounds; the slots they spawn in are chosen by the game; you have no control of it when making a turn plan
Turn 2 - Play nothing
Turn 3 - Play nothing
Turn 4 - Play 1 Bonepile
Turn 5 - Play 1 Bonepile that is replaced with a Bonehound at difficulties >= 10
Turn 6 - Play nothing
Turn 7 - Play 1 Bonepile
Repeat turns 1-7 one time.

```c#
using static InscryptionAPI.Encounters.EncounterManager;

private void AddEncounters()
{
    CardBlueprint bonePile_rp = NewCardBlueprint("Bonepile").SetReplacement("Bonehound", 10);
    
    List<List<CardBlueprint>> turnPlan = new()
    {
        CreateTurn("Bonehound", "Bonehound"),
        CreateTurn(),
        CreateTurn(),
        CreateTurn("Bonepile"),
        CreateTurn(bonePile_rp),
        CreateTurn(),
        CreateTurn("Bonepile")
    };
    
    New("ExampleEncounter").AddTurns(turnPlan).DuplicateTurns(1);
}
```

The following methods are provided for convenience:
- **New:** Creates a new instance of EncounterBlueprintData and adds it to the API (does not add the EBD to any regions).
- **NewCardBlueprint:** Creates a new CardBlueprint.
- **CreateTurn:** Creates a new List\<CardBlueprint\> that represents a single turn in an encounter's turn plan. Can pass card names through or CardBlueprints, or nothing if you want an empty turn.

The following extensions are available for making turn plans, sorted by what class they affect:
- EncounterBlueprintData
    - **SetDifficulty:** Sets the minimum and maximum difficulties at which the encounter can be used.
    - **AddDominantTribes:** Used in Totem battles to determine the potential Totem top that will be used.
    - **SetRegionSpecific:** Unused by the game, just here for posterity (unless you want to use it for something yourself).
    - **AddRandomReplacementCards:** When making a card blueprint, you can set whether the card can be randomly replaced. This field determines the cards that can potentially replace that card.
    - **SetRedundantAbilities:** Sets what abilities can't be chosen by Leshy to be used for his totem during Totem battles.
    - **SetUnlockedCardPrerequisites:** Sets what cards have to be unlocked in order to be used in this encounter.
    - **AddTurnMods:** Used in Part 3 to overclock played cards above the specified difficulty on the specified turn.
    - **AddTurn:** Adds a turn to the turn plan.
    - **AddTurns:** Adds turns to the turn plan.
    - **DuplicateTurns:** Duplicates all turns currently in the turn plan by the specified amount.
    - **SyncDifficulties:** Sets the minimum and maximum difficulties of all cards in the turn plan.
-  CardBlueprint
    - **SetDifficulty:** Sets the minimum and maximum difficulties at which this card can be played.
    - **SetReplacement:** Sets the card that will replace the base card at and above the specified difficulty.
- List\<CardBlueprint\>
    - **SetTurnDifficulty:** Sets the minimum and maximum difficulties of each card in the list.
    - **DuplicateTurn:** Duplicates the list the specified number of times. Designed to be used with EncounterBlueprintData.AddTurns().

## Adding Map Nodes
---
When adding a custom node to the game map, be prepared to implement potentially complex game logic.
Every new node requires a 'sequencer' class, which controls the event that happens when you enter a map node.
This includes the creation of game objects, animation of objects, manipulating the player's deck, and everything else you may want the map node to do.

Your sequencer class must implement the API's `ICustomNodeSequencer` interface, otherwise the API will have difficulty executing the event correctly.
The API provides a couple template sequencer classes to make this easier for you: `CustomNodeSequencer` and `CustomCardChoiceNodeSequencer`.

The former is a basic abstract class that simply implements the above interface as well as every other node-related interface in the API.
The latter class is intended for custom card choice nodes.

**It's highly recommended that you decompile the game's node sequencer classes to get a feel of how they work.**

Along with a sequencer, you will need to create an animation so the node will appear on the map.
This takes the form of an array of exactly four textures, each sized at 49x49 pixels.
Each of these textures should be similar to each other if you want to mimic the 'shakiness' of the game's nodes.

With all these pieces, the final step is to register your custom node with the API:
```c#
Assembly asm = typeof(MyCustomSequencer).Assembly;

NewNodeManager.New(
    "MyPluginGUID", "MyCustomNode",
    GenerationType.SpecialEvent, typeof(MyCustomSequencer),
    new List<Texture2D> {
        TextureHelper.GetImageAsTexture("animated_node_1.png", asm),
        TextureHelper.GetImageAsTexture("animated_node_2.png", asm),
        TextureHelper.GetImageAsTexture("animated_node_3.png", asm),
        TextureHelper.GetImageAsTexture("animated_node_4.png", asm)
    }
);
```

GenerationType is a flag-style enumeration that tells the API where the node should appear.
Flag-style means that multiple values can be combined using the bitwise-or operator (|).
For example, if you want the node to randomly appear before a battle, but also be forced to appear directly before the boss,
you can use this combination:
```c#
GenerationType.SpecialEvent | GenerationType.PreBoss
```

The values of GenerationType are:

- **None:** This node will not be autogenerated on the map, but can still be manually added (for example, if you are manually manipulating Act 3 maps).
- **SpecialCardChoice:** This node will appear in the pool of random events that happen right after a battle.
- **SpecialEvent:** This node will appear in the pool of random events that happen right before a battle.
- **RegionStart:** This node will be forced to appear at the start of each map, unless the node fails a prerequisite condition.
- **PreBoss:** This node will be forced to appear right before the boss of each map, unless the node fails a prerequisite condition
- **PostBoss:** This node will be forced to appear right after the boss of each map, unless the node fails a prerequisite condition

### Conditional Nodes
Map nodes can implement logic that determines whether or not they are generated on the map.
This is done using the `SelectionCondition` class, and when registering your node you'll have the ability to give it SelectionConditions that control its generation logic.

There are two condition types: prerequisite and forced generation.
The former prevents a node from generating if any condition return false, and the latter forces a node to generate if any condition returns true;

The game comes prepackaged with several premade SelectionConditions, but you can also create your own.

Base SelectionConditions:
- **CardsInDeckTraits:** Checks if any cards in your deck have the given Trait, can be used as a blacklist or whitelist.
- **EitherOr:** Checks if either of the given SelectionConditions were satisified.
- **IsAscension:** Checks if the player is playing Kaycee's Mod.
- **PastRunsCompleted:** Checks if the given integer is greater or equal to the number of previous completed runs.
- **PreviousNodesContent:** Checks if the list of all previous map nodes contains the given NodeType, can be used as a blacklist or whitelist.
- **PreviousRowContent:** Checks if the previous row of map nodes contains the given NodeType, can be used as a blacklist or whitelist.
- **StoryEventCompleted:** Checks if the given StoryEvent has been completed.
- **WithinGridYRange:** Checks if the current map node location is within the given range.

The API also adds two custom SelectionConditions:
- **ChallengeIsActive:** Checks if the given Challenge is active, can be used as a blacklist or whitelist.
- **NumChallengesOfTypeActive:** Compares the given integer to the number of active challenges of the given Type based on the value of `greaterThanNumActive`.

Here is a code sample:
```c#
Assembly asm = typeof(MyCustomSequencer).Assembly;

NewNodeManager.New(
    "MyPluginGUID", "MyCustomNode",
    GenerationType.SpecialEvent, typeof(MyCustomSequencer),
    new List<Texture2D> {
        TextureHelper.GetImageAsTexture("animated_node_1.png", asm),
        TextureHelper.GetImageAsTexture("animated_node_2.png", asm),
        TextureHelper.GetImageAsTexture("animated_node_3.png", asm),
        TextureHelper.GetImageAsTexture("animated_node_4.png", asm)
    },
    new List<NodeData.SelectionCondition> {
        new ChallengeIsActive(AscensionChallenge.BaseDifficulty, exclude: true) // prevent generation if there are no BaseDifficulty challenges active.
    },
    new List<NodeData.SelectionCondition> {
        new NumChallengesOfTypeActive(AscensionChallenge.BaseDifficulty, 2, greaterThanNumActive: false) // force generation if there are 2 or more BaseDifficulty challenges active.
    }
);
```