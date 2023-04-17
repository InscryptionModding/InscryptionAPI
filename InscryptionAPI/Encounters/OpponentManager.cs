using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Masks;
using InscryptionAPI.Saves;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Encounters;

[HarmonyPatch]
public static class OpponentManager
{
    public class FullOpponent
    {
        public readonly Opponent.Type Id;
        public LeshyAnimationController.Mask MaskType = MaskManager.NoMask;
        public Type Opponent;
        public string SpecialSequencerId;
        public List<Texture2D> NodeAnimation = new();

        public FullOpponent(Opponent.Type id, Type opponent, string specialSequencerId) : this(id, opponent, specialSequencerId, null) { }

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
        var gameAsm = typeof(Opponent).Assembly;
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

    public static List<FullOpponent> AllOpponents { get; private set; } = BaseGameOpponents.ToList();

    public static FullOpponent Add(string guid, string opponentName, string sequencerID, Type opponentType)
    {
        return Add(guid, opponentName, sequencerID, opponentType, null);
    }

    public static FullOpponent Add(string guid, string opponentName, string sequencerID, Type opponentType, List<Texture2D> nodeAnimation)
    {
        Opponent.Type opponentId = GuidManager.GetEnumValue<Opponent.Type>(guid, opponentName);
        FullOpponent opp = new(opponentId, opponentType, sequencerID, nodeAnimation);
        NewOpponents.Add(opp);
        return opp;
    }

    #region Patches
    [HarmonyPatch(typeof(Opponent), nameof(Opponent.SpawnOpponent))]
    [HarmonyPrefix]
    private static bool ReplaceSpawnOpponent(EncounterData encounterData, ref Opponent __result)
    {
        if (encounterData.opponentType == Opponent.Type.Default || !ProgressionData.LearnedMechanic(MechanicsConcept.OpponentQueue))
            return true; // For default opponents or if we're in the tutorial, just let the base game logic flow

        // This mostly just follows the logic of the base game, other than the fact that the
        // opponent gets instantiated by looking up the type from the list

        GameObject gameObject = new GameObject();
        gameObject.name = "Opponent";

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

    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(BossBattleSequencer), nameof(BossBattleSequencer.GetSequencerIdForBoss))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string OriginalGetSequencerIdForBoss(Opponent.Type bossType) { throw new NotImplementedException(); }

    [HarmonyPatch(typeof(BossBattleSequencer), nameof(BossBattleSequencer.GetSequencerIdForBoss))]
    [HarmonyPrefix]
    private static bool ReplaceGetSequencerId(Opponent.Type bossType, ref string __result)
    {
        __result = AllOpponents.First(o => o.Id == bossType).SpecialSequencerId;
        return false;
    }

    [HarmonyPatch(typeof(BossBattleNodeData), nameof(BossBattleNodeData.PrefabPath), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool ReplacePrefabPath(ref string __result, Opponent.Type ___bossType)
    {
        GameObject obj = ResourceBank.Get<GameObject>("Prefabs/Map/MapNodesPart1/MapNode_" + ___bossType);
        if (obj != null)
        {
            __result = "Prefabs/Map/MapNodesPart1/MapNode_" + ___bossType;
        }
        else
        {
            __result = "Prefabs/Map/MapNodesPart1/MapNode_ProspectorBoss";
        }
        return false;
    }

    [HarmonyPatch]
    private class MapGenerator_CreateNode
    {
        static MethodInfo ProcessMethodInfo = AccessTools.Method(typeof(MapGenerator_CreateNode), nameof(ProcessBossType), new Type[] { typeof(NodeData) });

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

    #endregion
}