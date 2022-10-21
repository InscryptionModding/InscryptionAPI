using UnityEngine;

namespace InscryptionAPI.Helpers.Extensions;

public static class GameObjectExtensions
{
    public static GameObject FindChild(this GameObject parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        foreach (Transform child in parent.transform)
        {
            GameObject go = child.gameObject.FindChild(name);
            if (go != null)
            {
                return go;
            }
        }

        return null;
    }
}
