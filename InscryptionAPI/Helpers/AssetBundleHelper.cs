using UnityEngine;

namespace InscryptionAPI.Helpers;

public class AssetBundleHelper
{
    public static bool TryGet<T>(string pathToAssetBundle, string prefabName, out T prefab) where T : Object
    {
        AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(pathToAssetBundle);
        if (myLoadedAssetBundle == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting asset bundle at path: '{pathToAssetBundle}' but failed! Is the path wrong?");
            prefab = default(T);
            return false;
        }

        prefab = myLoadedAssetBundle.LoadAsset<T>(prefabName);
        myLoadedAssetBundle.Unload(false);
        
        if (prefab == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab '{prefabName}' from asset bundle at path: '{pathToAssetBundle}' but failed! Is the prefab name or type wrong?");
            return false;
        }

        return true;
    }
}
