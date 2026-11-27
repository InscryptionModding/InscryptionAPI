using BepInEx.Logging;
using MonoMod;
using UnityObject = UnityEngine.Object;

namespace APIPatcher;

/// <summary>
/// Uses MonoMod to patch the Singleton for later usability by the Inscryption API.
/// </summary>
/// <typeparam name="T">A ManagedBehaviour associated with the Singleton System.</typeparam>
[MonoModPatch("global::Singleton`1")]
internal class patch_Singleton<T> : Singleton<T> where T : ManagedBehaviour
{
    /// <summary>
    /// A simple function used to Find a Specified UnityObject of the specified Type within the Active Scene, if it fails it tosses a Warning.
    /// </summary>
    new protected static void FindInstance()
    {
        if (m_Instance == null)
        {
            T @object = UnityObject.FindObjectOfType<T>();
            if (@object == null)
            {
                Singleton.SingletonLogSource.LogWarning($"Got null in Singleton<{typeof(T).FullName}>.FindInstance");
            }
            m_Instance = @object;
        }
    }
}

/// <summary>
/// A simple class object for singletons in correlation with the API.
/// </summary>
internal static class Singleton
{
    /// <summary>
    /// A ManualLogSource specifically for logging issues related to Singletons.
    /// </summary>
    internal static readonly ManualLogSource SingletonLogSource = Logger.CreateLogSource("Singleton");
}
