using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using System.Collections.ObjectModel;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class ChallengeManager
{
    public static readonly ReadOnlyCollection<AscensionChallengeInfo> BaseGameChallenges = new(GenBaseGameChallengs());
        
    private static Dictionary<AscensionChallenge, bool> stackableMap;

    private static Dictionary<AscensionChallenge, int> unlockLevelMap;

    internal static readonly ObservableCollection<AscensionChallengeInfo> NewInfos = new();

    public static List<AscensionChallengeInfo> GenBaseGameChallengs()
    {
        stackableMap = new();
        unlockLevelMap = new();

        List<AscensionChallengeInfo> infos = new(Resources.LoadAll<AscensionChallengeInfo>("Data/Ascension/Challenges"));

        GameObject screen = Resources.Load<GameObject>("prefabs/ui/ascension/AscensionChallengesScreen");

        InscryptionAPIPlugin.Logger.LogDebug($"Challenge manager init - loaded screen {screen}");

        GameObject challengeIconGrid = screen.transform.Find("Icons/ChallengeIconGrid").gameObject;

        GameObject topRow = challengeIconGrid.transform.Find("TopRow").gameObject;
        GameObject bottomRow = challengeIconGrid.transform.Find("BottomRow").gameObject;

        InscryptionAPIPlugin.Logger.LogDebug($"Initializing challenge data from toprow {topRow} bottomrow {bottomRow}:");

        List<AscensionChallengeInfo> retval = new();
        for (int i = 1; i <= 7; i++)
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Top icon {i}: icon = {topRow.transform.Find($"Icon_{i}")}");
            InscryptionAPIPlugin.Logger.LogDebug($"Top icon {i}: component = {topRow.transform.Find($"Icon_{i}").gameObject.GetComponent<AscensionIconInteractable>()}");
            InscryptionAPIPlugin.Logger.LogDebug($"Top icon {i}: info = {topRow.transform.Find($"Icon_{i}").gameObject.GetComponent<AscensionIconInteractable>().Info}");
            AscensionChallenge icon = topRow.transform.Find($"Icon_{i}").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;
            retval.Add(infos.First(i => i.challengeType == icon));

            InscryptionAPIPlugin.Logger.LogDebug($"Bottom icon {i}: icon = {bottomRow.transform.Find($"Icon_{i+7}")}");
            InscryptionAPIPlugin.Logger.LogDebug($"Bottom icon {i}: component = {bottomRow.transform.Find($"Icon_{i+7}").gameObject.GetComponent<AscensionIconInteractable>()}");
            InscryptionAPIPlugin.Logger.LogDebug($"TBottomop icon {i}: info = {bottomRow.transform.Find($"Icon_{i+7}").gameObject.GetComponent<AscensionIconInteractable>().Info}");

            AscensionChallenge icon2 = bottomRow.transform.Find($"Icon_{i+7}").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;

            if (icon2 == icon)
                stackableMap.Add(icon, true);
            else
                retval.Add(infos.First(i => i.challengeType == icon2));
        }

        InscryptionAPIPlugin.Logger.LogDebug($"Initializing challenge data: {String.Join(",", retval.Select(i => i.challengeType.ToString()))}");

        return retval;
    }

    public static event Func<List<AscensionChallengeInfo>, List<AscensionChallengeInfo>> ModifyChallenges;

    private static AscensionChallengeInfo CloneChallengeInfo(AscensionChallengeInfo info)
    {
        AscensionChallengeInfo retval = new();
        retval.activatedSprite = info.activatedSprite;
        retval.challengeType = info.challengeType;
        retval.description = info.description;
        retval.iconSprite = info.iconSprite;
        retval.name = info.name;
        retval.pointValue = info.pointValue;
        retval.title = info.title;
        return retval;
    }

    public static void SyncChallengeList()
    {
        List<AscensionChallengeInfo> asci = new (NewInfos);
        asci.Sort((a, b) => unlockLevelMap[a.challengeType] - unlockLevelMap[b.challengeType]);
        var challenges = BaseGameChallenges.Concat(asci).Select(x => CloneChallengeInfo(x)).ToList();
        AllInfo = ModifyChallenges?.Invoke(challenges) ?? challenges;        
    }

    static ChallengeManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(AscensionChallengeInfo))
            {
                ScriptableObjectLoader<AscensionChallengeInfo>.allData = AllInfo;
            }
        };
        NewInfos.CollectionChanged += static (_, _) =>
        {
            SyncChallengeList();
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

        stackableMap.Add(info.challengeType, stackable);
        unlockLevelMap.Add(info.challengeType, unlockLevel);

        NewInfos.Add(info);

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

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    public static void ResyncWhenTransitioningToGame()
    {
        SyncChallengeList();
    }
}