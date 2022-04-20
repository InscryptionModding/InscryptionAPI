using MonoMod;
using UnityObject = UnityEngine.Object;

namespace APIPatcher;

[MonoModPatch("global::ScriptableObjectLoader`1")]
internal class patch_ScriptableObjectLoader<T> : ScriptableObjectLoader<T> where T : UnityObject
{
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
