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
Repeat this once.

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

### Conditional Nodes
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

## Special Sequencers
---
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