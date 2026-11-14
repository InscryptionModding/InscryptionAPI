using UnityEngine;

namespace InscryptionAPI.Helpers.Extensions;

/// <summary>
/// An Extensions Class for the <see cref="GameObject"/>
/// </summary>
public static class GameObjectExtensions
{
    /// <summary>
    /// Finds the Children of a Given <see cref="GameObject"/>
    /// </summary>
    /// <param name="parent">The <see cref="GameObject"/> we want to find the children of.</param>
    /// <param name="name">The name of the child <see cref="GameObject"/> we are looking for within the children.</param>
    /// <returns>A <see cref="GameObject"/> with the name being looked for.</returns>
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
