using DiskCardGame;
using UnityEngine;

namespace APIPlugin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Obsolete("Use AbilityManager and extension methods instead", true)]
public class AbilityInfoUtils
{
    public static AbilityInfo CreateInfoWithDefaultSettings(string rulebookName, string rulebookDescription)
    {
        AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
        info.powerLevel = 0;
        info.rulebookName = rulebookName;
        info.rulebookDescription = rulebookDescription;
        info.metaCategories = new List<AbilityMetaCategory>()
        {
            AbilityMetaCategory.Part1Modular, AbilityMetaCategory.Part1Rulebook
        };

        return info;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member