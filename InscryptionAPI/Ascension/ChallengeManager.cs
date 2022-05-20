using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using System.Collections.ObjectModel;
using InscryptionAPI.Boons;
using System.Collections;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class ChallengeManager
{

    public static readonly Sprite DEFAULT_ACTIVATED_SPRITE = TextureHelper.ConvertTexture(
        Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"),
        TextureHelper.SpriteType.ChallengeIcon);

    public class FullChallenge
    {
        public AscensionChallengeInfo Challenge { get; set; }
        public int AppearancesInChallengeScreen { get; set; }
        public bool Stackable
        {
            get
            {
                return AppearancesInChallengeScreen > 1;
            }
            set
            {
                if(Stackable != value)
                {
                    AppearancesInChallengeScreen = value ? 2 : 1;
                }
            }
        }
        public bool AppearsInChallengeScreen
        {
            get
            {
                return AppearancesInChallengeScreen > 0;
            }
            set
            {
                if(AppearsInChallengeScreen != value)
                {
                    AppearancesInChallengeScreen = value ? 1 : 0;
                }
            }
        }
        public int UnlockLevel { get; set; }
        public Type ChallengeHandler { get; set; }
        public Func<FullChallenge[], IEnumerable<AscensionChallenge>> DependantChallengeGetter { get; set; }
        public Func<FullChallenge[], IEnumerable<AscensionChallenge>> IncompatibleChallengeGetter { get; set; }
        public List<object> Flags { get; set; }
        public static bool ValidHandlerType(Type t)
        {
            return t != null && !t.IsAbstract && t.IsSubclassOf(typeof(ChallengeBehaviour));
        }
        public int SortValue => Challenge.pointValue == 0 ? 0 : Math.Sign(Challenge.pointValue);
        public bool HasValidHandler()
        {
            return ValidHandlerType(ChallengeHandler);
        }
        public static implicit operator AscensionChallengeInfo(FullChallenge fc)
        {
            return fc.Challenge;
        }
        public FullChallenge SetChallenge(AscensionChallengeInfo challenge)
        {
            Challenge = challenge;
            return this;
        }
        public FullChallenge SetAppearancesInChallengeScreen(int appearancesInChallengeScreen)
        {
            AppearancesInChallengeScreen = appearancesInChallengeScreen;
            return this;
        }
        public FullChallenge SetUnlockLevel(int unlockLevel)
        {
            UnlockLevel = unlockLevel;
            return this;
        }
        public FullChallenge SetChallengeHandler(Type challengeHandler)
        {
            ChallengeHandler = challengeHandler;
            return this;
        }
        public FullChallenge SetDependantChallengeGetter(Func<FullChallenge[], IEnumerable<AscensionChallenge>> dependantChallengeGetter)
        {
            DependantChallengeGetter = dependantChallengeGetter;
            return this;
        }
        public FullChallenge SetDependantChallengeGetterStatic(params AscensionChallenge[] dependantChallenges)
        {
            DependantChallengeGetter = (x) => dependantChallenges;
            return this;
        }
        public FullChallenge SetDependantChallengeGetterFlagWhitelist(params string[] whitelistFlags)
        {
            DependantChallengeGetter = (x) => x.Where(x2 => x2.Flags.Exists(x3 => whitelistFlags.Contains(x3))).Select(x => x.Challenge.challengeType);
            return this;
        }
        public FullChallenge SetDependantChallengeGetterFlagBlacklist(params string[] blacklistFlags)
        {
            DependantChallengeGetter = (x) => x.Where(x2 => !x2.Flags.Exists(x3 => blacklistFlags.Contains(x3))).Select(x => x.Challenge.challengeType);
            return this;
        }

        public FullChallenge SetIncompatibleChallengeGetter(Func<FullChallenge[], IEnumerable<AscensionChallenge>> incompatibleChallengeGetter)
        {
            IncompatibleChallengeGetter = incompatibleChallengeGetter;
            return this;
        }
        public FullChallenge SetIncompatibleChallengeGetterStatic(params AscensionChallenge[] incompatibleChallenges)
        {
            IncompatibleChallengeGetter = (x) => incompatibleChallenges;
            return this;
        }
        public FullChallenge SetIncompatibleChallengeGetterFlagWhitelist(params string[] whitelistFlags)
        {
            IncompatibleChallengeGetter = (x) => x.Where(x2 => x2.Flags.Exists(x3 => whitelistFlags.Contains(x3))).Select(x => x.Challenge.challengeType);
            return this;
        }
        public FullChallenge SetIncompatibleChallengeGetterFlagBlacklist(params string[] blacklistFlags)
        {
            IncompatibleChallengeGetter = (x) => x.Where(x2 => !x2.Flags.Exists(x3 => blacklistFlags.Contains(x3))).Select(x => x.Challenge.challengeType);
            return this;
        }
        public FullChallenge SetFlags(params object[] flags)
        {
            Flags = flags.ToList();
            return this;
        }
    }

    public static readonly ReadOnlyCollection<FullChallenge> BaseGameChallenges = new(GenBaseGameChallengs());

    internal static readonly ObservableCollection<FullChallenge> NewInfos = new();

    private static List<FullChallenge> GenBaseGameChallengs()
    {
        List<AscensionChallengeInfo> infos = new(Resources.LoadAll<AscensionChallengeInfo>("Data/Ascension/Challenges"));

        GameObject screenobj = Resources.Load<GameObject>("prefabs/ui/ascension/AscensionChallengesScreen");

        AscensionUnlockSchedule.Initialize();
        AscensionUnlockSchedule schedule = AscensionUnlockSchedule.instance;
        List<FullChallenge> retval = new();
        AscensionChallengeScreen screen = screenobj.GetComponentInChildren<AscensionChallengeScreen>();
        List<AscensionIconInteractable> sortedicons = new(screen.icons);
        sortedicons.Sort((x, x2) => Mathf.RoundToInt((Mathf.Abs(x.transform.position.x - x2.transform.position.x) < 0.1f ? x2.transform.position.y - x.transform.position.y : x.transform.position.x - x2.transform.position.x) * 100));
        foreach (AscensionIconInteractable icon in sortedicons)
        {
            FullChallenge existing = retval.Find(x => x != null && x.Challenge == icon.Info);
            if(existing != null)
            {
                existing.AppearancesInChallengeScreen++;
            }
            else
            {
                retval.Add(new()
                {
                    Challenge = icon.Info,
                    AppearancesInChallengeScreen = 1,
                    UnlockLevel = schedule.unlockTiers.FindIndex(x => x.challengesUnlocked.Contains(icon.Info.challengeType)),
                    ChallengeHandler = null
                });
            }
        }

        return retval;
    }

    internal static FullChallenge[] GetChallengeIcons()
    {
        AscensionChallengeScreen screen = Singleton<AscensionChallengeScreen>.Instance;
        if(screen == null)
        {
            return new FullChallenge[0];
        }
        return screen.icons?.FindAll(x => x != null && x.challengeInfo != null && x.challengeInfo.challengeType != AscensionChallenge.None && x.challengeInfo.GetFullChallenge() != null)?
            .ConvertAll(x => x.challengeInfo.challengeType.GetFullChallenge())?.ToArray() ?? new FullChallenge[0];
    }

    public static event Func<List<FullChallenge>, List<FullChallenge>> ModifyChallenges;

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
        List<FullChallenge> asci = new(NewInfos);
        var challenges = BaseGameChallenges.Concat(asci).ToList();
        AllChallenges = ModifyChallenges?.Invoke(challenges) ?? challenges;        
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

    public static AscensionChallengeInfo GetInfo(this AscensionChallenge info)
    {
        return AllChallenges.Find(x => x != null && x.Challenge != null && x.Challenge.challengeType == info)?.Challenge;
    }

    public static FullChallenge GetFullChallenge(this AscensionChallenge info)
    {
        return AllChallenges.Find(x => x != null && x.Challenge != null && x.Challenge.challengeType == info);
    }

    public static FullChallenge GetFullChallenge(this AscensionChallengeInfo info)
    {
        if(info == null)
        {
            return null;
        }
        return info.challengeType.GetFullChallenge();
    }

    public static List<FullChallenge> AllChallenges = BaseGameChallenges.ToList();
    public static List<AscensionChallengeInfo> AllInfo => AllChallenges.ConvertAll(x => x.Challenge);

    public static bool IsStackable(AscensionChallenge id)
    {
        return (AllChallenges.Find(x => x != null && x.Challenge != null && x.Challenge.challengeType == id)?.Stackable).GetValueOrDefault();
    }

    public static FullChallenge Add(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, bool stackable = false)
    {
        return AddSpecific(pluginGuid, info, unlockLevel, stackable ? 2 : 1);
    }

    public static FullChallenge AddSpecific(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, int numAppearancesInChallengeScreen = 1)
    {
        return AddSpecific(pluginGuid, info, null, unlockLevel, null, null, null, numAppearancesInChallengeScreen);
    }

    public static FullChallenge AddSpecific<T>(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
        List<object> flags = null,
        Func<FullChallenge[], IEnumerable<AscensionChallenge>> dependantChallengeGetter = null,
        Func<FullChallenge[], IEnumerable<AscensionChallenge>> incompatibleChallengeGetter = null,
        int numAppearancesInChallengeScreen = 1
    ) where T : ChallengeBehaviour
    {
        return AddSpecific(pluginGuid, title, description, pointValue, iconTexture, activatedTexture, typeof(T), unlockLevel, flags, dependantChallengeGetter, incompatibleChallengeGetter, numAppearancesInChallengeScreen);
    }

    public static FullChallenge AddSpecific(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        Type handlerType = null,
        int unlockLevel = 0,
        List<object> flags = null,
        Func<FullChallenge[], IEnumerable<AscensionChallenge>> dependantChallengeGetter = null,
        Func<FullChallenge[], IEnumerable<AscensionChallenge>> incompatibleChallengeGetter = null,
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

        return AddSpecific(pluginGuid, info, handlerType, unlockLevel, flags, dependantChallengeGetter, incompatibleChallengeGetter, numAppearancesInChallengeScreen);
    }

    public static FullChallenge AddSpecific<T>(string pluginGuid, AscensionChallengeInfo info, int unlockLevel = 0, List<object> flags = null,
        Func<FullChallenge[], IEnumerable<AscensionChallenge>> dependantChallengeGetter = null, Func<FullChallenge[], IEnumerable<AscensionChallenge>> incompatibleChallengeGetter = null, int numAppearancesInChallengeScreen = 1)
        where T : ChallengeBehaviour
    {
        return AddSpecific(pluginGuid, info, typeof(T), unlockLevel, flags, dependantChallengeGetter, incompatibleChallengeGetter, numAppearancesInChallengeScreen);
    }

    public static FullChallenge AddSpecific(string pluginGuid, AscensionChallengeInfo info, Type handlerType = null, int unlockLevel = 0, List<object> flags = null, 
        Func<FullChallenge[], IEnumerable<AscensionChallenge>> dependantChallengeGetter = null, Func<FullChallenge[], IEnumerable<AscensionChallenge>> incompatibleChallengeGetter = null, int numAppearancesInChallengeScreen = 1)
    {
        info.challengeType = GuidManager.GetEnumValue<AscensionChallenge>(pluginGuid, info.title);

        FullChallenge fc = new()
        {
            Challenge = info,
            AppearancesInChallengeScreen = numAppearancesInChallengeScreen,
            UnlockLevel = unlockLevel,
            ChallengeHandler = handlerType,
            DependantChallengeGetter = dependantChallengeGetter,
            IncompatibleChallengeGetter = incompatibleChallengeGetter,
            Flags = flags != null ? new(flags) : new()
        };

        NewInfos.Add(fc);

        return fc;
    }

    public static FullChallenge AddSpecific(
        string pluginGuid,
        string title,
        string description,
        int pointValue,
        Texture2D iconTexture,
        Texture2D activatedTexture = null,
        int unlockLevel = 0,
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

        return AddSpecific(pluginGuid, info, unlockLevel, numAppearancesInChallengeScreen);
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
        return AddSpecific(pluginGuid, title, description, pointValue, iconTexture, activatedTexture, unlockLevel, stackable ? 2 : 1);
    }

    [HarmonyPatch(typeof(AscensionUnlockSchedule), "ChallengeIsUnlockedForLevel")]
    [HarmonyPostfix]
    private static void IsCustomChallengeUnlocked(ref bool __result, int level, AscensionChallenge challenge)
    {
        if (!__result)
        {
            FullChallenge fc = NewInfos.ToList().Find(x => x != null && x.Challenge != null && x.Challenge.challengeType == challenge);
            __result = fc != null && level >= fc.UnlockLevel;
        }
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    private static void ResyncWhenTransitioningToGame()
    {
        SyncChallengeList();
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
    [HarmonyPostfix]
    private static IEnumerator ActivateChallenges(IEnumerator result, TurnManager __instance)
    {
        ChallengeBehaviour.DestroyAllInstances();
        if (SaveFile.IsAscension)
        {
            AscensionChallenge[] challenges = AscensionSaveData.Data.activeChallenges.ToArray();
            foreach (AscensionChallenge chall in challenges)
            {
                FullChallenge fc = chall.GetFullChallenge();
                if (fc != null && fc.HasValidHandler())
                {
                    int instances = ChallengeBehaviour.CountInstancesOfType(fc.Challenge.challengeType);
                    if (fc.Stackable || instances < 1)
                    {
                        GameObject handler = new(fc.Challenge.name + " Challenge Handler");
                        ChallengeBehaviour behav = handler.AddComponent(fc.ChallengeHandler) as ChallengeBehaviour;
                        if (behav != null)
                        {
                            GlobalTriggerHandler.Instance?.RegisterNonCardReceiver(behav);
                            behav.challenge = fc;
                            behav.instanceNumber = instances + 1;
                            ChallengeBehaviour.Instances.Add(behav);
                            if (behav.RespondsToPreBattleSetup())
                            {
                                yield return behav.OnPreBattleSetup();
                            }
                        }
                    }
                }
            }
        }
        yield return result;
        ChallengeBehaviour[] cbs = ChallengeBehaviour.Instances.ToArray();
        foreach (ChallengeBehaviour bb in cbs)
        {
            if (bb != null && bb.RespondsToPostBattleSetup())
            {
                yield return bb.OnPostBattleSetup();
            }
        }
        yield break;
    }

    [HarmonyPatch(typeof(ChallengeActivationUI), nameof(ChallengeActivationUI.ShowActivation))]
    [HarmonyPrefix]
    private static void FixIconColor(ChallengeActivationUI __instance, AscensionChallenge challenge)
    {
        Color iconColor = GameColors.Instance.gold;
        var info = AscensionChallengesUtil.GetInfo(challenge);
        if(info == null || info.pointValue > 0 || challenge == AscensionChallenge.None)
        {
            iconColor = GameColors.Instance.red;
        }
        else if(info.pointValue < 0)
        {
            iconColor = GameColors.Instance.darkLimeGreen;
        }
        __instance.icon.color = iconColor;
        __instance.blinkEffect.blinkOffColor = iconColor;
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
    [HarmonyPostfix]
    private static IEnumerator Postfix(IEnumerator result)
    {
        ChallengeBehaviour[] cbs = ChallengeBehaviour.Instances.ToArray();
        foreach (ChallengeBehaviour bb in cbs)
        {
            if (bb != null && bb.RespondsToPreBattleCleanup())
            {
                yield return bb.OnPreBattleCleanup();
            }
        }
        yield return result;
        cbs = ChallengeBehaviour.Instances.ToArray();
        foreach (ChallengeBehaviour bb in cbs)
        {
            if (bb != null && bb.RespondsToPostBattleCleanup())
            {
                yield return bb.OnPostBattleCleanup();
            }
        }
        ChallengeBehaviour.DestroyAllInstances();
        yield break;
    }

    [HarmonyPatch(typeof(AscensionIconInteractable), nameof(AscensionIconInteractable.OnCursorSelectStart))]
    [HarmonyPostfix]
    private static void DoDependenciesAndIncompatibilities(AscensionIconInteractable __instance)
    {
        if (__instance.Unlocked && __instance.clickable)
        {
            bool setCurrentIcon = false;
            if(currentIcon == null)
            {
                currentIcon = __instance;
                setCurrentIcon = true;
            }
            bool activated = __instance.activatedRenderer.enabled;
            AscensionChallengeScreen screen = Singleton<AscensionChallengeScreen>.Instance;
            var challengeInfo = __instance.challengeInfo;
            if (activated)
            {
                bool shouldDisable = false;
                List<AscensionChallenge> dependencies = challengeInfo?.GetFullChallenge()?.DependantChallengeGetter?.Invoke(GetChallengeIcons())?.ToList();
                List<AscensionChallenge> incompatibilities = challengeInfo?.GetFullChallenge()?.IncompatibleChallengeGetter?.Invoke(GetChallengeIcons())?.ToList();
                if (dependencies != null && incompatibilities != null)
                {
                    incompatibilities.RemoveAll(x => dependencies.Contains(x));
                }
                if (dependencies != null)
                {
                    dependencies.RemoveAll(x => x == challengeInfo.challengeType);
                    List<AscensionChallenge> dependenciesClone = new(dependencies);
                    foreach (var icon in screen.icons.Where(x => x != null && x.Unlocked && x.clickable && x.activatedRenderer != null && x.activatedRenderer.enabled && x.Info != null && 
                        dependencies.Contains(x.Info.challengeType)))
                    {
                        if (dependenciesClone.Contains(icon.Info.challengeType))
                        {
                            dependenciesClone.Remove(icon.Info.challengeType);
                        }
                    }
                    if (dependenciesClone.Count > 0)
                    {
                        List<AscensionIconInteractable> challengeIcons = new();
                        foreach (var icon in screen.icons.Where(x => x != null && x.Unlocked && x.clickable && x.activatedRenderer != null && !x.activatedRenderer.enabled && x.Info != null && 
                            dependencies.Contains(x.Info.challengeType)))
                        {
                            if (dependenciesClone.Contains(icon.Info.challengeType))
                            {
                                dependenciesClone.Remove(icon.Info.challengeType);
                                challengeIcons.Add(icon);
                            }
                        }
                        if (dependenciesClone.Count > 0)
                        {
                            shouldDisable = true;
                        }
                        else
                        {
                            foreach (var icon in challengeIcons)
                            {
                                icon?.OnCursorSelectStart();
                            }
                            Singleton<AscensionChallengeDisplayer>.Instance?.DisplayChallenge(challengeInfo, false);
                        }
                    }
                }
                if (shouldDisable)
                {
                    __instance.ShowActivated(false);
                    screen.SetChallengeActivated(__instance.challengeInfo, false);
                }
                else
                {
                    foreach (var icon in screen.icons)
                    {
                        if (icon.activatedRenderer != null && icon.activatedRenderer.enabled && icon != currentIcon && icon.challengeInfo != null)
                        {
                            List<AscensionChallenge> dependencies2 = icon?.challengeInfo?.GetFullChallenge()?.DependantChallengeGetter?.Invoke(GetChallengeIcons())?.ToList();
                            List<AscensionChallenge> incompatibilities2 = icon?.challengeInfo?.GetFullChallenge()?.IncompatibleChallengeGetter?.Invoke(GetChallengeIcons())?.ToList();
                            if (dependencies2 != null && incompatibilities2 != null)
                            {
                                incompatibilities.RemoveAll(x => dependencies2.Contains(x));
                            }
                            if (incompatibilities2 != null)
                            {
                                incompatibilities2.RemoveAll(x => x == icon.challengeInfo.challengeType);
                                if(screen.icons.Exists(x => x.Unlocked && x.clickable && x.Info != null && x.activatedRenderer != null && x.activatedRenderer.enabled && incompatibilities2.Contains(x.Info.challengeType)))
                                {
                                    icon?.OnCursorSelectStart();
                                }
                            }
                        }
                    }
                    if (incompatibilities != null)
                    {
                        incompatibilities.RemoveAll(x => x == challengeInfo.challengeType);
                        screen.icons.FindAll(x => x.Unlocked && x.clickable && x.activatedRenderer.enabled && incompatibilities.Contains(x.Info.challengeType)).ForEach(x => x.OnCursorSelectStart());
                        screen.icons.FindAll(x => x.Unlocked && x.clickable && x.activatedRenderer.enabled && incompatibilities.Contains(x.Info.challengeType)).ForEach(x => x.OnCursorSelectStart());
                        AscensionSaveData.Data.activeChallenges?.RemoveAll(x => incompatibilities.Contains(x));
                        screen.challengeLevelText?.UpdateText();
                        Singleton<AscensionChallengeDisplayer>.Instance?.DisplayChallenge(challengeInfo, false);
                    }
                }
            }
            else
            {
                foreach(var icon in screen.icons)
                {
                    if (icon.activatedRenderer.enabled && icon != currentIcon)
                    {
                        List<AscensionChallenge> dependencies = icon?.challengeInfo?.GetFullChallenge()?.DependantChallengeGetter?.Invoke(GetChallengeIcons())?.ToList();
                        List<AscensionChallenge> incompatibilities = icon?.challengeInfo?.GetFullChallenge()?.IncompatibleChallengeGetter?.Invoke(GetChallengeIcons())?.ToList();
                        if (dependencies != null && incompatibilities != null)
                        {
                            incompatibilities.RemoveAll(x => dependencies.Contains(x));
                        }
                        if (dependencies != null)
                        {
                            dependencies.RemoveAll(x => x == icon.challengeInfo.challengeType);
                            List<AscensionChallenge> dependenciesClone = new(dependencies);
                            foreach (var icon2 in screen.icons.Where(x => x.Unlocked && x.clickable && x.Info != null && x.activatedRenderer != null && x.activatedRenderer.enabled && dependencies.Contains(x.Info.challengeType)))
                            {
                                if (dependenciesClone.Contains(icon2.Info.challengeType))
                                {
                                    dependenciesClone.Remove(icon2.Info.challengeType);
                                }
                            }
                            if(dependenciesClone.Count > 0)
                            {
                                icon?.OnCursorSelectStart();
                            }
                        }
                    }
                }
            }
            if (setCurrentIcon)
            {
                currentIcon = null;
            }
        }
    }

    private static AscensionIconInteractable currentIcon;
}