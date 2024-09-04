## Custom Items
---
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

## Adding your New Item
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

If you want your item to appear in the rulebook in multiple Acts, use the extension `AddExtraRulebookCategories` on the returned ConsumableItemData.