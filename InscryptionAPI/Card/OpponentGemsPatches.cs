using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionAPI.Card;
[HarmonyPatch]
internal class OpponentGemsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Setup))]
    [HarmonyPatch(typeof(PixelResourcesManager), nameof(PixelResourcesManager.Setup))]
    [HarmonyPatch(typeof(Part3ResourcesManager), nameof(Part3ResourcesManager.Setup))]
    private static void SetUpOpponentGems(ResourcesManager __instance)
    {
        if (__instance.GetComponent<OpponentGemsManager>() == null)
            __instance.gameObject.AddComponent<OpponentGemsManager>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
    private static void CleanUpOpponentGems() => Singleton<OpponentGemsManager>.Instance?.opponentGems.Clear();

    [HarmonyPostfix, HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ForceGemsUpdate))]
    private static void ForceOpponentGemsUpdate() => Singleton<OpponentGemsManager>.Instance?.ForceGemsUpdate();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FishHookGrab), nameof(FishHookGrab.PullHook))]
    [HarmonyPatch(typeof(FishHookItem), nameof(FishHookItem.OnValidTargetSelected))]
    private static IEnumerator UpdateGemsOnHookGrab(IEnumerator enumerator)
    {
        yield return enumerator;
        Singleton<ResourcesManager>.Instance.ForceGemsUpdate();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GainGem), nameof(GainGem.OnResolveOnBoard))]
    private static IEnumerator GainGemsForOpponents(IEnumerator enumerator, GainGem __instance)
    {
        yield return enumerator;
        if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance != null)
            Singleton<OpponentGemsManager>.Instance.AddGem(__instance.Gem);
    }
    [HarmonyPostfix, HarmonyPatch(typeof(GainGem), nameof(GainGem.OnDie))]
    private static IEnumerator LoseGemsForOpponents(IEnumerator enumerator, GainGem __instance)
    {
        yield return enumerator;
        if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance != null)
            Singleton<OpponentGemsManager>.Instance.LoseGem(__instance.Gem);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GainGemTriple), nameof(GainGemTriple.OnResolveOnBoard))]
    private static IEnumerator GainTripleGemsForOpponents(IEnumerator enumerator, GainGemTriple __instance)
    {
        yield return enumerator;
        if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance)
            Singleton<OpponentGemsManager>.Instance.AddGems(GemType.Green, GemType.Orange, GemType.Blue);
    }
    [HarmonyPostfix, HarmonyPatch(typeof(GainGemTriple), nameof(GainGemTriple.OnDie))]
    private static IEnumerator LoseTripleGemsForOpponents(IEnumerator enumerator, GainGemTriple __instance)
    {
        yield return enumerator;
        if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance != null)
            Singleton<OpponentGemsManager>.Instance.LoseGems(GemType.Green, GemType.Orange, GemType.Blue);
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(DiskRenderStatsLayer), nameof(DiskRenderStatsLayer.RenderCard))]
    private static IEnumerable<CodeInstruction> CorrectGemifiedColours(IEnumerable<CodeInstruction> instructions)
    {
        string name_ResourcesManager = "DiskCardGame.ResourcesManager get_Instance()";
        string name_HealthTextColour = "UnityEngine.Color healthTextColor";
        int startIndex = -1, endIndex = -1;

        List<CodeInstruction> codes = new(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            // this is where we'll be inserting our code
            if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == name_ResourcesManager)
            {
                startIndex = i;
                for (int j = i + 1; j < codes.Count; j++)
                {
                    if (codes[j].opcode == OpCodes.Stfld && codes[j].operand.ToString() == name_HealthTextColour)
                    {
                        endIndex = j + 1;
                        break;
                    }
                }
                break;
            }
        }

        if (startIndex > -1 && endIndex > -1)
        {
            MethodInfo customMethod = AccessTools.Method(typeof(OpponentGemsPatches), nameof(OpponentGemsPatches.FixAct3Render), new Type[] { typeof(DiskRenderStatsLayer), typeof(CardRenderInfo) });
            codes.RemoveRange(startIndex, endIndex - startIndex);
            codes.Insert(startIndex, new(OpCodes.Ldarg_0));
            codes.Insert(startIndex + 1, new(OpCodes.Ldarg_1));
            codes.Insert(startIndex + 2, new(OpCodes.Call, customMethod));
        }
        return codes;
    }
    private static void FixAct3Render(DiskRenderStatsLayer __instance, CardRenderInfo info)
    {
        if (__instance.PlayableCard)
        {
            if (__instance.PlayableCard.OpponentCard && Singleton<OpponentGemsManager>.Instance)
            {
                if (Singleton<OpponentGemsManager>.Instance.HasGem(GemType.Orange))
                    info.attackTextColor = GameColors.Instance.gold;

                if (Singleton<OpponentGemsManager>.Instance.HasGem(GemType.Green))
                    info.healthTextColor = GameColors.Instance.brightLimeGreen;
            }
            else if (!__instance.PlayableCard.OpponentCard)
            {
                if (Singleton<ResourcesManager>.Instance.HasGem(GemType.Orange))
                    info.attackTextColor = GameColors.Instance.gold;

                if (Singleton<ResourcesManager>.Instance.HasGem(GemType.Green))
                    info.healthTextColor = GameColors.Instance.brightLimeGreen;
            }
        }
    }
}