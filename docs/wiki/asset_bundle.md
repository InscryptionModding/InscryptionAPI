## Asset Bundles
---
Asset bundles are how you can import your own models, textures, gameobjects and more into Inscryption.

Think of them as fancy .zip's that are supported by Unity.

### Making Asset Bundles
1. Make a Unity project. Make sure you are using 2019.4.24f1 or your models will not show in-game.
2. Install the AssetBundleBrowser package. (Window->Package Manager)
3. Select the assets you want to be in the bundle (They need to be in the hierarchy, not in a scene!)
4. At the bottom of the Inspector window you'll see a section labedled "Asset Bundle"
5. Assign a new asset bundle name (example: testbundleexample)
6. Build Asset bundles Window->AssetBundle Browser
7. Go to the output path using file explorer
8. There should be a file called 'testbundleexample' in that folder (It will not have an extension!)
9. Copy this file into your mod folder

### Loading Asset Bundles

```csharp
if (AssetBundleHelper.TryGet<GameObject>("pathToBundleFile", "nameOfPrefabInsideAssetBundle", out GameObject prefab))
{
    GameObject clone = GameObject.Instantiate(prefab);
    // Do things with gameobject!
}
```

First parameter is the path to the asset bundle that we copied to your mod folder in #9.

Second parameter is the name of the prefab or texture... etc that you changed to have the asset bundle name in #4.

Third parameter is the result of taking the object out of the asset bundle.

**NOTE**: Getting a prefab from an asset bundle does not load it into the world. You need to clone it with Instantiate! 

**NOTE 2**: If the GameObject is being created but the model isn't showing up in-game, make sure you are using Unity 2019.4.24f1 to build the asset bundle; the model will not show up otherwise!