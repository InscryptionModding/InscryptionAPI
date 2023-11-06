## Custom Totem Tops
---
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