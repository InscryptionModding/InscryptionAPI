using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

/// <summary>
/// Fixes activated abilities not working outside Act 2.
/// </summary>
[HarmonyPatch]
public static class ActivatedAbilityIconFix
{
    /// <summary>
    /// A bool determining whether or not a card has an Activated Ability.
    /// </summary>
    /// <param name="card">The <see cref="PlayableCard"/> in which we want to see if it has a Activated Ability or not.</param>
    /// <returns>True if the card has an Activated Ability, false otherwise.</returns>
    private static bool HasActivatedAbility(this PlayableCard card)
    {
        return card.Info.Abilities.Exists(elem => AbilitiesUtil.GetInfo(elem).activated) || card.temporaryMods.Exists(elem => elem.abilities.Exists(elem2 => AbilitiesUtil.GetInfo(elem2).activated));

    }

    [HarmonyPostfix, HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.RespondsToResolveOnBoard))]
    private static void RespondsToResolveOnBoard_PostFix(ref bool __result)
    {
        __result &= SaveManager.SaveFile.IsPart2;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.OnStatsChanged))]
    private static void FixActivatedAbilitiesOnAnyChange(ref PlayableCard __instance)
    {
        if (SaveManager.SaveFile.IsPart2 || !__instance.HasActivatedAbility())
            return;

        ActivatedAbilityHandler3D abilityHandler3D = __instance.GetComponent<ActivatedAbilityHandler3D>();

        if (abilityHandler3D.SafeIsUnityNull()) {
            if (PatchPlugin.configDisplayCompleteDebugLogsInConsole.Value)
                PatchPlugin.Logger.LogDebug($"[PlayableCard.OnStatsChanged] Adding activated ability handler to card [{__instance.Info.displayedName}]");

            abilityHandler3D = __instance.gameObject.AddComponent<ActivatedAbilityHandler3D>();
        }

        if (!abilityHandler3D.SafeIsUnityNull() && __instance.AbilityIcons.abilityIcons != null)
        {
            if (PatchPlugin.configDisplayCompleteDebugLogsInConsole.Value)
                PatchPlugin.Logger.LogDebug($"[PlayableCard.OnStatsChanged] -> Resetting icon list for [{__instance.Info.displayedName}]");

            abilityHandler3D.UpdateInteractableList(
                __instance.AbilityIcons.abilityIcons,
                (CardRenderCamera.Instance.cardDisplayer as CardDisplayer3D).AbilityIcons.abilityIcons
                );
        }
    }

    /// <summary>
    /// Determines the colour of the given <see cref="AbilityIconInteractable"/>'s icon, while supporting dynamic colouring for 3D activated sigil icons.
    /// </summary>
    /// <param name="abilityIconInteractable">The <see cref="AbilityIconInteractable"/> whose icon we're colouring.</param>
    /// <param name="color">The default ability colour. This will be used if no override conditions are met.</param>
    /// <param name="inConduitCircuit">Whether the card with this ability is in a conduit circuit.</param>
    public static void SetAbilityIconDefaultColour(AbilityIconInteractable abilityIconInteractable, Color color, bool inConduitCircuit) {
        AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
        ActivatedAbilityIconInteractable act = abilityIconInteractable.GetComponent<ActivatedAbilityIconInteractable>();

        if (info.hasColorOverride && !(info.conduitCell && inConduitCircuit)) {
            abilityIconInteractable.SetColor(info.colorOverride);
        }
        else if (act?.CurrentColourOverride != null) {
            abilityIconInteractable.SetColor((Color)act.CurrentColourOverride);
        }
        else {
            abilityIconInteractable.SetColor(color);
        }
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.SetColorOfDefaultIcons))]
    private static IEnumerable<CodeInstruction> FixActivatedIconSetColour(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);
        codes.LogCodeInstructions();
        CodeInstruction startCode = codes.Find(x => x.opcode == OpCodes.Stloc_S && x.operand.ToString() == "DiskCardGame.AbilityIconInteractable (4)");
        object operand = startCode.operand;
        int startIndex = codes.FindIndex(x => x.opcode == OpCodes.Stloc_S && x.operand.ToString() == "DiskCardGame.AbilityInfo (5)") - 2;
        int endIndex = codes.FindLastIndex(x => x.opcode == OpCodes.Add) - 3;

        MethodBase customMethod = AccessTools.Method(
            typeof(ActivatedAbilityIconFix), nameof(ActivatedAbilityIconFix.SetAbilityIconDefaultColour),
            new Type[] { typeof(AbilityIconInteractable), typeof(Color), typeof(bool) });

        codes.RemoveRange(startIndex, endIndex - startIndex + 1);
        codes.Insert(startIndex, new(OpCodes.Callvirt, customMethod));
        codes.Insert(startIndex, new(OpCodes.Ldarg_2));
        codes.Insert(startIndex, new(OpCodes.Ldarg_1));
        codes.Insert(startIndex, new(OpCodes.Ldloc_S, operand));

        codes.LogCodeInstructions("post");
        return codes;
    }
}