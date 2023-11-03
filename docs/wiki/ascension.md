## Adding New Challenges
---
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
---
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
---
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