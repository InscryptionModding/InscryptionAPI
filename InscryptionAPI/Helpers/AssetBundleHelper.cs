using UnityEngine;

namespace InscryptionAPI.Helpers;

/// <summary>
/// A Helper Set related to the <see cref="AssetBundle"/>.
/// </summary>
public static class AssetBundleHelper
{
    /// <summary>
    /// A function that attempts to get a Prefab of Type T from the given <see cref="AssetBundle"/>.
    /// </summary>
    /// <param name="bundle">The <see cref="AssetBundle"/> we want to get a Prefab out of.</param>
    /// <param name="prefabName">The Name of the specific Prefab we are after.</param>
    /// <param name="prefab">The Fetched Prefab from the <see cref="AssetBundle"/>, if it fails we return Default(T), e.g. the Null associated with the given Type.</param>
    /// <typeparam name="T">The Type in which is associated with the Prefab itself.</typeparam>
    /// <returns>False if we fail, true if we successfully returned a Prefab associated with Type T.</returns>
    public static bool TryGet<T>(AssetBundle bundle, string prefabName, out T prefab) where T : UnityObject
    {
        if (bundle == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab from {prefabName} but the assetbundle is null!");
            prefab = default(T);
            return false;
        }

        prefab = bundle.LoadAsset<T>(prefabName);
        bundle.Unload(false); // Does not unload assets, just the Bundle.

        if (prefab == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab '{prefabName}' from asset bundle but failed! Is the prefab name or type wrong?");
            return false;
        }

        return true;
    }

    /// <summary>
    /// A function that attempts to get a Prefab of Type T from the given path to the <see cref="AssetBundle"/>.
    /// </summary>
    /// <param name="pathToAssetBundle">The path to the <see cref="AssetBundle"/></param>
    /// <param name="prefabName">The Name of the specific Prefab we are after.</param>
    /// <param name="prefab">The Fetched Prefab from the <see cref="AssetBundle"/>, if it fails we return Default(T), e.g. the Null associated with the given Type.</param>
    /// <typeparam name="T">The Type in which is associated with the Prefab itself.</typeparam>
    /// <returns>False if we fail, true if we successfully returned a Prefab associated with Type T.</returns>
    public static bool TryGet<T>(string pathToAssetBundle, string prefabName, out T prefab) where T : UnityObject
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(pathToAssetBundle);
        if (bundle == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting asset bundle at path: '{pathToAssetBundle}' but failed! Is the path wrong?");
            prefab = default(T);
            return false;
        }

        prefab = bundle.LoadAsset<T>(prefabName);
        bundle.Unload(false); // Does not unload assets, just the Bundle.

        if (prefab == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab '{prefabName}' from asset bundle at path: '{pathToAssetBundle}' but failed! Is the prefab name or type wrong?");
            return false;
        }

        return true;
    }

    /// <summary>
    /// A function that attempts to get a Prefab of Type T from the given Memory associated with the <see cref="AssetBundle"/>.
    /// </summary>
    /// <param name="resources">A <see cref="byte"/> array in the form of <c>byte[]</c> representing the Memory associated with a <see cref="AssetBundle"/>.</param>
    /// <param name="prefabName">The Name of the specific Prefab we are after.</param>
    /// <param name="prefab">The Fetched Prefab from the <see cref="AssetBundle"/>, if it fails we return Default(T), e.g. the Null associated with the given Type.</param>
    /// <typeparam name="T">The Type in which is associated with the Prefab itself.</typeparam>
    /// <returns>False if we fail, true if we successfully returned a Prefab associated with Type T.</returns>
    public static bool TryGet<T>(byte[] resources, string prefabName, out T prefab) where T : UnityObject
    {
        AssetBundle bundle = AssetBundle.LoadFromMemory(resources);
        if (bundle == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting asset bundle from bytes but failed! Is the path wrong?");
            prefab = default(T);
            return false;
        }

        prefab = bundle.LoadAsset<T>(prefabName);
        bundle.Unload(false); // Does not unload assets, just the Bundle.

        if (prefab == null)
        {
            InscryptionAPIPlugin.Logger.LogError($"Tried getting prefab '{prefabName}' from asset bundle from bytes' but failed! Is the prefab name or type wrong?");
            return false;
        }

        return true;
    }
}
