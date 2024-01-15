## Custom/Extended Properties
---
The API implements a system of custom properties that you can add to CardInfo's, AbilityInfo's, and CardModificationInfo's, and then retrieve them as needed.

In the same way that you can use Evolve parameters to make the evolve ability work, or the Ice Cube parameters to make the IceCube ability work, this can allow you to set custom parameters to make your custom abilities work.
```c#

// adding a custom property to a CardInfo
CardInfo sample = CardLoader.CardByName("MyCustomCard");
sample.SetExtendedProperty("CustomPropertyName", "CustomPropertyValue");

string propValue = sample.GetExtendedProperty("CustomPropertyName");
```

### Reserved Properties
Some extended properties are reserved by the API for certain uses.
The following are some extension properties you can use for your cards.

If you're using C# you can set these properties using their respective setter method, and can retrieve these properties with the appropritate getter method.
For JsonLoader users, these properties can be accessed using the same method as accessing any other extended property.

**NOTE THAT THE NAMES ARE CASE-SENSITIVE.**

|Property Name          |Affected Type  |Value Type |Description                                                                            |Extension Method       |
|-----------------------|---------------|-----------|---------------------------------------------------------------------------------------|-----------------------|
|TriggersOncePerStack   |AbilityInfo    |Boolean    |If the ability should trigger twice when the card evolves.                             |SetTriggersOncePerStack|
|HideSingleStacks       |AbilityInfo    |Boolean    |If making an ability hidden should hide all of an ability's stacks or only one per.    |SetHideSingleStacks    |
|AffectedByTidalLock    |CardInfo       |Boolean    |If the card should be killed by the effect of Tidal Lock.                              |SetAffectedByTidalLock |
|TransformerCardId		|CardInfo		|String		|The name of the card this card will transform into when it has the Transformer sigil.  |SetTransformerCardId   |
|RemoveGreenGem         |CardModificationInfo|Boolean    |Removes the Green Mox from the card.                                              |RemoveGreenGemCost*    |
|RemoveOrangeGem        |CardModificationInfo|Boolean    |Removes the Green Mox from the card.                                              |RemoveOrangeGemCost*   |
|RemoveBlueGem          |CardModificationInfo|Boolean    |Removes the Green Mox from the card.                                              |RemoveBlueGemCost*     |

* You can also use RemoveGemsCost to remove multiple gems at once