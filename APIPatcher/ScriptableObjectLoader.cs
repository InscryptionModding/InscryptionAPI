using MonoMod;
using UnityObject = UnityEngine.Object;

namespace APIPatcher;

/// <summary>
/// Uses MonoMod to patch Unity's ScriptableObjectLoader to also use our version of the ScriptableObjectLoader seen in the Inscryption API itself.
/// </summary>
/// <typeparam name="T"></typeparam>
[MonoModPatch("global::ScriptableObjectLoader`1")]
internal class patch_ScriptableObjectLoader<T> : ScriptableObjectLoader<T> where T : UnityObject
{
    /// <summary>
    /// A List of AllData used within the overarching <see cref="ScriptableObjectLoader{T}"/> system, e.g. a List of <see cref="UnityObject"/>.
    /// </summary>
    new public static List<T> AllData
    {
        get
        {
            InscryptionAPI.InscryptionAPIPlugin.InvokeSOLEvent(typeof(T));
            LoadData();
            return allData;
        }
    }
}
