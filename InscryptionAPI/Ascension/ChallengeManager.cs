using System.Collections.ObjectModel;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class ChallengeManager
{
    public static readonly ReadOnlyCollection<AscensionChallengeInfo> BaseGameChallenges = new(GenBaseGameChallenges());

    private static Dictionary<AscensionChallenge, bool> _stackableMap;

    private static Dictionary<AscensionChallenge, int> _unlockLevelMap;

    internal static readonly ObservableCollection<AscensionChallengeInfo> NewInfos = new();

    public static List<AscensionChallengeInfo> GenBaseGameChallenges()
    {
        _stackableMap = new();
        _unlockLevelMap = new();

        List<AscensionChallengeInfo> infos = new(Resources.LoadAll<AscensionChallengeInfo>("Data/Ascension/Challenges"));

        GameObject screen = Resources.Load<GameObject>("prefabs/ui/ascension/AscensionChallengesScreen");

        GameObject challengeIconGrid = screen.transform.Find("Icons/ChallengeIconGrid").gameObject;

        GameObject topRow = challengeIconGrid.transform.Find("TopRow").gameObject;
        GameObject bottomRow = challengeIconGrid.transform.Find("BottomRow").gameObject;

        List<AscensionChallengeInfo> returnValue = new();
        for (int i = 1; i <= 7; i++)
        {
            AscensionChallenge icon = topRow.transform.Find($"Icon_{i}").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;
            returnValue.Add(infos.First(aci => aci.challengeType == icon));

            AscensionChallenge icon2 = bottomRow.transform.Find($"Icon_{i + 7}").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;

            if (icon2 == icon)
                _stackableMap.Add(icon, true);
            else
                returnValue.Add(infos.First(aci => aci.challengeType == icon2));
        }

        // Can't forget the 15th challenge
        AscensionChallenge iconFinal = bottomRow.transform.Find($"Icon_15").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;
        returnValue.Add(infos.First(i => i.challengeType == iconFinal));

        return returnValue;
    }

    public static event Func<List<AscensionChallengeInfo>, List<AscensionChallengeInfo>> ModifyChallenges;

    private static AscensionChallengeInfo CloneChallengeInfo(AscensionChallengeInfo info)
    {
        AscensionChallengeInfo returnValue = new()
        {
            activatedSprite = info.activatedSprite,
            challengeType = info.challengeType,
            description = info.description,
            iconSprite = info.iconSprite,
            name = info.name,
            pointValue = info.pointValue,
            title = info.title
        };
        return returnValue;
    }

    public static void SyncChallengeList()
    {
        List<AscensionChallengeInfo> asci = new(NewInfos);
        asci.Sort((a, b) => _unlockLevelMap[a.challengeType] - _unlockLevelMap[b.challengeType]);
        var challenges = BaseGameChallenges.Concat(asci).Select(CloneChallengeInfo).ToList();
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
        return _stackableMap.ContainsKey(id) && _stackableMap[id];
    }

    public static AscensionChallengeInfo Add(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, bool stackable = false)
    {
        info.challengeType = GuidManager.GetEnumValue<AscensionChallenge>(pluginGuid, info.title);

        _stackableMap.Add(info.challengeType, stackable);
        _unlockLevelMap.Add(info.challengeType, unlockLevel);

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
        info.iconSprite = iconTexture.ConvertTexture(TextureHelper.SpriteType.ChallengeIcon);

        Texture2D infoActivationTexture = activatedTexture
            ?? ((pointValue > 0)
                ? Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default")
                : Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_difficulty"));
        info.activatedSprite = infoActivationTexture.ConvertTexture(TextureHelper.SpriteType.ChallengeIcon);

        return Add(pluginGuid, info, unlockLevel, stackable);
    }

    [HarmonyPatch(typeof(AscensionUnlockSchedule), "ChallengeIsUnlockedForLevel")]
    [HarmonyPostfix]
    public static void IsCustomChallengeUnlocked(ref bool __result, int level, AscensionChallenge challenge)
    {
        if (_unlockLevelMap.ContainsKey(challenge))
            __result = _unlockLevelMap[challenge] <= level;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    public static void ResyncWhenTransitioningToGame()
    {
        SyncChallengeList();
    }
}
