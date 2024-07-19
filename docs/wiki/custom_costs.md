## Custom Card Costs
---
In Inscryption, the player will encounter four different resources as they play: Blood, Bones, Energy, and Mox/Gems.
These resources are needed in order to play cards with the corresponding card cost, with most cards costing at least one of these four costs.
With the API, it's possible to create cards that cost multiple types of resources, giving modders greater creativity when creating cards.

There may come a time where the base costs don't meet your needs. Or maybe you just want to make a new cost.
Regardless of reason, the API provides a few ways of creating a basic card cost using the CardCostManager.

The community patches contain a TestCost class that implements many of the features this page will cover.

### Creating a New Cost
To create a new card cost, you need to create a new class that inherits from the API's [CustomCardCost](InscryptionAPI.CardCosts.CustomCardCost) class.

The CustomCardCost class provides you with the basic functionality to ensure your cost works.
Note that the API does not provide support for custom cost resources, so you will need to create the logic for it yourself through patching.

Once you have created your cost's class, you need to register it with the API.
An example for all this can be found below:
```c#
// for simplicity, this cost replicates the logic by the energy cost
public class MyCardCost : CustomCardCost
{
    // this is a required field, and should be equal to the name you pass into the API when registering your cost
    public override string CostName => "TestCost";

    // whether or not this cost's price has been satisfied by the card
    public override bool CostSatisfied(int cardCost, PlayableCard card)
    {
        // if the player has enough energy to pay the cost
	// takes the vanilla energy cost into account
        return cardCost <= (ResourcesManager.Instance.PlayerEnergy - card.EnergyCost);
    }

    // the dialogue that's played when you try to play a card with this cost, and CostSatisfied is false
    public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
    {
        return $"Eat your greens aby. {card.Info.DisplayedNameLocalized}";
    }

    // this is called after a card with this cost resolves on the board
    // if your cost spends a resource, this is where you'd put that logic
    public override IEnumerator OnPlayed(int cardCost, PlayableCard card)
    {
        // reduce the player's current energy by the card's cost
        yield return ResourcesManager.Instance.SpendEnergy(cardCost);
    }
}

public void AddCost()
{
    // when registering your card, you need to provide 2 Func's: one for grabbing the cost texture in the 3D Acts, and one for grabbing the pixel texture in Act 2
    // if your cost is exclusive to one part of the game, you can pass in null for the appropriate Func.
    CardCostManager.Register("api", "TestCost", typeof(TestCost), TextureMethod, PixelTextureMethod);
}
```
When registering your cost, you can pass a static method for the Func's instead of creating a delegate.
These methods MUST have 3 parameters types: int, CardInfo, PlayableCard; and return a Texture2D.

The int parameter represents the card's cost, and the CardInfo and PlayableCard parameters represent the current CardInfo and PlayableCard being checked. Note that the PlayableCard parameter can be null

```c#
public static Texture2D TextureMethod(int cardCost, CardInfo info, PlayableCard card)
{
    return TextureHelper.GetImageAsTexture($"myCost_{cardCost}");
}

public static Texture2D PixelTextureMethod(int cardCost, CardInfo info, PlayableCard card)
{
    return TextureHelper.GetImageAsTexture($"myCost_pixel_{cardCost}");

    // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
    // return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("myCost_pixel_7x8"));
}
```

Card cost textures vary based on what part of the game the cost will be displayed in.
For Act 1, textures must be 64x28. In Act 2 they can be 30x8 at maximum or 7x8 (see above). Act 3 has no set size but they should be no larger than 300x78.

### Negative Costs, Cost Tier, and Turn 2 Play
The API allows you to define whether or not negative cost values should be considered in the game.
This is false by default, meaning negative costs on cards will be read as 0 by the API (the actual assigned value is not changed).

You can use SetCanBeNegative to change this value, or directly modify the CanBeNegative field.

Cost tier is an integer denoting how expensive a card is, with each cost having its own formula that adds to the tier.
From example, the formula for Bones' tier is (amount / 3), rounded down.

By default, custom costs are not accounted when determining a card's cost tier;
this can be fixed using SetCostTier to define the function to use.

```c#
public static void Init()
{
    FullCardCost cost = CardCostManager.Register(InscryptionAPIPlugin.ModGUID, "TestCost", typeof(TestCost), Texture3D, TexturePixel);
    
    cost.SetCostTier(CostTier);
}

public static int CostTier(int amount)
{
    return Mathf.FloorToInt(amount / 2f);
}
```

A vital part of Inscryption's gameplay is the fair hand mechanic;
when a battle starts, the game will give you at least one card that can be played immediately, as well as a card that can be played by the second turn.

By default, when the game checks if a card with custom costs can be played by turn 2, it will return 2 - even if it can't be.

To fix this, you'll need to set your cost's CanBePlayedByTurn2WithHand function (long name, I know):
```c#
public static void Init()
{
    CardCostManager.FullCardCost fullCardCost = CardCostManager.Register(InscryptionAPIPlugin.ModGUID, "TestCost", typeof(TestCost), Texture3D, TexturePixel);
    fullCardCost.SetCanBePlayedByTurn2WithHand(CanBePlayed);
}

// amount is the card's cost amount, and hand is the list of cards in the player's hand
public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
{
    // TestCost is just a copy of Energy, so any card that costs 2 or less will be playable by turn 2
    return amount <= 2;
}
```

### Cost Choice Nodes
If you want your cost to be chooseable in Act 1 at cost choice nodes, the API provides a method to do so:
```c#
public static void Init()
{
    CardCostManager.FullCardCost fullCardCost = CardCostManager.Register(InscryptionAPIPlugin.ModGUID, "TestCost", typeof(TestCost), Texture3D, TexturePixel);

    // if true, assigns a custom ResourceType to the cost, letting it be found at cost choice nodes.
    // rewardBack is the texture that will be displayed at the choice node, and is 125 x 190 in size
    fullCardCost.SetFoundAtChoiceNodes(isChoice: true, rewardBack: (Texture2D)ResourceBank.Get<Texture>("Art/Cards/RewardBacks/card_rewardback_bird"));
}
```

Looking at the code above, you may be wondering what a ResourceType is.
To put it simply, the ResourceType is used by the game to determine what costs will be offered to the player, and then is used to determine what cards the player can receive.

#### Different Resource Amounts
Alongside a ResourceType, the game can further distinguish between different amounts of that resource when offering choices.
By default, a custom cost at the choice node will give you any valid card with that cost, regardless of how many resources it needs.

This is similar to how Bones and Energy are handled, but what if you want your cost to be handled like Blood?
Being able to choose being a card that costs 1, 2, or 3 of your cost?
Well, all you need to do is use a variant of SetFoundAtChoiceNodes like so:

```c#
public static void Init()
{
    CardCostManager.FullCardCost fullCardCost = CardCostManager.Register(InscryptionAPIPlugin.ModGUID, "TestCost", typeof(TestCost), Texture3D, TexturePixel);

    // in this version of the method, you must provide a Func instead of a Texture2D.
    // this Func will be used to determine the correct rewardback texture to use for each different valid amount.
    fullCardCost.SetFoundAtChoiceNodes(isChoice: true, rewardBackFunc: GetRewardBack, 1, 2, 4);

    // next time we go to a cost choice node, we may be able to choose between cards that cost 1, 2, or 4 TestCost.
}
```

Alternatively, you can set the FullCardCost object's 'ChoiceAmounts' field to an integer array containing the amounts.

#### Grouping Different Costs
In the event you're adding multiple custom costs to the game, you may want to group them all into a single choice.
Maybe they're all related to each other, or you don't want to push out other choices at the node.
Either way, the API offers this functionality.

```c#
private void Example()
{
    FullCardCost cost = CardCostManager.Register(...);
    cost.ChoiceAmounts = new int[] { 1, 4, 7 };
}
```

In order to group custom costs together, they must share the same ResourceType value.
```c#
public static void Init()
{
    // firstly, we'll want to mark one of our costs as chooseable
    CardCostManager.FullCardCost fullCardCost = CardCostManager.Register(InscryptionAPIPlugin.ModGUID, "TestCost", typeof(TestCost), Texture3D, TexturePixel);
    fullCardCost.SetFoundAtChoiceNodes(isChoice: true, rewardBack: (Texture2D)ResourceBank.Get<Texture>("Art/Cards/RewardBacks/card_rewardback_bird"));

    // then when creating subsequent costs, we want to set their ResourceType to the same one being used by our first cost
    CardCostManager.FullCardCost fullCardCost2 = CardCostManager.Register(InscryptionAPIPlugin.ModGUID, "TestCost2", typeof(TestCost2), Texture3D2, TexturePixel2);
    fullCardCost2.ResourceType = fullCardCost.ResourceType;
}
```

Note that grouped costs are incompatible with multiple-amount costs, meaning if one of the grouped costs has different possible choice amounts defined, they will be ignored.

### Adding Costs to Cards
Custom costs are added to cards using the API's extended properties system and can be accessed the same way.
For clarity of purpose, the API provides some extension methods for setting a CardInfo's custom cost:
```c#
public void AddCard()
{
    CardInfo info = CardManager.New("myMod", "custom_card", "Card", 1, 1);
    info.SetCustomCost("TestCost", 1);

    int cost = info.GetCustomCost("Test") // equal to 1
}
```

### Custom Costs for Death Cards
Death cards aren't technically cards; they're card mods that are added to a template card.

Because of this, simply adding an extended property to them won't work, since properties apply to ALL copies of the card.
If you want to create a death card that uses a custom play cost, you'll need to create a new card and then add properties to that new CardInfo.

Fortunately, the API's DeathCardManager is here to handle all this.
CreateCustomDeathCard() will return a new CardInfo that will represent your custom death card, using the data from the CardModificationInfo you give it to set the card's name, stats, etc..
```c#
private void AddCustomDeathCard()
{
    CardModificationInfo deathCardMod = new CardModificationInfo(2, 2)
        .SetNameReplacement("Mabel").SetSingletonId("wstl_mabel")
        .SetBonesCost(2).AddAbilities(Ability.SplitStrike)
        .SetDeathCardPortrait(CompositeFigurine.FigurineType.SettlerWoman, 5, 2)
        .AddCustomCostId("CustomCost", 1);

    // you can then add your newly created death card to the list of default death card mods like so
    DeathCardManager.AddDefaultDeathCard(deathCardMod);
}
```

## Further Functionality
In older versions of the API, adding custom costs (or their textures at least) was handled by the community patches.

These methods still exist for use, and this section will go over how to use them to add cost textures the old way.

```c#
using InscryptionCommunityPatch.Card;
using InscryptionAPI.Helpers;

Part1CardCostRender.UpdateCardCost += delegate(CardInfo card, List<Texture2D> costs)
{
    int myCustomCost = card.GetExtensionPropertyAsInt("myCustomCardCost") ?? 0; // GetExtensionPropertyAsInt can return null, so remember to check for that
    if (myCustomCost > 0)
        costs.Add(TextureHelper.GetImageAsTexture($"custom_cost_{myCustomCost}.png"));
}
```

For adding custom costs to Act 2, you have two main ways of going about it:
```c#
using InscryptionCommunityPatch.Card;
using InscryptionAPI.Helpers;

// if you want the API to handle adding stack numbers, provide a - 7x8 - texture representing your cost's icon.
Part2CardCostRender.UpdateCardCost += delegate(CardInfo card, List<Texture2D> costs)
{
    int myCustomCost = card.GetExtensionPropertyAsInt("myCustomCardCost_pixel") ?? 0;
    if (myCustomCost > 0)
    {
        Texture2D customCostTexture = TextureHelper.GetImageAsTexture($"custom_cost_pixel.png");
        costs.Add(Part2CardCostRender.CombineIconAndCount(myCustomCost, customCostTexture));
    }
}

// if you want more control over your cost's textures, or don't want to use stack numbers, provide a - 30x8 - texture for your custom cost.
Part2CardCostRender.UpdateCardCost += delegate(CardInfo card, List<Texture2D> costs)
{
    int myCustomCost = card.GetExtensionPropertyAsInt("myCustomCardCost_pixel") ?? 0;
    if (myCustomCost > 0)
    {
        costs.Add(TextureHelper.GetImageAsTexture($"custom_cost_{myCustomCost}_pixel.png"));
    }
}
```
### Act 3
Card costs in Act 3 are a tad more complex since Act 3 cards display costs as 3D objects rather than simply flat textures.

In practice, this means that you have more control over a cost's appearance than in other acts.


There are two custom cost events to hook into:
`Part3CardCostRender.UpdateCardCostSimple` provides a quick way to provide/modify the cost textures that will be rendered on the card.
`Part3CardCostRender.UpdateCardCostComplex` provides more advanced functionality, letting you modify a custom cost's `GameObject` directly, allowing you to attach any arbitrary amount of further objects/components as you wish.

#### Basic Card Cost
If you just want to display a basic card cost, all you need to do is prepare an icon that represents a single unit of your custom cost. For example, if you are building a custom cost for currency, you need an icon that represents spending one currency (for example, a `$` symbol). The width and height of the icon are up to you, but keep in mind that they will be placed onto a region that is 300x73 pixels, so the height should not exceed 73 pixels, and the wider the icon is, the fewer will fit on the space.

The `Part3CardCostRender.GetIconifiedCostTexture` helper method accepts one of these icons as well as a cost value and generates a pair of textures displaying that total cost. The first texture is the default texture (albedo) and the second is an emissive texture. If there is enough space in the 300x73 region to repeat the icon enough times to display the cost, it will do so. Otherwise, it will render a 7-segement display.

In the "currency cost" example, where the icon is the `$` symbol:
- A cost of 3 would be rendered as `$$$`
- A cost of 10 would be rendered as `$ x10`

From here, you just need to hook into the event. Each custom cost is represented by a `CustomCostRenderInfo` object, which holds the textures and game objects that will become the cost rendering on the card. Each of these objects needs a unique identifier so the can be found later. In this simple example, that identifier isn't particularly useful because we won't use the second part of the event, but it's still required.

```C#
using InscryptionCommunityPatch.Card;
using InscryptionAPI.Helpers;

MyIconTexture = TextureHelper.GetImageAsTexture("cost_icon.png");
Part3CardCostRender.UpdateCardCostSimple += delegate(CardInfo card, List<Part3CardCostRender.CustomCostRenderInfo> costs)
{
    int myCustomCost = card.GetExtensionPropertyAsInt("myCustomCardCost");
    costs.add(new ("MyCustomCost", Part3CardCostRender.GetIconifiedCostTexture(MyIconTexture, myCustomCost)));
}
```

### Advanced Card Costs
You can also directly modify the card costs by adding new game objects to them, using the `UpdateCardCostComplex` event. This gives you complete creative freedom, but does require you to truly understand the Unity engine to be able to make it work.

To do this, don't add any textures to the render info during the first event. You can then access the `GameObject` during the second event and do whatever you want:

```C#
using InscryptionCommunityPatch.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

Part3CardCostRender.UpdateCardCostSimple += delegate(CardInfo card, List<Part3CardCostRender.CustomCostRenderInfo> costs)
{
    costs.add(new ("MyCustomCost"));
}

Part3CardCostRender.UpdateCardCostComplex += delegate(CardInfo card, List<Part3CardCostRender.CustomCostRenderInfo> costs)
{
    GameObject costObject = costs.Find(c => c.name.Equals("MyCustomCost")).CostContainer;
    // Now I can add things to the cost object directly.
}
```