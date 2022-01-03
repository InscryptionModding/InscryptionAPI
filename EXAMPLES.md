# Examples

### Modifying an Existing Card

```c#
private void ChangeWolf()
{
    List<Ability> abilities = new List<Ability> { NewTestAbility.ability };
    new CustomCard("Wolf") { baseAttack = 10, abilities = abilities };
}
```

### Custom Card

```c#
private void AddBears()
{
    // metaCategories determine the card pools
    List<CardMetaCategory> metaCategories = new List<CardMetaCategory>
    {
        CardMetaCategory.ChoiceNode,
        CardMetaCategory.Rare
    };

    List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = new List<CardAppearanceBehaviour.Appearance>
    {
        CardAppearanceBehaviour.Appearance.RareCardBackground,
        // Optional. If not provided for a talking card, it will be added automatically
        CardAppearanceBehaviour.Appearance.AnimatedPortrait
    };

    List<SpecialTriggeredAbility> specialAbilities = new List<SpecialTriggeredAbility>
    {
        // Required for talking cards!
        SpecialTriggeredAbility.TalkingCardChooser
    };

    // Load the image into a Texture2D object
    byte[] imgBytes = File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("ExampleMod.dll", ""),"Artwork/eightfuckingbears.png"));
    Texture2D tex = new Texture2D(2,2);
    tex.LoadImage(imgBytes);

    // Add the card
    NewCard.Add(
        "Eight_Bears", "8 fucking bears!", 
        32, 48, metaCategories, 
        CardComplexity.Simple, CardTemple.Nature, 
        description: "Kill this abomination please", 
        bloodCost: 3, appearanceBehaviour: appearanceBehaviour, defaultTex: tex, specialAbilities: specialAbilities
    );

    // Add the talking card behaviour. The name must be the same as the card name!
    NewTalkingCard.Add<EightBearsTalkingCard>("Eight_Bears", EightBearsTalkingCard.GetDictionary());
}
```

### Custom Ability

```c#
private NewAbility AddAbility()
{
    AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
    info.powerLevel = 0;
    info.rulebookName = "Example Ability";
    info.rulebookDescription = "Example ability which adds a PiggyBank!";

    // metaCategories determine the pools
    info.metaCategories = new List<AbilityMetaCategory> { AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part1Modular };

    // Create the 'learned' dialogue
    List<DialogueEvent.Line> lines = new List<DialogueEvent.Line>();
    DialogueEvent.Line line = new DialogueEvent.Line();
    line.text = "New abilities? I didn't authorise homebrew!";
    lines.Add(line);
    info.abilityLearnedDialogue = new DialogueEvent.LineSet(lines);

    // Load the image into a Texture2D object
    byte[] imgBytes = File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("ExampleMod.dll", ""), "Artwork/new.png"));
    Texture2D tex = new Texture2D(2, 2);
    tex.LoadImage(imgBytes);

    NewAbility ability = new NewAbility(info, typeof(NewTestAbility), tex, AbilityIdentifier.GetID(PluginGuid, info.rulebookName));
    NewTestAbility.ability = ability.ability;
    return ability;
}

public class NewTestAbility : AbilityBehaviour
{
    public override Ability Ability => ability;

    public static Ability ability;

    public override bool RespondsToResolveOnBoard()
    {
        return true;
    }

    public override IEnumerator OnResolveOnBoard()
    {
        yield return base.PreSuccessfulTriggerSequence();
        yield return new WaitForSeconds(0.2f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
        yield return new WaitForSeconds(0.25f);
        // Add piggy bank, only if there is room for it
        if (RunState.Run.consumables.Count < 3)
        {
            RunState.Run.consumables.Add("PiggyBank");
            Singleton<ItemsManager>.Instance.UpdateItems(false);
        }
        else
        {
            base.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.2f);
            Singleton<ItemsManager>.Instance.ShakeConsumableSlots(0f);
        }
        yield return new WaitForSeconds(0.2f);
        yield return base.LearnAbility(0f);
        yield break;
    }
}
```

### Creating dialogue for a talking card

```c#
public class EightBearsTalkingCard : PaperTalkingCard
{
    // Static method for easy access
    public static DialogueEvent.Speaker Speaker => (DialogueEvent.Speaker) 100;

    // Only important for multi-speaker dialogs
    public override DialogueEvent.Speaker SpeakerType => Speaker;

    // IDs should point to dictionary entries in GetDictionary().
    // Required:

    public override string OnDrawnDialogueId => "TalkingEightBearsDrawn";

    public override string OnDrawnFallbackDialogueId => "TalkingEightBearsDrawn2";

    public override string OnPlayFromHandDialogueId => "TalkingEightBearsPlayed";

    public override string OnAttackedDialogueId => "TalkingEightBearsAttacked";

    public override string OnBecomeSelectablePositiveDialogueId => "TalkingEightBearsPositiveSelectable";

    public override string OnBecomeSelectableNegativeDialogueId => "TalkingEightBearsNegativeSelectable";

    public override Dictionary<Opponent.Type, string> OnDrawnSpecialOpponentDialogueIds => new Dictionary<Opponent.Type, string>();

    // Optional:

    public override string OnSacrificedDialogueId => "TalkingEightBearsSacrificed";

    public override string OnSelectedForCardMergeDialogueId => "TalkingEightBearsMerged";

    public override string OnSelectedForCardRemoveDialogueId => "TalkingEightBearsRemoved";

    public override string OnSelectedForDeckTrialDialogueId => "TalkingEightBearsDeckTrial";

    public override string OnDiscoveredInExplorationDialogueId => "TalkingEightBearsDiscovered";

    public static Dictionary<string, DialogueEvent> GetDictionary()
    {
        Dictionary<string, DialogueEvent> events = new Dictionary<string, DialogueEvent>();
        events.Add("TalkingEightBearsDrawn", new DialogueEvent()
        {
            id = "TalkingEightBearsDrawn",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsDrawn2", new DialogueEvent()
        {
            id = "TalkingEightBearsDrawn2",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsPlayed", new DialogueEvent()
        {
            id = "TalkingEightBearsPlayed",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsAttacked", new DialogueEvent()
        {
            id = "TalkingEightBearsAttacked",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsPositiveSelectable", new DialogueEvent()
        {
            id = "TalkingEightBearsPositiveSelectable",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsNegativeSelectable", new DialogueEvent()
        {
            id = "TalkingEightBearsNegativeSelectable",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsSacrificed", new DialogueEvent()
        {
            id = "TalkingEightBearsSacrificed",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsMerged", new DialogueEvent()
        {
            id = "TalkingEightBearsMerged",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsRemoved", new DialogueEvent()
        {
            id = "TalkingEightBearsRemoved",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsDeckTrial", new DialogueEvent()
        {
            id = "TalkingEightBearsDeckTrial",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        events.Add("TalkingEightBearsDiscovered", new DialogueEvent()
        {
            id = "TalkingEightBearsDiscovered",
            speakers = new List<DialogueEvent.Speaker>() { Speaker },
            mainLines = new DialogueEvent.LineSet()
            {
                lines = new List<DialogueEvent.Line>()
                {
                    new DialogueEvent.Line { text = "*Bear Noises*" }
                }
            }
        });
        return events;
    }
}
```

### Creating card with an animated portrait

```c#
void AddTalkingCardToCardPool()
{
    var animatedBehaviour = new List<CardAppearanceBehaviour.Appearance>()
    {
        CardAppearanceBehaviour.Appearance.AnimatedPortrait
    };

    var specialTriggeredAbilities = new List<SpecialTriggeredAbility>()
    {
        SpecialTriggeredAbility.TalkingCardChooser
    };

    // Name of the card must match the class that inherits a type of TalkingCard.
    // For example, TalkingCardTest inherits StoatTalkingCard, 
    //  which inherits PaperTalkingCard, which inherits Talking Card
    const string talkingCardName = "TalkingCardTest";
    NewTalkingCard.Add<TalkingCardTest>(talkingCardName);

    // Place the png files inside the \BepInEx\plugins\ directory.
    // The code will search in the base plugins directory and sub-directories until the file is found 
    GameObject animatedPortraitObj = NewTalkingCard.CreateTalkingCardAnimation(
        facePng: "testingcard_character_face.png",
        eyesOpenPng: "talkingcard_eyes_open1.png", eyesClosedPng: "talkingcard_eyes_closed1.png",
        mouthOpenPng: "talkingcard_mouth_open1.png", mouthClosedPng: "talkingcard_mouth_closed1.png");

    NewCard.Add(
        talkingCardName, "Talking Card Test", 1, 1,
        CardUtils.getNormalCardMetadata, CardComplexity.Simple, CardTemple.Nature,
        appearanceBehaviour: animatedBehaviour, specialAbilities: specialTriggeredAbilities,
        animatedPortrait: animatedPortraitObj
    );
}

// this class inherits StoatTalkingCard, which inherits PaperTalkingCard, which inherits Talking Card
public class TalkingCardTest : StoatTalkingCard
{
}
```