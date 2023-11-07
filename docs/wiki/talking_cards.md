## Talking Cards
---
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

A guide on how to **use emotions** in your card's dialogue can be found [in this section below](#using-emotions-in-dialogue).

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

### Custom Voices
You can add a custom voice to your character instead of using one of the default voices. For that, all you need to is pass the path to your audio file as the "customVoice" parameter.

The supported audio formats are MP3, WAV, OGG and AIFF!

Please use a very short audio file for your voice. Typically, you want only a very short 'vowel' sound for this, since it's going to be played in rapid repetition.

If you pass anything as "customVoice", then the contents of the "voiceId" parameter will not matter.

## Dialogue Events
---
After looking at the example above, you might be wondering *"What's all of that DialogueId stuff about? How do I make my own dialogue events?"*. 

I'm gonna explain everything to you in detail now!

### Dialogue Triggers
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

### Dialogue Codes
A really neat feature of Inscryption's dialogue events are dialogue codes. They add a lot of life to dialogue!

The dialogue codes most relevant to talking cards will be explained below. All of these work with talking cards.

### Wait (\[w:])
This is by far the dialogue code you'll wanna use the most. It's also the one the game itself uses the most in all of its dialogue.

The "\[w:x]" dialogue code adds a pause of x seconds before the rest of a sentence plays.

You can use it like this:
```
"Hello.[w:1] How are you?"
```
In this example, after saying "Hello.", the character waits 1 second before saying "How are you?".

The number of seconds does not have to be an integer. Using "\[w:0.2]" to wait only 0.2 seconds is valid, for example, and used often throughout the base game's dialogue.

This being said, I'd advise you not to go below \[w:0.1], as I don't know how small the number can go before issues arise. (And there's no point in going below that, anyhow.)

### Color (\[c:])
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

#### Custom Colors
I have added a way to use custom colors with dialogue codes. In place of one of the color codes in the table above, you can instead use a [hex color code](https://htmlcolorcodes.com/color-picker/), and this mod will parse the code into an usable Color for the text.

Here's an example:
```
"You must be... [w:0.4][c:#7f35e6]confused[c:][w:1].",
```
In this example, the word "confused" is colored in the color #7f35e6. Which, if you don't wanna look it up, it's [this color!](https://g.co/kgs/JPHV5v)

Please note that for compatibility reasons, your hex color code **should include the '#'**.

### Making Leshy Talk
The \[leshy:x] dialogue code makes Leshy say x. This color code is very useful for making Leshy and your card talk a bit between each other!

You can use it like this:

```
"We're all doomed.[leshy:Quiet now.][w:2]",
```

In this example, the character says "We're all doomed." and then Leshy says "Quiet now." right after. The text remains on the screen for 2 seconds.

There are a few things to note from that example:

1. You don't need to put quotation marks around the line Leshy is going to say.
2. The "Wait" dialogue code is still usable with Leshy's lines.

### Using Emotions in Dialogue
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
