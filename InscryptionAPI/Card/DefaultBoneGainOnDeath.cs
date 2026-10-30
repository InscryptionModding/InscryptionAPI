using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using MonoMod.Cil;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public class DefaultBoneGainOnDeath {
    
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Die), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> InsertBoneMethodHook(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this

        // if (!HasAbility(Ability.QuadrupleBones) && slotBeforeDeath.IsPlayerSlot) {
        //     yield return Singleton<ResourcesManager>.Instance.AddBones(1, slotBeforeDeath);
        // }

        // === Into this

        // if (ShouldDoDefaultBoneGain(base.Card, slotBeforeDeath) {
        //     yield return DoDefaultBoneGain(base.Card, slotBeforeDeath, 1);
        // }

        // ===
        List<CodeInstruction> codes = new(instructions);

        for (int i = 0; i < codes.Count; i++) {
            // there are two instances where this is true, but we only care about the first instance
            // the second instance will be ignored
            if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand.ToString() == "12") {
                /*
                 * // if (!playableCard.HasAbility(Ability.QuadrupleBones) && <slotBeforeDeath>5__2.IsPlayerSlot)
                 * ldloc.1
                 * ldc.i4.s 12
                 * call instance bool DiskCardGame.PlayableCard::HasAbility(valuetype DiskCardGame.Ability)
                 * brtrue.s IL_022f
                 * 	
                 * ldarg.0
                 * ldfld class DiskCardGame.CardSlot DiskCardGame.PlayableCard/'<Die>d__98'::'<slotBeforeDeath>5__2'
                 * callvirt instance bool DiskCardGame.CardSlot::get_IsPlayerSlot()
                 * brfalse.s IL_022f
                 * 	
                 * =
                 * 	
                 * // if (ShouldDoDefaultBoneGain(playableCard, <slotBeforeDeath>5__2)
                 * ldloc.1
                 * ldarg.0
                 * ldfld class DiskCardGame.CardSlot DiskCardGame.PlayableCard/'<Die>d__98'::'<slotBeforeDeath>5__2'
                 * callvirt ShouldDo...
                 * brfalse.s IL_022f
                 */
                MethodInfo validationMethod = AccessTools.Method(
                    typeof(DefaultBoneGainOnDeath), nameof(ShouldDoDefaultBoneGain),
                    new Type[] { typeof(PlayableCard), typeof(CardSlot) });

                codes.RemoveAt(i);
                codes.RemoveAt(i);
                codes.RemoveAt(i);
                i += 2;
                codes[i] = new CodeInstruction(OpCodes.Callvirt, validationMethod);

                /*
                 * // <>2__current = Singleton<ResourcesManager>.Instance.AddBones(1, <slotBeforeDeath>5__2);
                 * ldarg.0
                 * call !0 class Singleton`1<class DiskCardGame.ResourcesManager>::get_Instance()
                 * ldc.i4.1
                 * ldarg.0
                 * ldfld class DiskCardGame.CardSlot DiskCardGame.PlayableCard/'<Die>d__98'::'<slotBeforeDeath>5__2'
                 * callvirt instance class [netstandard]System.Collections.IEnumerator DiskCardGame.ResourcesManager::AddBones(int32, class DiskCardGame.CardSlot)
                 * stfld object DiskCardGame.PlayableCard/'<Die>d__98'::'<>2__current'
                 * 
                 * =
                 * 
                 * // <>2__current = DoDefaultBoneGain(playableCard, slotBeforeDeath, 1);
                 * ldarg.0
                 * ldloc.1
                 * ldarg.0
                 * ldfld class DiskCardGame.CardSlot DiskCardGame.PlayableCard/'<Die>d__98'::'<slotBeforeDeath>5__2'
                 * ldc.i4.1
                 * callvirt DoDefaultBoneGain
                 * stfld object DiskCardGame.PlayableCard/'<Die>d__98'::'<>2__current'
                 */

                MethodInfo executionMethod = AccessTools.Method(
                    typeof(DefaultBoneGainOnDeath), nameof(DoDefaultBoneGain),
                    new Type[] { typeof(PlayableCard), typeof(CardSlot), typeof(int) });

                i += 3;
                codes[i] = new CodeInstruction(OpCodes.Ldloc_1);
                i++;

                // since codes are taken from the top of the stack, we need to move the ldc.i4.1 to be AFTER we call slotBeforeDeath
                codes.RemoveAt(i);

                i += 2;

                codes.Insert(i, new CodeInstruction(OpCodes.Ldc_I4_1));
                i++;

                codes[i] = new CodeInstruction(OpCodes.Callvirt, executionMethod);
                break;
            }
        }

        return codes;
    }

    public static bool ShouldDoDefaultBoneGain(PlayableCard card, CardSlot slotBeforeDeath) {
        return slotBeforeDeath.IsPlayerSlot && !card.HasAbility(Ability.QuadrupleBones);
    }
    /// <summary>
    /// A simple call to ResourcesManager.AddBones, intended to give modders some more flexibility over the default death behaviour of player cards giving 1 Bone.
    /// </summary>
    /// <param name="card">The PlayableCard that has died.</param>
    /// <param name="slotBeforeDeath">The CardSlot of card.</param>
    /// <param name="amount">How many Bones to drop whenever a player's card dies (default is 1).</param>
    public static IEnumerator DoDefaultBoneGain(PlayableCard card, CardSlot slotBeforeDeath, int amount) {
        yield return Singleton<ResourcesManager>.Instance.AddBones(amount, slotBeforeDeath);
    }

    public static CardInfo LogCardInfo(CardInfo info, string cardInfoName)
    {
        if (info == null)
            InscryptionAPIPlugin.Logger.LogError("[CardLoader] Could not find CardInfo with name '" + cardInfoName + "'");

        return info;
    }
}
