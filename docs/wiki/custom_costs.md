## Custom Card Costs
---
The Inscryption Community Patch allows for custom card costs to be displayed in all three main acts of the game:

### Acts 1
If you want to have your card display a custom card cost in either Act 1 (Leshy's Cabin), you can simply hook into one of the following events:

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

### Act 2
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
Custom card costs in Act 3 are a little more complex due to the fact that the Disk Card model displays card costs as 3D objects instead of texture rendered on the card art. As such, you actually have a little more control over how your custom costs are displayed if you want it.

There are two custom cost events to hook into.
`Part3CardCostRender.UpdateCardCostSimple` allows you to simply provide textures for your custom cost that will be rendered onto the card for you (and as you will see, there are helpers to build those textures for you as well).
`Part3CardCostRender.UpdateCardCostComplex` gives you an opportunity to modify the `GameObject` for your card cost directly, allowing you to attach any arbitrary game objects to the card you wish.

The cost texture image must be 64x28 pixels for Act 1, or 30x8 pixels for Act 2.

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