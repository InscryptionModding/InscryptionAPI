using BepInEx.Logging;
using MonoMod;
using UnityObject = UnityEngine.Object;

namespace APIPatcher;

[MonoModPatch("global::Singleton`1")]
internal class patch_Singleton<T> : Singleton<T> where T : ManagedBehaviour
{
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

internal static class Singleton
{
    internal static readonly ManualLogSource SingletonLogSource = Logger.CreateLogSource("Singleton");
}
