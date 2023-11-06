## Opponents
---
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

If you have a specific blueprint that you want your opponent type to use, using the example for setting up the blueprint from before, then in in the `Awake`, `Start`, or `IntroSequence` override you can set it there if you didn't already set it in the EncounterData object:

```c#
public override IEnumerator IntroSequence(EncounterData encounter)
{
	encounterData.Blueprint = EncounterManager.AllEncountersCopy.Find(enc => enc.name == "TurnPlan_2");
        List<List<CardInfo>> plan = EncounterBuilder.BuildOpponentTurnPlan(encounterData.Blueprint, difficulty, removeLockedCards);
	base.ReplaceAndAppendTurnPlan(plan);
	yield return QueueNewCards();
        yield return base.IntroSequence(encounter);
}
```

## AI
---
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

## Boss Masks
---
When creating a custom boss opponent, you may feel the urge to change what mask Leshy adorns during the fight.
Fortunately, the API's got you covered.

### Changing an Existing Mask
This allows you to change a mask already added to Inscyrption. Can be any Vanilla masks or one someone else has added.

```csharp
MaskManager.Override("guid", "nameOfNewMask", LeshyAnimationController.Mask.Angler, "pathToTexture");
```
This example shows changing the mask the Angler uses to have a custom texture we are using.

NOTE: This also changes the model so we can use a texture without the fuss of UV mapping the Anglers actual mask.
If you still want to use the Anglers model then use `.SetModelType(MaskManager.ModelType.Angler)`

### Adding a Random Mask
If you want to add a new mask that will be randomly chosen when Leshy goes to put on a mask use this.
```csharp
MaskManager.AddRandom("guid", "nameOfNewMask", LeshyAnimationController.Mask.Prospector, "pathToTexture");
```
This example shows adding a new mask that when going to the Prospector boss fight, Leshy will choose between the default Prospector mask and this new one.

You can add as many random masks as you want. There is no limit.

### Adding a Custom Mask

```csharp
MaskManager.Add("guid", "nameOfNewMask", "pathToTexture");
```

### Adding a Custom Model

```csharp
ResourceLookup resourceLookup = new ResourceLookup();
resourceLookup.FromAssetBundle("pathToAssetBundle", "prefabNameInsideBundle");
MaskManager.ModelType modelType = MaskManager.RegisterPrefab("guid", "nameOfModel", resourceLookup);

var mask = MaskManager.Add("guid", "nameOfMask");
mask.SetModelType(modelType);
```

### Putting on a Mask
This will tell Leshy to push a mask on his face.

Useful for when you have your own boss sequence and you want to tell Leshy to put on your new mask!

```csharp
LeshyAnimationController.Instance.PutOnMask(LeshyAnimationController.Mask.Woodcarver, false);
```

### Adding Custom Mask Behaviour

```csharp
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        MyCustomMask.Setup();        
    }
}
```

```csharp
public class MyCustomMask : MaskBehaviour
{
    public static LeshyAnimationController.Mask ID;
    
    public static void Setup()
    {
        var mask = MaskManager.Add("guid", "nameOfNewMask", "pathToTexture");
        mask.SetMaskBehaviour(typeof(MyCustomMask));
        ID = mask.ID;
    }
}
```