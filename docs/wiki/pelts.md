## Custom Pelts
---
The pelts bought and sold by the Trapper and Tradder are comprised of two components: the CardInfo and the PeltData.

The CardInfo represents the actual card that you can obtain and sell, and is created using the CardManager the same way as any regular card.  The PeltData on the other hand is created using the API's PeltManager.

The PeltData determines how the CardInfo is handled by the Trapper and Trader.  This includes how much it costs to buy, how much the price increases over a run,and what and how many cards the Trader will offer for the pelt.

### Adding a Custom Pelt
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

### Pelt Extensions
---
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