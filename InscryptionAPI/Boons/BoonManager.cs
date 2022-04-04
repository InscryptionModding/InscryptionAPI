using DiskCardGame;
using UnityEngine;
using HarmonyLib;
using System.Collections;
using System.Collections.ObjectModel;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;

namespace InscryptionAPI.Boons
{
    [HarmonyPatch]
    public static class BoonManager
    {
        public static readonly ReadOnlyCollection<BoonData> BaseGameBoons = new(Resources.LoadAll<BoonData>("Data/Boons"));
        public static readonly ObservableCollection<FullBoon> NewBoons = new();
        public static List<BoonData> AllBoonsCopy { get; private set; } = BaseGameBoons.ToList();

        public static BoonData.Type New(string guid, string name, Type boonHandlerType, string rulebookDescription, Texture icon, Texture cardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
            appearInRulebook = true)
        {
            FullBoon fb = new();
            BoonData data = ScriptableObject.CreateInstance<BoonData>();
            data.name = name;
            data.displayedName = name;
            data.description = rulebookDescription;
            data.icon = icon;
            data.cardArt = cardArt;
            data.minorEffect = !appearInLeshyTrials;
            data.type = GuidManager.GetEnumValue<BoonData.Type>(guid, name);
            fb.appearInRulebook = appearInRulebook;
            fb.boon = data;
            fb.boonHandlerType = boonHandlerType;
            fb.stacks = stackable;
            NewBoons.Add(fb);
            return data.type;
        }

        public static BoonData.Type New<T>(string guid, string name, string rulebookDescription, Texture icon, Texture cardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
            appearInRulebook = true) where T : BoonBehaviour
        {
            return New(guid, name, typeof(T), rulebookDescription, icon, cardArt, stackable, appearInLeshyTrials, appearInRulebook);
        }

        public static BoonData.Type New(string guid, string name, Type boonHandlerType, string rulebookDescription, string pathToIcon, string pathToCardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
            appearInRulebook = true)
        {
            return New(guid, name, boonHandlerType, rulebookDescription, TextureHelper.GetImageAsTexture(pathToIcon), TextureHelper.GetImageAsTexture(pathToCardArt), stackable, appearInLeshyTrials, appearInRulebook);
        }

        public static BoonData.Type New<T>(string guid, string name, string rulebookDescription, string pathToIcon, string pathToCardArt, bool stackable = true, bool appearInLeshyTrials = true, bool
            appearInRulebook = true) where T : BoonBehaviour
        {
            return New<T>(guid, name, rulebookDescription, pathToIcon, pathToCardArt, stackable, appearInLeshyTrials, appearInRulebook);
        }

        public static void SyncCardList()
        {
            var boons = BaseGameBoons.Concat(NewBoons.Select((x) => x.boon)).ToList();
            AllBoonsCopy = boons;
        }

        static BoonManager()
        {
            InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
            {
                if (type == typeof(BoonData))
                {
                    ScriptableObjectLoader<BoonData>.allData = AllBoonsCopy;
                }
            };
            NewBoons.CollectionChanged += static (_, _) =>
            {
                SyncCardList();
            };
        }

        [HarmonyPatch(typeof(BoonsHandler), "ActivatePreCombatBoons")]
        [HarmonyPostfix]
        private static IEnumerator ActivatePreCombatBoons(IEnumerator result, BoonsHandler __instance)
        {
            BoonBehaviour.DestroyAllInstances();
            if (__instance.BoonsEnabled && RunState.Run != null && RunState.Run.playerDeck != null && RunState.Run.playerDeck.Boons != null && NewBoons != null)
            {
                foreach (BoonData boon in RunState.Run.playerDeck.Boons)
                {
                    if (boon != null)
                    {
                        FullBoon nb = NewBoons.ToList().Find((x) => x.boon.type == boon.type);
                        if (nb != null && nb.boonHandlerType != null && nb.boonHandlerType.IsSubclassOf(typeof(BoonBehaviour)) && (nb.stacks || BoonBehaviour.CountInstancesOfType(nb.boon.type) < 1))
                        {
                            int instances = BoonBehaviour.CountInstancesOfType(nb.boon.type);
                            GameObject boonhandler = new(nb.boon.name + " Boon Handler");
                            BoonBehaviour behav = boonhandler.AddComponent(nb.boonHandlerType) as BoonBehaviour;
                            if (behav != null)
                            {
                                GlobalTriggerHandler.Instance?.RegisterNonCardReceiver(behav);
                                behav.boon = nb;
                                behav.instanceNumber = instances + 1;
                                BoonBehaviour.Instances.Add(behav);
                                if (behav.RespondToPreBoonActivation())
                                {
                                    yield return behav.OnPreBoonActivation();
                                }
                            }
                        }
                    }
                }
            }
            yield return result;
            foreach (BoonBehaviour bb in BoonBehaviour.Instances)
            {
                if (bb != null && bb.RespondToPostBoonActivation())
                {
                    yield return bb.OnPostBoonActivation();
                }
            }
            yield break;
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        private static IEnumerator Postfix(IEnumerator result)
        {
            foreach (BoonBehaviour bb in BoonBehaviour.Instances)
            {
                if (bb != null && bb.RespondToPreBattleCleanup())
                {
                    yield return bb.OnPreBattleCleanup();
                }
            }
            yield return result;
            foreach (BoonBehaviour bb in BoonBehaviour.Instances)
            {
                if (bb != null && bb.RespondToPostBattleCleanup())
                {
                    yield return bb.OnPostBattleCleanup();
                }
            }
            BoonBehaviour.DestroyAllInstances();
            yield break;
        }

        [HarmonyPatch(typeof(DeckInfo), "AddBoon")]
        [HarmonyPostfix]
        private static void AddBoon(BoonData.Type boonType)
        {
            if (TurnManager.Instance != null && !TurnManager.Instance.GameEnded && !TurnManager.Instance.GameEnding && !TurnManager.Instance.IsSetupPhase && TurnManager.Instance.Opponent != null)
            {
                FullBoon nb = NewBoons.ToList().Find((x) => x.boon.type == boonType);
                if (nb != null && nb.boonHandlerType != null && (nb.stacks || BoonBehaviour.CountInstancesOfType(nb.boon.type) < 1))
                {
                    int instances = BoonBehaviour.CountInstancesOfType(nb.boon.type);
                    GameObject boonhandler = new GameObject(nb.boon.name + " Boon Handler");
                    BoonBehaviour behav = boonhandler.AddComponent(nb.boonHandlerType) as BoonBehaviour;
                    if (behav != null)
                    {
                        GlobalTriggerHandler.Instance?.RegisterNonCardReceiver(behav);
                        behav.boon = nb;
                        behav.instanceNumber = instances + 1;
                        BoonBehaviour.Instances.Add(behav);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DeckInfo), "ClearBoons")]
        [HarmonyPostfix]
        private static void ClearBoons()
        {
            BoonBehaviour.DestroyAllInstances();
        }

        [HarmonyPatch(typeof(DeckInfo), nameof(DeckInfo.Boons), MethodType.Getter)]
        [HarmonyPostfix]
        private static void get_Boons(ref List<BoonData> __result, DeckInfo __instance)
        {
            if (__instance.boons != null && __instance.boonIds != null && __instance.boons.Count != __instance.boonIds.Count)
            {
                __instance.LoadBoons();
            }
            __result = __instance.boons;
        }

        [HarmonyPatch(typeof(DeckInfo), "LoadBoons")]
        [HarmonyPostfix]
        private static void LoadBoons(DeckInfo __instance)
        {
            __instance.boons.RemoveAll((x) => x == null);
        }

        [HarmonyPatch(typeof(RuleBookInfo), "ConstructPageData")]
        [HarmonyPostfix]
        private static void ConstructPageData(ref List<RuleBookPageInfo> __result, RuleBookInfo __instance, AbilityMetaCategory metaCategory)
        {
            if (NewBoons.Count > 0)
            {
                foreach (PageRangeInfo info in __instance.pageRanges)
                {
                    if (info.type == PageRangeType.Boons)
                    {
                        List<int> customBoons = NewBoons.Select(x => (int)x.boon.type).ToList();
                        int min = customBoons.AsQueryable().Min();
                        int max = customBoons.AsQueryable().Max();
                        List<RuleBookPageInfo> infos = __instance.ConstructPages(info, max + 1, min, (int index) => metaCategory == AbilityMetaCategory.Part1Rulebook && BoonsUtil.GetData((BoonData.Type)index).icon != null && customBoons.Contains(index) &&
                            (NewBoons.ToList().Find((x) => (int)x.boon.type == index)?.appearInRulebook).GetValueOrDefault(),
                            new Action<RuleBookPageInfo, PageRangeInfo, int>(__instance.FillBoonPage), Localization.Translate("APPENDIX XII, SUBSECTION VIII - BOONS {0}"));
                        __result.InsertRange(__result.FindLastIndex((x) => x.pagePrefab == info.rangePrefab), infos);
                    }
                }
            }
        }

        public class FullBoon
        {
            public BoonData boon;
            public Type boonHandlerType;
            public bool appearInRulebook;
            public bool stacks;
        }
    }
}
