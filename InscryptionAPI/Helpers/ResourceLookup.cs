using UnityEngine;

namespace InscryptionAPI.Helpers;

/// <summary>
/// A Helper Object for ResourceLookup.
/// </summary>
/// <remarks>Define how an asset should be retrieved so we can fetch it at any time.</remarks>
public class ResourceLookup : ICloneable
{
    /// <summary>
    /// The Path to the Resource.
    /// </summary>
    public string ResourcePath { get; private set; }
    /// <summary>
    /// The Resources BankID.
    /// </summary>
    public string ResourceBankID { get; private set; }
    /// <summary>
    /// The Prefab associated with the Lookup.
    /// </summary>
    public GameObject Prefab { get; private set; }

    /// <summary>
    /// Gets a Prefab from the passed <see cref="AssetBundle"/> Path and Prefab Name.
    /// </summary>
    /// <param name="assetBundlePath">The Path to the <see cref="AssetBundle"/> in which to search for the Prefab.</param>
    /// <param name="assetBundlePrefabName">The Name of the Prefab we are searching for in the <see cref="AssetBundle"/>.</param>
    public void FromAssetBundle(string assetBundlePath, string assetBundlePrefabName)
    {
        if (AssetBundleHelper.TryGet(assetBundlePath, assetBundlePrefabName, out GameObject go))
        {
            Prefab = go;
        }
    }
    /// <summary>
    /// Gets a Prefab from the passed <see cref="AssetBundle"/> and Prefab Name.
    /// </summary>
    /// <param name="assetBundle">The <see cref="AssetBundle"/> in which to search for the Prefab.</param>
    /// <param name="assetBundlePrefabName">The Name of the Prefab we are searching for in the <see cref="AssetBundle"/>.</param>
    public void FromAssetBundle(AssetBundle assetBundle, string assetBundlePrefabName)
    {
        if (AssetBundleHelper.TryGet(assetBundle, assetBundlePrefabName, out GameObject go))
        {
            Prefab = go;
        }
    }
    /// <summary>
    /// Gets a Prefab from the passed <see cref="AssetBundle"/> DLL based Path and Prefab Name.
    /// </summary>
    /// <param name="assetBundlePath">The Path to the Asset Bundle within the DLL Assembly.</param>
    /// <param name="assetBundlePrefabName">The Name of the Prefab we are searching for in the <see cref="AssetBundle"/>.</param>
    /// <typeparam name="T"></typeparam>
    public void FromAssetBundleInAssembly<T>(string assetBundlePath, string assetBundlePrefabName)
    {
        byte[] resourceBytes = TextureHelper.GetResourceBytes(assetBundlePath, typeof(T).Assembly);
        if (AssetBundleHelper.TryGet(resourceBytes, assetBundlePrefabName, out GameObject go))
        {
            Prefab = go;
        }
    }
    
    /// <summary>
    /// Sets the <see cref="ResourcePath"/> to the ResourcePath passed in.
    /// </summary>
    /// <param name="resourcePath">The Path to the Resource.</param>
    public void FromResources(string resourcePath)
    {
        this.ResourcePath = resourcePath;
    }
    /// <summary>
    /// Sets the <see cref="ResourceBankID"/> to the ResourceBankID passed in.
    /// </summary>
    /// <param name="resourceBankID">The ResourceBankID.</param>
    public void FromResourceBank(string resourceBankID)
    {
        this.ResourceBankID = resourceBankID;
    }
    /// <summary>
    /// Sets the <see cref="Prefab"/> to the Prefab passed in.
    /// </summary>
    /// <param name="prefab">The Prefab itself.</param>
    public void FromPrefab(GameObject prefab)
    {
        this.Prefab = prefab;
    }

    /// <summary>
    /// Gets the UnityObject Representative of the Prefab.
    /// </summary>
    /// <typeparam name="T">A <see cref="UnityObject"/> representative of the Prefab.</typeparam>
    /// <returns>A <see cref="UnityObject"/> if successful, a Null <see cref="UnityObject"/> if we failed to get the Type from the Prefab, and a Default(T) if the ResourceLookup failed.</returns>
    public virtual T Get<T>() where T : UnityObject
    {
        if (!string.IsNullOrEmpty(ResourcePath))
        {
            return Resources.Load<T>(ResourcePath);
        }

        if (!string.IsNullOrEmpty(ResourceBankID))
        {
            return ResourceBank.Get<T>(ResourceBankID);
        }

        if (Prefab != null)
        {
            if (Prefab.GetType() == typeof(T))
            {
                return (T)(UnityObject)Prefab;
            }
            else if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                return Prefab.GetComponent<T>();
            }

            InscryptionAPIPlugin.Logger.LogError("No way to get Type " + typeof(T) + " from prefab " + Prefab.name);
            return null;
        }

        InscryptionAPIPlugin.Logger.LogError("ResourceLookup not setup correctly!");
        return default(T);
    }

    /// <summary>
    /// Turns the ResourceLookup Object into a String.
    /// </summary>
    /// <returns>A String of the ResourceLookup Object.</returns>
    public override string ToString()
    {
        return $"ResourceLookup(ResourcePath:{ResourcePath}, ResourceBankID:{ResourceBankID}, Prefab:{Prefab})";
    }

    /// <summary>
    /// Clones the Object into a Shallow Copy of the current ResourceLookup.
    /// </summary>
    /// <returns>A Cloned Object representing the ResourceLookup.</returns>
    public object Clone()
    {
        ResourceLookup resourceLookup = (ResourceLookup)base.MemberwiseClone();
        return resourceLookup;
    }
}