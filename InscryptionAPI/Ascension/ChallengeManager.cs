using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using System.Collections.ObjectModel;
using System.Collections;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class ChallengeManager
{
    public class FullChallenge
    {
        public AscensionChallengeInfo Info { get; set; }
        public int UnlockLevel { get; set; }
        public bool Stackable
        {
            get
            {
                return AppearancesInChallengeScreen > 1;
            }
            set
            {
                if(value != Stackable)
                {
                    AppearancesInChallengeScreen = value ? 2 : 1;
                }
            }
        }
        public int AppearancesInChallengeScreen { get; set; }
        public Type Handler { get; set; }
    }

    public static readonly ReadOnlyCollection<AscensionChallengeInfo> BaseGameChallenges = new(GenBaseGameChallengs());

    private static Dictionary<AscensionChallenge, bool> stackableMap;

    //private static Dictionary<AscensionChallenge, int> unlockLevelMap;

    internal static readonly ObservableCollection<FullChallenge> NewInfos = new();

    public static readonly Sprite DEFAULT_ACTIVATED_SPRITE = TextureHelper.ConvertTexture(
        Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"),
        TextureHelper.SpriteType.ChallengeIcon);

    public static List<AscensionChallengeInfo> GenBaseGameChallengs()
    {
        stackableMap = new();
        //unlockLevelMap = new();

        List<AscensionChallengeInfo> infos = new(Resources.LoadAll<AscensionChallengeInfo>("Data/Ascension/Challenges"));

        GameObject screen = Resources.Load<GameObject>("prefabs/ui/ascension/AscensionChallengesScreen");

        GameObject challengeIconGrid = screen.transform.Find("Icons/ChallengeIconGrid").gameObject;

        GameObject topRow = challengeIconGrid.transform.Find("TopRow").gameObject;
        GameObject bottomRow = challengeIconGrid.transform.Find("BottomRow").gameObject;

        List<AscensionChallengeInfo> retval = new();
        for (int i = 1; i <= 7; i++)
        {
            AscensionChallenge icon = topRow.transform.Find($"Icon_{i}").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;
            retval.Add(infos.First(i => i.challengeType == icon));

            AscensionChallenge icon2 = bottomRow.transform.Find($"Icon_{i + 7}").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;

            if (icon2 == icon)
                stackableMap.Add(icon, true);
            else
                retval.Add(infos.First(i => i.challengeType == icon2));
        }

        // Can't forget the 15th challenge
        AscensionChallenge iconFinal = bottomRow.transform.Find($"Icon_15").gameObject.GetComponent<AscensionIconInteractable>().Info.challengeType;
        retval.Add(infos.First(i => i.challengeType == iconFinal));

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
        List<AscensionChallengeInfo> asci = new List<FullChallenge>(NewInfos).ConvertAll((x) => x.Info);
        //asci.Sort((a, b) => unlockLevelMap[a.challengeType] - unlockLevelMap[b.challengeType]);
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
        return stackableMap.ContainsKey(id) ? stackableMap[id] : false || NewInfos.ToList().Exists(x => x != null && x.Info != null && x.Info.challengeType == id && x.Stackable);
    }

    public static AscensionChallengeInfo AddSpecific(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, int numAppearancesInChallengeScreen = 1)
    {
        return AddSpecific(pluginGuid, info, unlockLevel, null, numAppearancesInChallengeScreen);
    }

    public static AscensionChallengeInfo AddSpecific<T>(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, int numAppearancesInChallengeScreen = 1) where T : ChallengeBehaviour
    {
        return AddSpecific(pluginGuid, info, unlockLevel, typeof(T), numAppearancesInChallengeScreen);
    }

    public static AscensionChallengeInfo Add(string pluginGuid, AscensionChallengeInfo info, int unlockLevel=0, bool stackable=false)
    {
        return Add(pluginGuid, info, unlockLevel, null, stackable);
    }

    public static AscensionChallengeInfo Add<T>(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, bool stackable = false) where T : ChallengeBehaviour
    {
        return Add(pluginGuid, info, unlockLevel, typeof(T), stackable);
    }

    public static AscensionChallengeInfo Add(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, Type handlerType = null, bool stackable = false)
    {
        return AddSpecific(pluginGuid, info, unlockLevel, handlerType, stackable ? 2 : 1);
    }

    public static AscensionChallengeInfo AddSpecific(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, Type handlerType = null, int numAppearancesInChallengeScreen = 1)
    {
        info.challengeType = GuidManager.GetEnumValue<AscensionChallenge>(pluginGuid, info.title);

        FullChallenge chall = new()
        {
            Info = info,
            UnlockLevel = unlockLevel,
            AppearancesInChallengeScreen = numAppearancesInChallengeScreen,
            Handler = handlerType
        };

        if (!stackableMap.ContainsKey(info.challengeType))
        {
            stackableMap.Add(info.challengeType, chall.Stackable);
        }
        //unlockLevelMap.Add(info.challengeType, unlockLevel);

        NewInfos.Add(chall);

        return info;
    }

    public static AscensionChallengeInfo AddSpecific(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
        int numAppearancesInChallengeScreen = 1)
    {
        return AddSpecific(pluginGuid, title, description, pointValue, iconTexture, activatedTexture, unlockLevel, null, numAppearancesInChallengeScreen);
    }

    public static AscensionChallengeInfo AddSpecific<T>(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
        int numAppearancesInChallengeScreen = 1) where T : ChallengeBehaviour
    {
        return AddSpecific(pluginGuid, title, description, pointValue, iconTexture, activatedTexture, unlockLevel, typeof(T), numAppearancesInChallengeScreen);
    }

    public static AscensionChallengeInfo Add(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
        bool stackable = false)
    {
        return Add(pluginGuid, title, description, pointValue, iconTexture, activatedTexture, unlockLevel, null, stackable);
    }

    public static AscensionChallengeInfo Add<T>(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
        bool stackable = false) where T : ChallengeBehaviour
    {
        return Add(pluginGuid, title, description, pointValue, iconTexture, activatedTexture, unlockLevel, typeof(T), stackable);
    }

    public static AscensionChallengeInfo Add(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
        Type handlerType = null,
        bool stackable = false
    )
    {
        return AddSpecific(pluginGuid, title, description, pointValue, iconTexture, activatedTexture, unlockLevel, handlerType, stackable ? 2 : 1);
    }

    public static AscensionChallengeInfo AddSpecific(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
        Type handlerType = null,
        int numAppearancesInChallengeScreen = 1
    )
    {
        AscensionChallengeInfo info = ScriptableObject.CreateInstance<AscensionChallengeInfo>();
        info.title = title;
        info.challengeType = AscensionChallenge.None;
        info.description = description;
        info.pointValue = pointValue;
        info.iconSprite = TextureHelper.ConvertTexture(iconTexture, TextureHelper.SpriteType.ChallengeIcon);

        Texture2D infoActivationTexture = activatedTexture ??
            ((pointValue > 0) ? Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default")
                : Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_difficulty"));
        info.activatedSprite = TextureHelper.ConvertTexture(infoActivationTexture, TextureHelper.SpriteType.ChallengeIcon);

        return AddSpecific(pluginGuid, info, unlockLevel, handlerType, numAppearancesInChallengeScreen);
    }

    [HarmonyPatch(typeof(AscensionUnlockSchedule), "ChallengeIsUnlockedForLevel")]
    [HarmonyPostfix]
    public static void IsCustomChallengeUnlocked(ref bool __result, int level, AscensionChallenge challenge)
    {
        if (NewInfos.ToList().Exists(x => x != null && x.Info != null && x.Info.challengeType == challenge && x.UnlockLevel <= level))
            __result = true;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    public static void ResyncWhenTransitioningToGame()
    {
        SyncChallengeList();
    }

    [HarmonyPatch(typeof(TurnManager), "SetupPhase")]
    [HarmonyPostfix]
    public static IEnumerator SetupPhase(IEnumerator result, TurnManager __instance)
    {
        ChallengeBehaviour.DestroyAllInstances();
        if (SaveFile.IsAscension && AscensionSaveData.Data != null && AscensionSaveData.Data.activeChallenges != null)
        {
            foreach (AscensionChallenge challenge in AscensionSaveData.Data.activeChallenges)
            {
                FullChallenge nc = NewInfos.ToList().Find((x) => x != null && x.Info != null && x.Info.challengeType == challenge);
                if (nc != null && nc.Handler != null && nc.Handler.IsSubclassOf(typeof(ChallengeBehaviour)))
                {
                    GameObject challengehandler = new(nc.Info.name + " Challenge Handler");
                    ChallengeBehaviour behav = challengehandler.AddComponent(nc.Handler) as ChallengeBehaviour;
                    if (behav != null)
                    {
                        GlobalTriggerHandler.Instance?.RegisterNonCardReceiver(behav);
                        behav.challenge = nc;
                        ChallengeBehaviour.Instances.Add(behav);
                        if (behav.RespondToPreBattleStart())
                        {
                            yield return behav.OnPreBattleStart();
                        }
                    }
                }
            }
        }
        yield return result;
        foreach (ChallengeBehaviour behav in ChallengeBehaviour.Instances)
        {
            if (behav != null && behav.RespondToBattleStart())
            {
                yield return behav.OnBattleStart();
            }
        }
        yield break;
    }

    [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
    [HarmonyPostfix]
    public static IEnumerator CleanupPhase(IEnumerator result)
    {
        foreach (ChallengeBehaviour behav in ChallengeBehaviour.Instances)
        {
            if (behav != null && behav.RespondToPreCleanup())
            {
                yield return behav.OnPreCleanup();
            }
        }
        yield return result;
        foreach (ChallengeBehaviour behav in ChallengeBehaviour.Instances)
        {
            if (behav != null && behav.RespondToPostCleanup())
            {
                yield return behav.OnPostCleanup();
            }
        }
        ChallengeBehaviour.DestroyAllInstances();
        yield break;
    }
}