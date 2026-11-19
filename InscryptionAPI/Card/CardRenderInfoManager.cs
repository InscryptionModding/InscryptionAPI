using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InscryptionAPI.Card;

/// <summary>
/// Helper class that adds patchable methods for easily modifying a card's <see cref="CardRenderInfo"/> data during runtime.
/// </summary>
/// <remarks>
/// Currently adds support for:<br/>
/// modifying the attack text colour used in <see cref="PlayableCard.OnStatsChanged"/><br/>
/// modifying the health text colour used in <see cref="CardDisplayer.SetTextColors(CardRenderInfo, PlayableCard)"/>
/// </remarks>
[HarmonyPatch]
public static class CardRenderInfoManager {
    /// <summary>
    /// This event runs every time <see cref="PlayableCard.OnStatsChanged"/> is called.
    /// By adding listeners to this event, you can modify what colour cards' attack text is.
    /// </summary>
    /// <remarks>
    /// Explanation of Func variables:<br/>
    /// <list type="table">
    ///     <item><term><c><see cref="Color"/> defaultCol</c></term><description>the vanilla attack text colour.</description></item>
    ///     <item><term><c><see cref="PlayableCard"/> card</c></term><description>the card whose attack text colour is being modified.</description></item>
    ///     <item><term><c><see cref="bool"/> hasPassiveBuffs</c></term><description>the return value of <see cref="HasAttackBuffs(PlayableCard)"/>.</description></item>
    /// </list>
    /// Returns the <see cref="Color"/> that the card's attack text will be set to.
    /// </remarks>
    public static event Func<Color, PlayableCard, bool, Color> ModifyAttackTextColour;
    /// <summary>
    /// This event runs every time <see cref="CardDisplayer.SetTextColors(CardRenderInfo, PlayableCard)"/> is called.
    /// By adding listeners to this event, you can modify what colour cards' health text is.
    /// </summary>
    /// <remarks>
    /// Explanation of Func variables:<br/>
    /// <list type="table">
    ///     <item><term><c><see cref="Color"/> defaultCol</c></term><description>the vanilla attack text colour.</description></item>
    ///     <item><term><c><see cref="PlayableCard"/> card</c></term><description>the card whose attack text colour is being modified.</description></item>
    ///     <item><term><c><see cref="CardRenderInfo"/> renderInfo</c></term><description>the render info for the current <see cref="CardDisplayer"/>.</description></item>
    ///     <item><term><c><see cref="bool"/> isUninjured</c></term><description>If the card's health is greater or equal to its max. If card is null, this is true if the render info's current health is greater or equal to the base info's health.</description></item>
    /// </list>
    /// Returns the <see cref="Color"/> that the card's health text will be set to.
    /// </remarks>
    public static event Func<Color, PlayableCard, CardRenderInfo, bool, Color> ModifyHealthTextColour;

    /// <summary>
    /// Determines if the given <see cref="PlayableCard"/> has any attack buffs from passive attack sigils or stat icons.
    /// </summary>
    /// <param name="card">The <see cref="PlayableCard"/> to check.</param>
    /// <returns>True if the sum of this card's passive attack buffs is NOT zero.</returns>
    public static bool HasAttackBuffs(PlayableCard card) {
        return (card.GetPassiveAttackBuffs() + card.GetStatIconAttackBuffs()) != 0;
    }
    /// <summary>
    /// Updates the attack text colour when <see cref="PlayableCard.OnStatsChanged"/> is called.
    /// </summary>
    /// <param name="card">The <see cref="PlayableCard"/> to check.</param>
    /// <returns>The colour to set the attack text to.</returns>
    public static Color UpdateAttackColour(PlayableCard card) {
        bool hasPassiveAttackBuffs = HasAttackBuffs(card);
        Color defaultColour = hasPassiveAttackBuffs ? GameColors.Instance.darkBlue : Color.black;

        if (ModifyAttackTextColour != null) {
            return ModifyAttackTextColour.Invoke(defaultColour, card, hasPassiveAttackBuffs);
        }
        return defaultColour;
    }

    /// <summary>
    /// Updates the attack text colour when <see cref="CardDisplayer.SetTextColors(CardRenderInfo, PlayableCard)"/> is called.
    /// </summary>
    /// <param name="isUninjured">If <paramref name="card"/> or <paramref name="renderInfo"/>'s health is greater or equal to its max or base health respectively.</param>
    /// <param name="card">The <see cref="PlayableCard"/> to check. Can be null.</param>
    /// <param name="renderInfo">The render info of the card being rendered.</param>
    /// <returns>The colour to set the health text to.</returns>
    public static Color UpdateHealthColour(CardRenderInfo renderInfo, PlayableCard card, bool isUninjured) {
        Color defaultColour = isUninjured ? renderInfo.healthTextColor : GameColors.Instance.darkFuschia;

        if (ModifyHealthTextColour != null) {
            return ModifyHealthTextColour.Invoke(defaultColour, card, renderInfo, isUninjured);
        }
        return defaultColour;
    }

    #region Transpilers
    [HarmonyTranspiler, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.OnStatsChanged))]
    private static IEnumerable<CodeInstruction> AddHookToModifyAttackTextColour(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);

        MethodBase customMethod = AccessTools.Method(typeof(CardRenderInfoManager), nameof(CardRenderInfoManager.UpdateAttackColour),
                            new Type[] { typeof(PlayableCard) });

        int startIndex = codes.FindIndex(
            x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == "Int32 GetPassiveAttackBuffs()");
        
        int endIndex = codes.FindIndex(
            x => x.opcode == OpCodes.Stfld && x.operand.ToString() == "UnityEngine.Color attackTextColor");

        codes.RemoveRange(startIndex, endIndex - startIndex);
        codes.Insert(startIndex, new(OpCodes.Callvirt, customMethod));

        return codes;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(CardDisplayer), nameof(CardDisplayer.SetTextColors))]
    private static IEnumerable<CodeInstruction> AddHookToModifyHealthTextColour(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);

        MethodBase customMethod = AccessTools.Method(typeof(CardRenderInfoManager), nameof(CardRenderInfoManager.UpdateHealthColour),
                            new Type[] { typeof(CardRenderInfo), typeof(PlayableCard), typeof(bool) });

        int startIndex = codes.FindIndex(x => x.opcode == OpCodes.Ldloc_0) - 1;

        int endIndex = codes.FindIndex(
            x => x.opcode == OpCodes.Callvirt && x.operand.ToString() == "Void SetHealthTextColor(UnityEngine.Color)");

        codes.RemoveRange(startIndex, endIndex - startIndex);
        codes.Insert(startIndex, new(OpCodes.Callvirt, customMethod));
        codes.Insert(startIndex, new(OpCodes.Ldloc_0));
        codes.Insert(startIndex, new(OpCodes.Ldarg_2));
        codes.Insert(startIndex, new(OpCodes.Ldarg_1));
        codes.Insert(startIndex, new(OpCodes.Ldarg_0));

        return codes;
    }
    #endregion
}
