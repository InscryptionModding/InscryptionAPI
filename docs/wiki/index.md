# Inscryption Modding Wiki
The Inscryption Modding API provides a large number of features to make it both possible and easier to mod the game.
This document provides explanations and examples to help you understand what everything does, as well as some other information pertaining to API features.

# Tweaks

## Card Cost Displays
Cards in Acts 1 and 2 can now display multiple costs at the same time, and cards in Act 1 can display Energy and Mox costs.

The API package also comes with a second DLL that includes a bunch of community bug fixes and quality-of-life improvements.

## Energy Drone in Act One/Kaycee's Mod
With the API installed, the energy management drone can be made available in Act 1 and in Kaycee's Mod. It will appear automatically if any cards with an energy or gem cost are in the Act 1 card pool, and can be forced to appear by modifying the configuration for the API.

You can also force these drones to appear in different areas of the game by overriding the following values.

```c#
using InscryptionCommunityPatch.ResourceManagers;

EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigEnergy = true; // Enables energy
EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigDrone = true; // Makes the drone appear
EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigMox = true; // Enables moxen management
EnergyDrone.ZoneConfigs[CardTemple.Nature].ConfigDroneMox = true; // Makes the Mox drone appear
```

At the time this README was written, the only zones where these settings will have any effect are CardTemple.Nature (Leshy's cabin) and CardTemple.Undead (Grimora's cabin).

# Core Features

## Extending Enumerations
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

## Custom Game Save Data
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

# Cards

## Card Management
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
- **SetPortrait:** Assigns the card's portrait art, and optionally its emissive portrait as well. You can supply Texture2D directly, or supply a path to the card's art.
- **SetEmissivePortrait:** If a card already has a portrait and you just want to modify its emissive portrait, you can use this. Note that this will throw an exception if the card does not have a portrait already.
- **SetAltPortrait:** Assigns the card's alternate portrait.
- **SetPixelPortrait:** Assigns the card's pixel portrait (for GBC mode).
- **SetCost:** Sets the cost for the card.
- **SetDefaultPart1Card:** Sets all of the metadata necessary to make this card playable in Part 1 (Leshy's cabin).
- **SetGBCPlayable:** Sets all of the metadata necessary to make this card playable in Part 2.
- **SetDefaultPart3Card:** Sets all of the metadata necessary to make this card playable in Part 3 (P03's cabin).
- **SetRare:** Sets all of the metadata ncessary to make this card look and play as Rare.
- **SetTerrain:** Sets all of the metadata necessary to make this card look and play as terrain.
- **SetPelt:** Sets all of the metadeta necessary to make this card look and play as a pelt card. Can optionally choose whether the card will trigger Pelt Lice's special ability.
- **SetTail:** Creates tail parameters. Note that you must also add the TailOnHit ability for this to do anything.
- **SetIceCube:** Creates ice cube parameters. Note that you must also add the IceCube ability for this to do anything.
- **SetEvolve:** Creates evolve parameters. Note that you must also add the Evolve ability for this to do anything.
- **SetOnePerDeck:** Sets whether or not the card is unique (only one copy in your deck per run).
- **SetHideAttackAndHealth:** Sets whether or not the card's Power and Health stats will be displayed or not.
- **AddAbilities:** Add any number of abilities to the card. This will add duplicates..
- **AddAppearances:** Add any number of appearance behaviors to the card. No duplicates will be added.
- **AddMetaCategories:** Add any number of metacategories to the card. No duplicates will be added.
- **AddTraits:** Add any number of traits to the card. No duplicates will be added.
- **AddTribes:** Add any number of tribes to the card. No duplicates will be added.
- **AddSpecialAbilities:** Add any number of special abilities to the card. No duplicates will be added.

## Evolve, Tail, Ice Cube, and delayed loading
It's possible that at the time your card is built, the card that you want to evolve into has not been built yet.
You can use the event handler to delay building the evolve/icecube/tail parameters of your card, or you can use the extension methods above which will handle that for you.

Note that if you use these extension methods to build these parameters, and the card does not exist yet, the parameters will come back null.
You will not see the evolve parameters until you add the evolution to the card list **and** you get a fresh copy of the card from CardLoader (as would happen in game).

```c#
CardManager.New("Example", "Base", "Base Card", 2, 2).SetEvolve("Evolve Card", 1); // "Evolve Card" hasn't been built yet
Plugin.Log.LogInfo(CardLoader.GetCardByName("Example_Base").evolveParams == null); // TRUE!

CardInfo myEvolveCard = CardManager.New("Example", "Evolve", "Evolve Card", 2, 5);

Plugin.Log.LogInfo(CardLoader.GetCardByName("Example_Base").evolveParams == null); // FALSE!
```

## Editing existing cards
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

## Custom Card Properties
The API allows you to add custom properties to a card, and then retrieve them for use inside of abilities. In the same way that you can use Evolve parameters to make the evolve ability work, or the Ice Cube parameters to make the IceCube ability work, this can allow you to set custom parameters to make your custom abilities work.

```c#

CardInfo sample = CardLoader.CardByName("MyCustomCard");
sample.SetExtendedProperty("CustomPropertyName", "CustomPropertyValue");

string propValue = sample.GetExtendedProperty("CustomPropertyName");
```

## Custom Card Costs
If you want to have your card display a custom card cost in either Act 1 (Leshy's Cabin) or Act 2 (pixel/GBC cards), you can simply hook into one of the following events:

```c#
using InscryptionCommunityPatch.Card;
using InscryptionAPI.Helpers;

Part1CardCostRender.UpdateCardCost += delegate(CardInfo card, List<Texture2D> costs)
{
    int myCustomCost = card.GetExtensionPropertyAsInt("myCustomCardCost");
    if (myCustomCost > 0)
        costs.Add(TextureHelper.GetImageAsTexture($"custom_cost_{myCustomCost}.png"));
}
```

The cost texture image must be 64x28 pixels for Act 1, or 30x8 pixels for Act 2.

## Talking Cards
This API supports creating new talking cards from scratch, without the need to load up your own Unity prefabs or anything of the sort!

All you have to do is create a class that implements the **ITalkingCard** interface, which contains the following fields:

| Field           | Type                    | Description                                                     |
|-----------------|-------------------------|-----------------------------------------------------------------|
| CardName        | string                  | The name of an existing card.                                   |
| Emotions        | List\<EmotionData>      | Your talking card's emotions.                                   |
| FaceInfo        | FaceInfo                | A bit of info about your talking card: blink rate and voice.    |
| DialogueAbility | SpecialTriggeredAbility | The special ability that controls your talking card's dialogue. |

And after that, all you need to do is register your talking card with the `TalkingCardManager.New<T>()` method as such:
```csharp
TalkingCardManager.New<ExampleClass>();
```

Additionally, as you can see from the last item in the table above, you're gonna need to create a new SpecialTriggeredAbility that controls your talking card's dialogue.

Don't worry, I'll explain exactly what you need to do! 

Fortunately for you, this API already supports creating new special abilities. Additionally, you won't have to do all the work: the base game already defines abstract classes you should inherit from when creating a special ability for your talking card:

| Act   | Class To Inherit From |
|-------|-----------------------|
| Act 1 | PaperTalkingCard      |
| Act 3 | DiskTalkingCard       |

**Additionally**, to make the task of creating Act 1 talking cards even easier for you, I've defined an abstract class that inherits from **PaperTalkingCard**, includes the **ITalkingCard** interface and adds a few small tweaks behind the scenes! It's called **CustomPaperTalkingCard**.

All you have to do is create a class that inherits from **CustomPaperTalkingCard** and implement its abstract fields as such:

```csharp
public class ExampleTalkingCard : CustomPaperTalkingCard
{
    public override string CardName => "example_ExampleCard";
    public override List<EmotionData> Emotions => new List<EmotionData>()
    {
        new EmotionData(emotion: Emotion.Neutral,
            face: "Example_Face.png",
            eyes: ("Example_EyesOpen.png", "Example_EyesClosed.png"),
            mouth: ("Example_MouthOpen.png", "Example_MouthClosed.png"),
            emission: ("Example_EyesOpenEmission.png", "_"))
    };
    public override FaceInfo FaceInfo => new FaceInfo(blinkRate: 1.5f, voiceSoundPitch: 1.1f);
    
    public override SpecialTriggeredAbility DialogueAbility => dialogueAbility;
    
    private SpecialTriggeredAbility dialogueAbility = SpecialTriggeredAbilityManager.Add(
            guid: Plugin.PluginGuid,
            abilityName: "ExampleDialogueAbility",
            behavior: typeof(ExampleTalkingCard)).Id;

    public override string OnDrawnDialogueId => "Example_OnDrawn";

    public override string OnPlayFromHandDialogueId => "Example_OnPlayFromHand";

    public override string OnAttackedDialogueId => "Example_OnAttacked";

    public override string OnBecomeSelectablePositiveDialogueId => "Example_OnSelectedPositive";

    public override string OnBecomeSelectableNegativeDialogueId => "Example_OnSelectedNegative";

    public override Dictionary<Opponent.Type, string> OnDrawnSpecialOpponentDialogueIds => new Dictionary<Opponent.Type, string>();
}
```

And, after that, all you need to do is register your new talking card with the `TalkingCardManager.New<T>()` method as such:

```csharp
TalkingCardManager.New<ExampleTalkingCard>();
```

**Note**: **CustomPaperTalkingCard** can only be used for Act 1 talking cards. If you want to make an Act 3 talking card, you're gonna have to inherit from DiskTalkingCard directly!

Below I'm going to explain a few important things about talking cards in-depth!

### EmotionData
The EmotionData class is a container for the sprites for one of your character's emotions. It has a constructor that takes Unity Sprites as parameters, and another constructor that takes the path to your image as a string for each image. 

The constructors take the following parameters:

| Parameter | Description                                                             |
|-----------|-------------------------------------------------------------------------|
| emotion   | The chosen emotion.                                                     |
| face      | A sprite for your character's face.                                     |
| eyes      | A pair of sprites for your character's eyes: open and closed.           |
| mouth     | A pair of sprites for your character's mouth: open and closed.          |
| emission  | A pair of sprites for your character's eyes' emission: open and closed. |

If you ever need to use an empty portrait texture, you can use `TalkingCardManager.EmptyPortrait`!

The string constructor also contains shorthand for an empty texture, if you need it: Just add an "\_" as your string for that given parameter. 

A guide on how to **use emotions** in your card's dialogue can be found [in this section below](#using-emotions).

### FaceInfo
The FaceInfo class contains a bit of info about your talking card: namely, blink rate, voice pitch, and the sound used for your character's voice in general.

The constructor takes the following parameters:
| Parameter       | Type   | Description                                                 |
|-----------------|--------|-------------------------------------------------------------|
| blinkRate       | float  | How often your character blinks.                            |
| voiceId         | string | Your character's "voice". Will explain more below.          |
| voiceSoundPitch | float  | Your character's voice's pitch.                             |
| customVoice     | string | A custom voice for your character. Will be explained below. |

"voiceId" can only be one of these three strings:
1. female1_voice
2. cat_voice
3. kobold_voice

Most talking cards in the game use the first and simply change the pitch.

#### Custom Voices
You can add a custom voice to your character instead of using one of the default voices. For that, all you need to is pass the path to your audio file as the "customVoice" parameter.

The supported audio formats are MP3, WAV, OGG and AIFF!

Please use a very short audio file for your voice. Typically, you want only a very short 'vowel' sound for this, since it's going to be played in rapid repetition.

If you pass anything as "customVoice", then the contents of the "voiceId" parameter will not matter.

### Dialogue Events
After looking at the example above, you might be wondering *"What's all of that DialogueId stuff about? How do I make my own dialogue events?"*. 

I'm gonna explain everything to you in detail now!

#### Dialogue Triggers
Talking cards can respond to a variety of game events. If you want your card to respond to a given event, you can override that property and return the ID of your new dialogue event.

Some properties are abstract and *must* be implemented: namely, `OnDrawnDialogueId`, `OnPlayFromHandDialogueId`, `OnAttackedDialogueId`, `OnBecomeSelectablePositiveDialogueId` and  `OnBecomeSelectableNegativeDialogueId`.

And here's a full list of triggers you can override, and a small explanation of each.

**Note**: All of these names end in "DialogueId", so I've omitted that last part of the name to be concise.

| Trigger                    | Description                                                    |
|----------------------------|----------------------------------------------------------------|
| OnDrawn                    | Plays when your card is drawn.                                 |
| OnPlayFromHand             | Plays when your card is played.                                |
| OnAttacked                 | Plays when your card is attacked.                              |
| OnBecomeSelectablePositive | Plays when your card becomes selectable for a positive effect. |
| OnBecomeSelectableNegative | Plays when your card becomes selectable for a negative effect. |
| OnSacrificed               | Plays when your card is sacrificed.                            |
| OnSelectedForDeckTrial     | Plays when your card is selected in the deck trial node.       |
| OnSelectedForCardMerge     | Plays before your card receives the sigil in the sigil node.   |
| OnSelectedForCardRemove    | Plays when your card is selected for removal.                  |

Additionally, the `OnDrawnSpecialOpponentDialogueIds` dictionary lets you add special dialogue when your card is drawn in a specific boss battle, like this:

```csharp
public override Dictionary<Opponent.Type, string> OnDrawnSpecialOpponentDialogueIds => new Dictionary<Opponent.Type, string>()
{
    { Opponent.Type.ProspectorBoss, "Example_TalkAboutProspector" }
};
```

### Creating a Dialogue Event
You can create your own dialogue events with this API's `DialogueManager.GenerateEvent()` method, like this:

```csharp
DialogueManager.GenerateEvent(
    pluginGUID: Plugin.PluginGuid,
    name: "Example_OnPlayFromHand",
    mainLines: new List<CustomLine>() { "This is a main line." },
    repeatLines: new List<List<CustomLine>>()
    {
         new List<CustomLine>() { "This is a repeat line!" },
         new List<CustomLine>() { "This is another repeat line!" }
    }
);
```

A brief explanation of each parameter:

| Field       | Description                                                                     |
|-------------|---------------------------------------------------------------------------------|
| pluginGUID  | Your mod's GUID.                                                                |
| name        | The name for your dialogue event; this is what you'll use to refer to it!       |
| mainLines   | A set of lines that plays in the very first time this event runs.               |
| repeatLines | Multiple sets of lines that are played after the first time this event has run. |

#### Dialogue Codes
A really neat feature of Inscryption's dialogue events are dialogue codes. They add a lot of life to dialogue!

The dialogue codes most relevant to talking cards will be explained below. All of these work with talking cards.

#### Wait (\[w:])
This is by far the dialogue code you'll wanna use the most. It's also the one the game itself uses the most in all of its dialogue.

The "\[w:x]" dialogue code adds a pause of x seconds before the rest of a sentence plays.

You can use it like this:
```
"Hello.[w:1] How are you?"
```
In this example, after saying "Hello.", the character waits 1 second before saying "How are you?".

The number of seconds does not have to be an integer. Using "\[w:0.2]" to wait only 0.2 seconds is valid, for example, and used often throughout the base game's dialogue.

This being said, I'd advise you not to go below \[w:0.1], as I don't know how small the number can go before issues arise. (And there's no point in going below that, anyhow.)

#### Color (\[c:])
The \[c:] dialogue code changes the color of a portion of your text.

You can use it like this:
```
"[c:R]This text is red.[c:] This text is not."
```
In this example, the part after \[c:R] is colored in the color that matches the code 'R', which is the color red, and the part after \[c:] has the default text color. You can think of this as "switching on" the colorful text mode and then switching it off.

*"But how do I know the codes for each color?"*
Fear not! Here's a comprehensive table of all available colors and their respective codes:

| Code | Color             |
|------|-------------------|
| B    | Blue              |
| bB   | Bright Blue       |
| bG   | Bright Gold       |
| blGr | Bright Lime Green |
| bR   | Bright Red        |
| brnO | Brown Orange      |
| dB   | Dark Blue         |
| dlGr | Dark Lime Green   |
| dSG  | Dark Seafoam      |
| bSG  | Glow Seafoam      |
| G    | Gold              |
| gray | Gray              |
| lGr  | Lime Green        |
| O    | Orange            |
| R    | Red               |

(For the record: These are the colors the game has available, built-in. I did not choose them. Yes, it's a very odd selection of colors.)

##### Custom Colors
I have added a way to use custom colors with dialogue codes. In place of one of the color codes in the table above, you can instead use a [hex color code](https://htmlcolorcodes.com/color-picker/), and this mod will parse the code into an usable Color for the text.

Here's an example:
```
"You must be... [w:0.4][c:#7f35e6]confused[c:][w:1].",
```
In this example, the word "confused" is colored in the color #7f35e6. Which, if you don't wanna look it up, is [this color!](https://g.co/kgs/JPHV5v)

Please note that for compatibility reasons, your hex color code **should include the '#'**.

#### Making Leshy Talk
The \[leshy:x] dialogue code makes Leshy say x. This color code is very useful for making Leshy and your card talk a bit between each other!

You can use it like this:

```
"We're all doomed.[leshy:Quiet now.][w:2]",
```

In this example, the character says "We're all doomed." and then Leshy says "Quiet now." right after. The text remains on the screen for 2 seconds.

There are a few things to note from that example:

1. You don't need to put quotation marks around the line Leshy is going to say.
2. The "Wait" dialogue code is still usable with Leshy's lines.

#### Using Emotions in Dialogue
You can change your character's emotion in their dialogue lines with the dialogue code `[e:x]`, where 'x' is the name of an emotion. You can look at the table above for the names of all the available emotions.

This mod adds patches to make the emotion names not case-sensitive, which means the following lines are all equally valid:

```
"[e:Anger]I'm angry."
"[e:anger]I'm angry."
"[e:AnGeR]I'm angry and my keyboard is acting up."
```

If you prefer, you can use the numeric value associated with an emotion instead of its name! This is perfectly valid, for example:

```
"[e:2]I'm angry."
```

# Abilities

## Ability Management
Abilities are unfortunately a little more difficult to manage than cards.
First of all, they have an attached 'AbilityBehaviour' type which you must implement.
Second, the texture for the ability is not actually stored on the AbilityInfo object itself; it is managed separately (bizarrely, the pixel ability icon *is* on the AbilityInfo object, but we won't get into all that).

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
- **SetActivated:** Sets whether or not the ability is an activated ability. Activated abilities can be clicked to trigger an effect.
- **SetPassive:** Sets whether or not the ability is a passive ability. Passive abilities don't do anything.
- **SetOpponentUsable:** Sets whether or not the ability can be used by the opponent (such as in a Totem battle).
- **SetConduit:** Sets whether or not the ability can be used to complete Circuits.
- **SetConduitCell:** Sets whether or not the ability is a conduit cell. Unsure of what this does.
- **SetCanStack:** Sets whether multiple copies of the ability will stack, activating once per copy. Optionally controls stack behaviour should a card with the ability evolve (see below).
- **SetTriggersOncePerStack:** Sets whether the ability (if it stacks) will only ever trigger once per stack. There's a...'feature' where stackable abilities will trigger twice per stack after a card evolves.

## How abilities are programmed
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

## Additional Functionality with Interfaces
The API adds a number of interfaces you can use to add additional functionality to your ability.
It also adds a new class: `ExtendedAbilityBehaviour`, which has all the interfaces already implemented for immediate use, saving you time.

### Modifying What Card Clots to Attack

To do this, you need to override RespondsToGetOpposingSlots to return true (like all RespondsToXXX overrides, you can make this conditional), and then override GetOpposingSlots to return the list of card slots that your ability wants the card to attack.
If you want to override the default slot (the one directly across from the card) instead of adding an additional card slot, you will need to override RemoveDefaultAttackSlot to return true.

### Passive Attack and Health Buffs

To do this, you need to override GetPassiveAttackBuff(PlayableCard target) or GetPassiveAttackBuff(PlayableCard target) to calculate the appropriate buffs.
These return an int representing the buff to give to 'target'.

In battle, the game will iterate across all cards on the board and check whether they should receive the buffs; this is what 'target' refers to; the current card being checked.
You will need to write the logic for determining what cards should get the buff, as well as what buff they should receive.

Note: you need to be very careful about how complicated the logic is in GetPassiveAttackBuffs and GetPassiveHealthBuffs.
These methods will be called *every frame* for *every instance of the ability!!*
If you're not careful, you could bog the game down substantially!

### TriggerWhenFaceDown
The API allows you to add custom properties to an ability, using the same methods as for adding custom properties to cards.

The API also allows you to control whether the ability's triggers will activate when the card is facedown.
You will need to override TriggerWhenFaceDown to return true.

There are also 2 other bools you can override for more control over what triggers should activate: ShouldTriggerWhenFaceDown, which controls whether vanilla triggers will activate; and ShouldTriggerCustomWhenFaceDown, which control whether custom triggers will activate.

## Additional Functionality for Activated Abilities
The API adds a new class `ExtendedActivatedAbilityBehaviour` that adds additional functionality for use when making activated abilities.

This new class changes how activated abilities are made a bit.
As an example, if you were inheriting from the vanilla ActivatedAbilityBehaviour, you would override BonesCost to set the... Bones cost.
ExtendedActivatedAbilityBehaviour moves that functionality to the virtual int StartingBonesCost, and BonesCost is now used to keep track of the total cost, modifiders included.

Put simply, to set the starting cost(s), you override StartingBonesCost, StartingEnergyCost, or StartingHealthCost.

There is also a new override IEnumerator `PostActivate()`.

The two main features of the new class are dynamic activation costs, and a new Health cost.

### Health Cost
This is easy enough to understand, it's a way of making an activated ability cost Health to use.
If the cost is equal to the card's current Health, then it will die.

You can set this by overriding StartingHealthCost.

### Dynamic Activation Costs
Using ExtendedActivatedAbilityBehaviour, you can change the cost of an activated ability during battle.

By overriding OnActivateBonesCostMod, OnActivateEnergyCostMod, or OnActivateHealthCostMod, you can make the ability's activation cost increase after it has been activated.
```c#
public class ActivateRepulsive : ExtendedActivatedAbilityBehaviour
{
    public static Ability abiliy;
    public override Ability Ability => ability;
    
    public override int StartingBonesCost => 2;
    public override int OnActivateBonesCostMod => 1;
    
    public override IEnumerator Activate()
    {
        yield return this.Effect();
    }
}
```
You can keep changing these modifier fields as well, if you want the cost to increase exponentially.

```c#
public override IEnumerator PostActivate();
{
    OnActivateBonesCostMod += 1;
}
```
You have basically no limits on how or when you can change the activation cost.

```c#
public override IEnumerator OnUpkeep(bool playerUpkeep)
{
    bonesCostMod -= 1;
}
```
This will reduce the Bones activation cost by 1 on upkeep.

Note that `bonesCostMod` was used here instead of `OnActivateBonesCostMod`; The OnActivate... fields are used for changing the cost every activation, while the ...CostMod fields are used for everything else.
Importantly, the ...CostMod fields keep track of the current cost modifier.

```c#
bonesCostMod = 0;
```
This resets the current Bones cost modifier.

An additional note to make is that you're not limited to only modifying a single cost; you can increase any cost you want, even those that aren't initially required for activation.

#### Keeping Track of It All
With the ability to modify a card's activation cost, the question of how to keep track of the new costs comes up.
While it's entirely possible you have an incredible memory and can just hoof it from there, the API provides a simpler way for you to keep track of it all by updating the Rulebook description of your ability.

When right-clicking the ability icon of a card, the API will grab the current activation costs and display them.
This is unique to each card, so don't worry about keeping track of multiple activation costs.

To tell the API what part of the Rulebook description should be modified, you must use `[sigilcost:X]` where X is the initial activation cost.
```c#
string rulebookDescription = "Pay [sigilcost:1 Bone, 1 Energy] to do a thing, then increase its activation cost by 1 Energy."

AbilityManager.New(pluginGuid, rulebookName, rulebookDescription, typeof(T), "artpath.png");
```

## Special Stat Icons
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

Note: you need to be very careful about how complicated the logic is in GetStatValues. This will be called *every frame!!* If you're not careful, you could bog the game down substantially.

## Special Triggered Abilities
Special triggered abilities are a lot like regular abilities; however, they are 'invisible' to the player (that is, they do not have icons or rulebook entries). As such, the API for these is very simple. You simply need to provide your plugin guid, the name of the ability, and the type implementing the ability, and you will be given back a wrapper containing the ID of your newly created special triggered ability.

Special triggered abilities inherit from DiskCardGame.SpecialCardBehaviour

```c#
public readonly static SpecialTriggeredAbility MyAbilityID = SpecialTriggeredAbilityManager.Add(MyPlugin.guid, "Special Ability", typeof(MySpecialTriggeredAbility)).Id;
```

And now MyAbilityID can be added to CardInfo objects.

### How special triggered abilities are programmed
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

## Card Appearance Behaviours
These behave the same as special triggered abilities from the perspective of the API.

Special triggered abilities inherit from DiskCardGame.CardAppearanceBehaviour

```c#
public readonly static CardAppearanceBehaviour.Appearance MyAppearanceID = CardAppearanceBehaviourManager.Add(MyPlugin.guid, "Special Appearance", typeof(MyAppearanceBehaviour)).Id;
```

### How appearance behaviours are programmed
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

# Custom Maps and Encounters
Unlike abilities, encounters are encoded into the game's data through a combination of enumerations and strings. For example, Opponents are enumerated by the enumeration Opponent.Type, but special sequencers and unique AI rules are represented as strings buried in each encounters data.

To create a custom encounter (for example, a custom boss fight), you will need some combination of opponents, special sequencers, and AI.

## Encounter Turn Plans
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

## Special Sequencers
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

## Opponents
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

#### Adding a Custom Model

```csharp
ResourceLookup resourceLookup = new ResourceLookup();
resourceLookup.FromAssetBundle("pathToAssetBundle", "prefabNameInsideBundle");
MaskManager.ModelType modelType = MaskManager.RegisterPrefab("guid", "nameOfModel", resourceLookup);

var mask = MaskManager.Add("guid", "nameOfMask");
mask.SetModelType(modelType);
```

#### Putting on a Mask
This will tell Leshy to push a mask on his face.

Useful for when you have your own boss sequence and you want to tell Leshy to put on your new mask!

```csharp
LeshyAnimationController.Instance.PutOnMask(LeshyAnimationController.Mask.Woodcarver, false);
```

#### Adding Custom Behaviour

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

## Adding new nodes to the map
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

### Conditional map nodes
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

# Ascension (Kaycee's Mod)

## Adding New Challenges
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

## Adding New Starter Decks
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

## Adding Custom Screens to Kaycee's Mod
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


# Custom Totem Tops
When creating custom Tribes, mostly likely you'll want it to be usable with Totems as well.
There is a default model for custom Totem Tops, but if you have a custom-made model you want to use, the API's got you covered.

If you want to add your own model for your top then you can use the example below.
```csharp
TotemManager.NewTopPiece<CustomIconTotemTopPiece>("NameOfTotem", Plugin.PluginGuid, Tribe, prefab);
```

If you are using a model that you have created then here is an example of how to use asset bundles to include it.
```csharp
if (AssetBundleHelper.TryGet("pathToAssetBundle", "nameOfPrefabInAssetBundle", out GameObject prefab))
{
    TotemManager.NewTopPiece<CustomIconTotemTopPiece>("NameOfTotem", Plugin.PluginGuid, Tribe, prefab);
}
```

## "I don't have an icon to show on my totem top!"
You will need a new class for your totem top so it doesn't look for an icon to populate from a tribe.   

```csharp
public class MyCustomTotemTopPiece : CompositeTotemPiece
{
    protected virtual string EmissionGameObjectName => "GameObjectName";
    
    public override void SetData(ItemData data)
    {
        base.SetData(data);

        // Set emissiveRenderer so the game knows what to highlight when hovering their mouse over the totem top
        emissiveRenderer = this.gameObject.FindChild(EmissionGameObjectName);
        if (emissiveRenderer != null)
        {
            emissiveRenderer = icon.GetComponent<Renderer>();
        }
        
        if (emissiveRenderer == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"emissiveRenderer not assigned to totem top!");
        }
    }
}
```

Then add your totem with your new class:
```csharp
if (AssetBundleHelper.TryGet("pathToAssetBundle", "nameOfPrefabInAssetBundle", out GameObject prefab))
{
    TotemManager.NewTopPiece<MyCustomTotemTopPiece>("NameOfTotem", Plugin.PluginGuid, Tribe, prefab);
}
```

# Custom Items
The API supports adding custom consumable items.
To create an item, you will need to create a new class that inherits from ConsumableItem.

Specify the class for your item and what happens when its used.
```csharp
public class CustomConsumableItem : ConsumableItem
{
    public override IEnumerator ActivateSequence()
    {
        base.PlayExitAnimation();
        yield return new WaitForSeconds(0.25f);
        yield return base.StartCoroutine(Singleton<ResourcesManager>.Instance.AddBones(4, null));
        yield break;
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        if (!base.ExtraActivationPrerequisitesMet())
        {
            return false;
        }

        // Optional: Stop player from using the item!
        return true;
    }
}
```

### Adding your New Item
If you don't have a custom model you can use one of the default types from ConsumableItemManager.ModelType provided by the API.

```csharp
ConsumableItemManager.ModelType modelType = ConsumableItemManager.ModelType.Basic;
ConsumableItemManager.New(Plugin.PluginGuid, "Custom Item", "Does a thing!", textureOrSprite, typeof(CustomConsumableItem), modelType)
		        .SetDescription(learnText)
		        .SetAct1();
```

If you want to create a simple 'card-in-a-bottle' type item, you can use the provided method like so:
```csharp
ConsumableItemManager.NewCardInABottle(PluginGuid, cardInfo.name)
			        .SetAct1();
```

If you have a custom model for your item you can specify it in the different constructor:
```csharp
GameObject prefab = ...
ConsumableItemManager.New(Plugin.PluginGuid, "Custom Item", "Does a thing!", textureOrSprite, typeof(CustomConsumableItem), prefab)
		        .SetDescription(learnText)
		        .SetAct1();
```

# Custom Pelts

## Pelt Management
Pelts are comprised of two components: the actual pelt card, and the pelt data.
Pelt cards are created the same way any of card is made.
The pelt data on the other hand is created using the API's PeltManager.

The pelt data is used to determine how the associated card is handled by the Trapper and Trader.
This includes how much it costs to buy, how much the price increases over a run,and what and how many cards the Trader will offer for the pelt.


## Adding a Custom Pelt
The first step is making the card.
```csharp
CardInfo bonePeltInfo = CardManager.New(PluginGuid, "Bone Pelt", "Bone Pelt", 0, 2);
bonePeltInfo.portraitTex = TextureHelper.GetImageAsTexture(Path.Combine(PluginDirectory, "Art/portrait_skin_bone.png")).ConvertTexture();
bonePeltInfo.cardComplexity = CardComplexity.Simple;
bonePeltInfo.temple = CardTemple.Nature;
bonePeltInfo.SetPelt();
```
You MUST create the card before creating the pelt data.

Once that's done, it's time to create the pelt data.
The most important complicated part of this is creating the Function that will be used to determine the cards the Trader will offer.

For this example, the Trader will only offer cards that cost Bones and are part of the Nature temple (meaning Act 1 cards only).
```csharp
Func<List<CardInfo>> cardChoices = ()
{
    return CardManager.AllCardsCopy.FindAll((CardInfo x) => x.BonesCost > 0 && x.temple == CardTemple.Nature);
};

PeltManager.CustomPeltData bonePelt = PeltManager.New(yourPluginGuid, bonePeltInfo, baseBuyPrice: 3, extraAbilitiesToAdd: 0, choicesOfferedByTrader: 8, cardChoices);
```

This pelt will now cost 3 Teeth to buy from the Trapper, and the Trader will offer you 8 cards to choose from for it; the offered cards will have 0 extra abilities added onto them as well.

The following extensions are provided for further customisation:
- **SetPluginGuid:** Sets the Guid of the plugin that's adding the pelt. Useful if you aren't using PeltManager.New() to create your pelt.
- **SetBuyPrice:** Sets the base buy price of the pelt, meaning how much it costs initially before modifiers are added. Optionally sets the max buy price for the pelt as well (default price is 20).
- **SetMaxBuyPrice:** A separate extension for only setting the max buy price of the pelt. Useful if you've already set the buy price using PeltManager.New(), for instance.
- **SetBuyPriceModifiers:** Sets the values used to determine how the price is affected by in-game events, such as the Trapper and Trader being defeated, or the Expensive Pelts challenge being active.
- **SetBuyPriceAdjustment:** Sets the Function used to determine how the price changes across a run. You can get really fancy with it, but by default it increases by 1 Tooth.
- **SetModifyCardChoiceAtTrader:** Lets you modify the cards offered by the Trader further, such as adding a decal or changing their cost.
- **SetIsSoldByTrapper:** Determines whether the pelt will be sold by the Trapper.
- **SetNumberOfCardChoices:** Sets how many cards you will able to choose from when trading the pelt.
- **SetCardChoices:** Sets the Function used to determine what potential cards the Trader will offer for the pelt.

# Sound

## Adding Music Tracks to the Gramophone
This API supports adding new tracks to the Gramophone in Leshy's Cabin.
(A user must have the Hex disc unlocked in order to be able to listen to the new tracks.)

All you need for that is a regular audio file. The API will do all of the conversion. The file should be inside the 'plugins' folder. and the supported audio formats are MP3, WAV, OGG and AIFF.

You can register your track like this:

```csharp
GramophoneManager.AddTrack(PluginGuid, "MyFile.wav", 0.5f);
```
The first parameter should be your plugin's GUID. The second parameter should be your file's name.
The third parameter is optional, and determines the volume of your track, from 0 to 1f. 


## Converting Audio Files to Unity AudioClip Objects
This API provides a helper method for converting audio files to Unity AudioClip objects so that they can be played in-game with the AudioSource component. You can use this to replace in-game music through patches, or to play your own sound effects.

The audio file should be located inside of the 'plugins' folder. The supported audio formats are MP3, WAV, OGG and AIFF.

You can convert your audio file into an AudioClip object like this:

```csharp
AudioClip audio = SoundManager.LoadAudioClip("Example.mp3");
```

# Asset Bundles
Asset bundles are how you can import your own models, texture, gameobjects and more into Inscryption.

Think of them as fancy .zip's that's supported by Unity.

## How to Make an Asset Bundle
1. Make a Unity project. Make sure you are using 2014.4.24f1 or your models will not show in-game.
2. Install the AssetBundleBrowser package. (Window->Package Manager)
3. Select the assets you want to be in the bundle (They need to be in the hierarchy, not in a scene!)
4. At the bottom of the Inspector window you'll see a section labedled "Asset Bundle"
5. Assign a new asset bundle name (example: testbundleexample)
6. Build Asset bundles Window->AssetBundle Browser
7. Go to the output path using file explorer
8. There should be a file called 'testbundleexample' in that folder (It will not have an extension!)
9. Copy this file into your mod folder

## Loading Asset Bundles

```csharp
if (AssetBundleHelper.TryGet<GameObject>("pathToBundleFile", "nameOfPrefabInsideAssetBundle", out GameObject prefab))
{
    GameObject clone = GameObject.Instantiate(prefab);
    // Do things with gameobject!
}
```

First parameter is the path to the asset bundle that we copied to your mod folder in #9

Second parameter is the name of the prefab or texture... etc that you changed to have the asset bundle name in #4

Third parameter is the result of taking the object out of the asset bundle.

NOTE: Getting a prefab from an asset bundle does not laod it into the world. You need to clone it with Instantiate! 

## Common Problems

### 1. The GameObject is being created but the model won't show up!
Make sure you are using Unity 2019.4.24f1 to build the asset bundle. The model will not show up otherwise!
