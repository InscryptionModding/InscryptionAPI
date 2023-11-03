## Card Management
---
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

### Card Extensions
---
The following card extensions are available (may not be up-to-date):
- **SetPortrait:** Assigns the card's portrait art, and optionally its emissive portrait as well. You can supply Texture2D directly, or supply a path to the card's art.
- **SetEmissivePortrait:** If a card already has a portrait and you just want to modify its emissive portrait, you can use this. Note that this will throw an exception if the card does not have a portrait already.
- **SetAltPortrait:** Assigns the card's alternate portrait.
- **SetPixelPortrait:** Assigns the card's pixel portrait (for GBC mode).
- **SetCost:** Sets the cost for the card. There are also extensions for setting Blood, Bones, Energy, and Mox individually.
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
- **SetGemify:** Sets whether or not the card should be Gemified by default.
- **SetAffectedByTidalLock:** Sets whether or not the card will be killed by the effect of the Tidal Lock sigil.
- **AddAbilities:** Add any number of abilities to the card. This will add duplicates.
- **AddAppearances:** Add any number of appearance behaviors to the card. No duplicates will be added.
- **AddMetaCategories:** Add any number of metacategories to the card. No duplicates will be added.
- **AddTraits:** Add any number of traits to the card. No duplicates will be added.
- **AddTribes:** Add any number of tribes to the card. No duplicates will be added.
- **AddSpecialAbilities:** Add any number of special abilities to the card. No duplicates will be added.

## Evolve, Tail, Ice Cube, and Delayed Loading
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

NOTE: for card blueprints (see [Encounters]()), Evolve, Tail, and Ice Cube will NOT work properly if you use delayed loading.
To solve this, simply create the evolution/tail/ice cube card before the base card like so:
```c#
CardInfo myEvolveCard = CardManager.New("Example", "Evolve", "Evolve Card", 2, 5); // build "Evolve Card" first

CardManager.New("Example", "Base", "Base Card", 2, 2).SetEvolve(myEvolveCard, 1); // then create the base card
```

## Editing Existing Cards
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

## Card Appearance Behaviours
Card Appearance Behaviours are special classes that modify a card's appearance in-game.
They can be built into a CardInfo, or added to a card mid-game.

Appearance behaviours are implemented similarly to special abilities (for more information on those, see [here](ability_management.md#special-triggered-abilities)).

```c#
public readonly static CardAppearanceBehaviour.Appearance MyAppearanceID = CardAppearanceBehaviourManager.Add(MyPlugin.guid, "Special Appearance", typeof(MyAppearanceBehaviour)).Id;
```

Appearance behaviours implement the CardAppearanceBehaviour class.
There is an abstract method called ApplyAppearance that you must implement - this where you'll override the default appearance of the card.
There are also three other virtual methods: ResetAppearance, OnCardAddedToDeck, and OnPreRenderCard that give other hooks for changing the card's appearance.

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