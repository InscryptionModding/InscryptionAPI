using UnityEngine;

namespace InscryptionAPI.Helpers;

public static class AssetBundleHelper
{
    public static bool TryGet<T>(AssetBundle bundle, string prefabName, out T prefab) where T : UnityObject
    {
        if (bundle == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab from {prefabName} but the assetbundle is null!");
            prefab = default(T);
            return false;
        }

        // Get object from bundle
        prefab = bundle.LoadAsset<T>(prefabName);

        // Unload bundle but don't unload the assets
        bundle.Unload(false);

        if (prefab == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab '{prefabName}' from asset bundle but failed! Is the prefab name or type wrong?");
            return false;
        }

        return true;
    }

    public static bool TryGet<T>(string pathToAssetBundle, string prefabName, out T prefab) where T : UnityObject
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(pathToAssetBundle);
        if (bundle == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting asset bundle at path: '{pathToAssetBundle}' but failed! Is the path wrong?");
            prefab = default(T);
            return false;
        }

        // Get object from bundle
        prefab = bundle.LoadAsset<T>(prefabName);

        // Unload bundle but don't unload the assets
        bundle.Unload(false);

        if (prefab == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab '{prefabName}' from asset bundle at path: '{pathToAssetBundle}' but failed! Is the prefab name or type wrong?");
            return false;
        }

        return true;
    }

    public static bool TryGet<T>(byte[] resources, string prefabName, out T prefab) where T : UnityObject
    {
        AssetBundle bundle = AssetBundle.LoadFromMemory(resources);
        if (bundle == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting asset bundle from bytes but failed! Is the path wrong?");
            prefab = default(T);
            return false;
        }

        // Get object from bundle
        prefab = bundle.LoadAsset<T>(prefabName);

        // Unload bundle but don't unload the assets
        bundle.Unload(false);

        if (prefab == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab '{prefabName}' from asset bundle from bytes' but failed! Is the prefab name or type wrong?");
            return false;
        }

        return true;
    }
}
