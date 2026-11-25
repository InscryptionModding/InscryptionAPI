using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Masks;
using InscryptionAPI.Saves;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Encounters;

/// <summary>
/// Manager class for adding custom opponents that will appear in-game.
/// </summary>
[HarmonyPatch]
public static class OpponentManager
{
    /// <summary>
    /// A utility class that holds all of the required information about an opponent in order to be able to use it in-game.
    /// </summary>
    public class FullOpponent {
        /// <summary>
        /// The unique ID for this opponent.
        /// </summary>
        public readonly Opponent.Type Id;
        /// <summary>
        /// The unique ID of the mask Leshy will use when this opponent is fought as a boss.
        /// </summary>
        public LeshyAnimationController.Mask MaskType = MaskManager.NoMask;
        /// <summary>
        /// A subclass of <see cref="DiskCardGame.Opponent"/> that implements the logic for the opponent.
        /// </summary>
        public Type Opponent;
        /// <summary>
        /// The ID of the <see cref="SpecialBattleSequencer"/> subclass the implements additional logic for the opponent's battle.
        /// </summary>
        public string SpecialSequencerId;
        /// <summary>
        /// The frames of the animation used for the opponent's dedicated node (ie, if it's a boss).
        /// </summary>
        public List<Texture2D> NodeAnimation = new();

        /// <summary>
        /// Creates a new instance of <see cref="FullOpponent"/> with no node animation.
        /// </summary>
        /// <param name="id">The ID of this opponent.</param>
        /// <param name="opponent">The <see cref="DiskCardGame.Opponent"/> subclass that implements this opponent's primary logic.</param>
        /// <param name="specialSequencerId">The ID of the <see cref="SpecialBattleSequencer"/> this opponent uses for supplemental battle logic.</param>
        public FullOpponent(Opponent.Type id, Type opponent, string specialSequencerId) : this(id, opponent, specialSequencerId, null) { }

        /// <summary>
        /// Creates a new instance of <see cref="FullOpponent"/>.
        /// </summary>
        /// <param name="id">The ID of this opponent.</param>
        /// <param name="opponent">The <see cref="DiskCardGame.Opponent"/> subclass that implements this opponent's primary logic.</param>
        /// <param name="specialSequencerId">The ID of the <see cref="SpecialBattleSequencer"/> this opponent uses for supplemental battle logic.</param>
        /// <param name="nodeAnimation">The frames of the animation used for the opponent's dedicated node (ie, if it's a boss).</param>
        public FullOpponent(Opponent.Type id, Type opponent, string specialSequencerId, List<Texture2D> nodeAnimation)
        {
            Id = id;
            SpecialSequencerId = specialSequencerId;
            Opponent = opponent;
            if (nodeAnimation != null)
            {
                NodeAnimation = new(nodeAnimation);
            }
        }
    }

    /// <summary>
    /// All of the vanilla game's opponents.
    /// </summary>
    public static readonly ReadOnlyCollection<FullOpponent> BaseGameOpponents = new(GenBaseGameOpponents());
    internal static readonly ObservableCollection<FullOpponent> NewOpponents = new();

    private static List<FullOpponent> GenBaseGameOpponents()
    {
        bool useReversePatch = true;
        try
        {
            OriginalGetSequencerIdForBoss(Opponent.Type.ProspectorBoss);
        }
        catch (NotImplementedException)
        {
            useReversePatch = false;
        }

        List<FullOpponent> baseGame = new();
        Assembly gameAsm = typeof(Opponent).Assembly;
        foreach (Opponent.Type opponent in Enum.GetValues(typeof(Opponent.Type)))
        {
            string specialSequencerId = useReversePatch ? OriginalGetSequencerIdForBoss(opponent) : BossBattleSequencer.GetSequencerIdForBoss(opponent);
            Type opponentType = gameAsm.GetType($"DiskCardGame.{opponent.ToString()}Opponent") ?? gameAsm.GetType($"GBC.{opponent.ToString()}Opponent");

            FullOpponent fullOpponent = new FullOpponent(opponent, opponentType, specialSequencerId)
            {
                MaskType = MaskManager.BossToMask(opponent)
            };
            baseGame.Add(fullOpponent);
        }
        return baseGame;
    }

    static OpponentManager()
    {
        NewOpponents.CollectionChanged += static (_, _) =>
        {
            AllOpponents = BaseGameOpponents.Concat(NewOpponents).ToList();
        };
    }

    /// <summary>
    /// The current processed list of all opponents in the game, including vanilla and mod-added opponents.
    /// </summary>
    public static List<FullOpponent> AllOpponents { get; private set; } = BaseGameOpponents.ToList();

    /// <summary>
    /// Creates a new opponent and registers it to be able to be fought in-game.
    /// <br/><br/>
    /// Does not use node animation textures.
    /// </summary>
    /// <param name="guid">The GUID of the mod adding the ability.</param>
    /// <param name="opponentName">The internal name of the opponent.</param>
    /// <param name="sequencerID">The ID of the <see cref="SpecialBattleSequencer"/> this opponent uses for supplemental battle logic.</param>
    /// <param name="opponentType">A subclass of <see cref="Opponent"/> that implements the logic for the opponent.</param>
    /// <returns>An instance of <see cref="FullOpponent"/> containing all this opponent's data.</returns>
    public static FullOpponent Add(string guid, string opponentName, string sequencerID, Type opponentType)
    {
        return Add(guid, opponentName, sequencerID, opponentType, null);
    }

    /// <summary>
    /// Creates a new opponent and registers it to be able to be fought in-game.
    /// </summary>
    /// <param name="guid">The GUID of the mod adding the ability.</param>
    /// <param name="opponentName">The internal name of the opponent.</param>
    /// <param name="sequencerID">The ID of the <see cref="SpecialBattleSequencer"/> this opponent uses for supplemental battle logic.</param>
    /// <param name="opponentType">A subclass of <see cref="Opponent"/> that implements the logic for the opponent.</param>
    /// <param name="nodeAnimation">The frames of the animation used for the opponent's dedicated node (ie, if it's a boss).</param>
    /// <returns>An instance of <see cref="FullOpponent"/> containing all this opponent's data.</returns>
    public static FullOpponent Add(string guid, string opponentName, string sequencerID, Type opponentType, List<Texture2D> nodeAnimation)
    {
        Opponent.Type opponentId = GuidManager.GetEnumValue<Opponent.Type>(guid, opponentName);
        FullOpponent opp = new(opponentId, opponentType, sequencerID, nodeAnimation);
        NewOpponents.Add(opp);
        return opp;
    }

    /// <summary>
    /// The list of opponents that will be fought in Act 1 in the current run.
    /// </summary>
    public static List<Opponent.Type> RunStateOpponents
    {
        get
        {
            List<Opponent.Type> previousBosses = new List<Opponent.Type>();

            string value = ModdedSaveManager.RunState.GetValue(InscryptionAPIPlugin.ModGUID, "PreviousBosses"); // 2,0,1
            if (value == null)
            {
                // Do nothing
            }
            else if (!value.Contains(','))
            {
                // Single boss encounter
                previousBosses.Add((Opponent.Type)int.Parse(value));
            }
            else
            {
                // Multiple boss encounters
                IEnumerable<Opponent.Type> ids = value.Split(',').Select(static (a) => (Opponent.Type)int.Parse(a));
                previousBosses.AddRange(ids);
            }

            return previousBosses;
        }
        set
        {
            string result = ""; // 2,0,1
            for (int i = 0; i < value.Count; i++)
            {
                if (i > 0)
                {
                    result += ",";
                }
                result += (int)value[i];

            }
            ModdedSaveManager.RunState.SetValue(InscryptionAPIPlugin.ModGUID, "PreviousBosses", result);
        }
    }

    #region Patches
    [HarmonyPrefix, HarmonyPatch(typeof(Opponent), nameof(Opponent.SpawnOpponent))]
    private static bool ReplaceSpawnOpponent(EncounterData encounterData, ref Opponent __result)
    {
        if (encounterData.opponentType == Opponent.Type.Default || !ProgressionData.LearnedMechanic(MechanicsConcept.OpponentQueue))
            return true; // For default opponents or if we're in the tutorial, just let the base game logic flow

        // This mostly just follows the logic of the base game, other than the fact that the
        // opponent gets instantiated by looking up the type from the list

        GameObject gameObject = new()
        {
            name = "Opponent"
        };

        __result = gameObject.AddComponent(AllOpponents.First(o => o.Id == encounterData.opponentType).Opponent) as Opponent;

        string typeName = string.IsNullOrWhiteSpace(encounterData.aiId) ? "AI" : encounterData.aiId;
        __result.AI = Activator.CreateInstance(CustomType.GetType("DiskCardGame", typeName)) as AI;
        __result.NumLives = __result.StartingLives;
        __result.OpponentType = encounterData.opponentType;
        __result.TurnPlan = __result.ModifyTurnPlan(encounterData.opponentTurnPlan);
        __result.Blueprint = encounterData.Blueprint;
        __result.Difficulty = encounterData.Difficulty;
        __result.ExtraTurnsToSurrender = SeededRandom.Range(0, 3, SaveManager.SaveFile.GetCurrentRandomSeed());
        return false;
    }

    /// <summary>
    /// Reverse patch containing the original, unmodified code for <see cref="BossBattleSequencer.GetSequencerIdForBoss(Opponent.Type)"/>.
    /// </summary>
    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(BossBattleSequencer), nameof(BossBattleSequencer.GetSequencerIdForBoss))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string OriginalGetSequencerIdForBoss(Opponent.Type bossType) { throw new NotImplementedException(); }

    [HarmonyPrefix, HarmonyPatch(typeof(BossBattleSequencer), nameof(BossBattleSequencer.GetSequencerIdForBoss))]
    private static bool ReplaceGetSequencerId(Opponent.Type bossType, ref string __result)
    {
        __result = AllOpponents.First(o => o.Id == bossType).SpecialSequencerId;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BossBattleNodeData), nameof(BossBattleNodeData.PrefabPath), MethodType.Getter)]
    private static bool ReplacePrefabPath(ref string __result, Opponent.Type ___bossType)
    {
        string fullPath = "Prefabs/Map/MapNodesPart1/MapNode_" + ___bossType;
        GameObject obj = ResourceBank.Get<GameObject>(fullPath);
        __result = obj != null ? fullPath : "Prefabs/Map/MapNodesPart1/MapNode_ProspectorBoss";
        return false;
    }

    [HarmonyPatch(typeof(Opponent), nameof(Opponent.CreateCard))]
    [HarmonyPrefix]
    private static bool CloneCardInfo(ref CardInfo cardInfo)
    {
        // Dynamic costs require a unique CardInfo to get the playable card
        cardInfo = (CardInfo)cardInfo.Clone();
        return true;
    }

    [HarmonyPatch]
    private class MapGenerator_CreateNode
    {
        private static readonly MethodInfo ProcessMethodInfo = AccessTools.Method(typeof(MapGenerator_CreateNode), nameof(ProcessBossType), new Type[] { typeof(NodeData) });

        internal static ItemData currentItemData = null;

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(MapGenerator), "CreateNode");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // === We want to turn this

            // nodeData = new BossBattleNodeData();
            // (nodeData as BossBattleNodeData).bossType = RunState.CurrentMapRegion.bosses[Random.Range(0, RunState.CurrentMapRegion.bosses.Count)];

            // === Into this

            // nodeData = new BossBattleNodeData();
            // (nodeData as BossBattleNodeData).bossType = RunState.CurrentMapRegion.bosses[Random.Range(0, RunState.CurrentMapRegion.bosses.Count)];
            // ProcessBossType(nodeData);

            // ===
            FieldInfo bossTypeField = AccessTools.Field(typeof(BossBattleNodeData), "bossType");

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction codeInstruction = codes[i];
                if (codeInstruction.opcode == OpCodes.Stfld)
                {
                    if (codeInstruction.operand == bossTypeField)
                    {
                        codes.Insert(++i, new CodeInstruction(OpCodes.Ldloc_0));
                        codes.Insert(++i, new CodeInstruction(OpCodes.Call, ProcessMethodInfo));
                        break;
                    }
                }
            }

            return codes;
        }

        public static void ProcessBossType(NodeData nodeData)
        {
            BossBattleNodeData bossBattleNodeData = (BossBattleNodeData)nodeData;

            // Parse data
            List<Opponent.Type> bosses = RunStateOpponents;
            if (bosses.Count == 0)
            {
                bosses.Add(Opponent.Type.ProspectorBoss);
                bosses.Add(Opponent.Type.AnglerBoss);
                bosses.Add(Opponent.Type.TrapperTraderBoss);
            }

            bossBattleNodeData.bossType = bosses[RunState.CurrentRegionTier];
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CardDrawPiles), nameof(CardDrawPiles.ExhaustedSequence))]
    private static IEnumerator CustomBossExhaustionSequence(IEnumerator enumerator, CardDrawPiles __instance)
    {
        if (TurnManager.Instance.Opponent is ICustomExhaustSequence exhaustSeq && exhaustSeq.RespondsToCustomExhaustSequence(__instance))
        {
            ViewManager.Instance.SwitchToView(View.CardPiles, immediate: false, lockAfter: true);
            yield return new WaitForSeconds(1f);
            yield return exhaustSeq.DoCustomExhaustSequence(__instance);
            ViewManager.Instance.SwitchToView(View.Default);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            __instance.turnsSinceExhausted++;
        }
        else
        {
            yield return enumerator;
        }
    }
    #endregion

    #region Optimization Patches

    [HarmonyPatch]
    internal class SpawnScenery_Patches
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Part1BossOpponent), nameof(Part1BossOpponent.SpawnScenery))]
        private static bool DisableTableScenery(Part1BossOpponent __instance)
        {
            // Show run method if scenery is disabled
            if (InscryptionAPIPlugin.configHideAct1BossScenery.Value)
            {
                __instance.sceneryObject = new GameObject("TemporaryScenary");
                __instance.sceneryObject.AddComponent<Animation>();
                if (__instance is PirateSkullBossOpponent)
                    __instance.sceneryObject.AddComponent<PirateSkullBossCannons>();
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PirateSkullBossCannons), nameof(PirateSkullBossCannons.AimCannons))]
        [HarmonyPatch(typeof(PirateSkullBossCannons), nameof(PirateSkullBossCannons.FireLeftSide))]
        [HarmonyPatch(typeof(PirateSkullBossCannons), nameof(PirateSkullBossCannons.FireRightSide))]
        [HarmonyPatch(typeof(PirateSkullBossCannons), nameof(PirateSkullBossCannons.ResetLeftSide))]
        [HarmonyPatch(typeof(PirateSkullBossCannons), nameof(PirateSkullBossCannons.ResetRightSide))]
        private static bool DisableCannonAnims()
        {
            if (InscryptionAPIPlugin.configHideAct1BossScenery.Value)
                return false;

            return true;
        }
    }

    [HarmonyPatch]
    internal class PreventCallsOnScenary_Patches
    {
        private static readonly Type StartNewPhaseSequence = Type.GetType("DiskCardGame.TrapperTraderBossOpponent+<StartNewPhaseSequence>d__6, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static readonly MethodInfo PlayAnimationInfo = AccessTools.Method(typeof(PreventCallsOnScenary_Patches), nameof(PlayAnimation), new[] { typeof(Animation), typeof(string) });

        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(StartNewPhaseSequence, "MoveNext");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // We want to change
            //
            // sceneryObject.GetComponent<Animation>().Play("knives_table_exit");
            //
            // To
            //
            // PlayAnimation(sceneryObject.GetComponent<Animation>(), "knives_table_exit");

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand == "knives_table_exit")
                {

                    // ldstr "knives_table_exit" 
                    codes[i + 1] = new CodeInstruction(OpCodes.Call, PlayAnimationInfo); // callvirt instance bool [UnityEngine.AnimationModule]UnityEngine.Animation::Play(string)
                    // pop
                    break;
                }
            }

            return codes;
        }

        private static bool PlayAnimation(Animation animation, string key)
        {
            if (InscryptionAPIPlugin.configHideAct1BossScenery.Value)
            {
                InscryptionAPIPlugin.Logger.LogInfo("PlayAnimation false");
                return false;
            }

            InscryptionAPIPlugin.Logger.LogInfo("PlayAnimation true");
            return animation.Play(key);
        }
    }
    #endregion
}