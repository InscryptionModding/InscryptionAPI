using DiskCardGame;
using HarmonyLib;
using Mono.Collections.Generic;
using UnityEngine;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class ChallengeManager
{
    public static readonly ReadOnlyCollection<AscensionChallengeInfo> BaseGameChallenges = new(Resources.LoadAll<AscensionChallengeInfo>("Data/"));
        
    private static Dictionary<AscensionChallenge, bool> stackableMap = new();

    private static Dictionary<AscensionChallenge, int> unlockLevelMap = new();

    internal static readonly List<AscensionChallengeInfo> newInfos = new();

    public static event Action<List<AscensionChallengeInfo>> ModifyAscensionChallengeList;

    static ChallengeManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(AscensionChallengeInfo))
            {
                ScriptableObjectLoader<AscensionChallengeInfo>.allData = AllInfo;
            }
        };
    }

    public static List<AscensionChallengeInfo> AllInfo = BaseGameChallenges.ToList();

    public static bool IsStackable(AscensionChallenge id)
    {
        return stackableMap.ContainsKey(id) ? stackableMap[id] : false;
    }

    public static AscensionChallengeInfo Add(string pluginGuid, AscensionChallengeInfo info, int unlockLevel=0, bool stackable=false)
    {
        info.challengeType = GuidManager.GetEnumValue<AscensionChallenge>(pluginGuid, info.title);

        newInfos.Add(info);

        ModifyAscensionChallengeList?.Invoke(newInfos);
        newInfos.Sort((a, b) => unlockLevelMap[a.challengeType] - unlockLevelMap[b.challengeType]);
        AllInfo = BaseGameChallenges.Concat(newInfos).ToList();

        stackableMap.Add(info.challengeType, stackable);
        unlockLevelMap.Add(info.challengeType, unlockLevel);

        return info;
    }

    public static AscensionChallengeInfo Add(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0, 
        bool stackable = false
    )
    {
        AscensionChallengeInfo info = ScriptableObject.CreateInstance<AscensionChallengeInfo>();
        info.title = title;
        info.challengeType = AscensionChallenge.None;
        info.description = description;
        info.pointValue = pointValue;
        info.iconSprite = TextureHelper.ConvertTexture(iconTexture, TextureHelper.SpriteType.ChallengeIcon);

        Texture2D infoActivationTexture = activatedTexture ?? 
            ((pointValue > 0 ) ? Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default")
                : Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_difficulty"));
        info.activatedSprite = TextureHelper.ConvertTexture(infoActivationTexture, TextureHelper.SpriteType.ChallengeIcon);

        return Add(pluginGuid, info, unlockLevel, stackable);
    }

    [HarmonyPatch(typeof(AscensionUnlockSchedule), "ChallengeIsUnlockedForLevel")]
    [HarmonyPostfix]
    public static void IsCustomChallengeUnlocked(ref bool __result, int level, AscensionChallenge challenge)
    {
        if (unlockLevelMap.ContainsKey(challenge))
            __result = unlockLevelMap[challenge] <= level;
    }
}