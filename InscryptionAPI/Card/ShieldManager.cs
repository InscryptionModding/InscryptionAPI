using BepInEx.Logging;
using DiskCardGame;
using DiskCardGame.CompositeRules;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class ShieldManager
{
    /// <summary>
    /// The method used for when a shielded card is damaged. Includes extra parameters for modders looking to modify this further.
    /// This method is only called when damage > 0 and the target has a shield.
    /// </summary>
    public static void BreakShield(PlayableCard target, int damage, PlayableCard attacker)
    {
        DamageShieldBehaviour shield = Array.Find(target.GetComponents<DamageShieldBehaviour>(), x => x.HasShields());
        if (shield != null)
        {
            if (target.TemporaryMods.Exists(x => x.abilities != null && x.abilities.Contains(shield.Ability))) // if the sigil is from a temp mod
            {
                Ability ability = shield.Ability;
                CardModificationInfo info = target.TemporaryMods.Find(x => x.abilities != null && x.abilities.Contains(shield.Ability));
                target.RemoveTemporaryMod(info, false); // RemoveShields is called here in a patch
                
                int shieldsToAdd = shield.NumShields - info.abilities.Count(x => x == shield.Ability);
                if (shieldsToAdd > 0)
                {
                    CardModificationInfo updatedinfo = new() { fromCardMerge = info.fromCardMerge, fromLatch = info.fromLatch, fromTotem = info.fromTotem };
                    for (int i = 0; i < shieldsToAdd; i++)
                        updatedinfo.abilities.Add(ability);

                    target.AddTemporaryMod(updatedinfo);
                }
            }
            else
            {
                shield.RemoveShields(1, false);
            }
        }
        if (target.GetTotalShields() == 0) // if we removed the last shield
        {
            target.Status.lostShield = true;
            if (target.Info.HasBrokenShieldPortrait())
                target.SwitchToPortrait(target.Info.BrokenShieldPortrait());
        }
        target.OnStatsChanged();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(LatchDeathShield), nameof(LatchDeathShield.OnSuccessfullyLatched))]
    private static bool PreventShieldReset() => false; // latch death shield doesn't reset shields

    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.HasShield))]
    private static void ReplaceHasShieldBool(PlayableCard __instance, ref bool __result) => __result = NewHasShield(__instance);
    /// <summary>
    /// The new version of PlayableCard.HasShield implementing the new shield logic.
    /// </summary>
    public static bool NewHasShield(PlayableCard instance) => instance.GetTotalShields() > 0 && !instance.Status.lostShield;

    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.ResetShield), new Type[] { })]
    private static void ResetModShields(PlayableCard __instance) // runs before the base ResetShield logic
    {
        foreach (var com in __instance.GetComponents<DamageShieldBehaviour>())
            com.ResetShields(false);

        // if we're using the broken shield portrait, reset to the default portrait - if we're MudTurtle
        if ((__instance.Info.BrokenShieldPortrait() != null && __instance.RenderInfo.portraitOverride == __instance.Info.BrokenShieldPortrait()) || __instance.Info.name == "MudTurtle")
            __instance.SwitchToDefaultPortrait();
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(ShieldGeneratorItem), nameof(ShieldGeneratorItem.ActivateSequence), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> DontResetOnActivate(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        int index = codes.FindIndex(x => x.operand?.ToString() == "DiskCardGame.BoardManager get_Instance()");
        if (index > 0)
        {
            MethodBase method = AccessTools.Method(typeof(ShieldManager), nameof(ShieldManager.EmptyList));
            codes.RemoveRange(index, 2);
            codes.Insert(index, new(OpCodes.Callvirt, method));
        }
        return codes;
    }
    [HarmonyTranspiler, HarmonyPatch(typeof(ShieldGems), nameof(ShieldGems.OnResolveOnBoard), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> DontResetOnResolve(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        int index = codes.FindLastIndex(x => x.operand?.ToString() == "0.25") - 4;
        if (index > 0)
        {
            MethodBase method = AccessTools.Method(typeof(ShieldManager), nameof(ShieldManager.EmptyList));
            codes.RemoveRange(index, 2);
            codes.Insert(index, new(OpCodes.Callvirt, method));
        }
        return codes;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(RandomCardGainsShieldEffect), nameof(RandomCardGainsShieldEffect.Execute), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> DontResetOnExecute(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        int index = codes.FindLastIndex(x => x.opcode == OpCodes.Newobj);
        if (index > 0)
        {
            MethodBase method = AccessTools.Method(typeof(ShieldManager), nameof(ShieldManager.EmptyList));
            codes.RemoveRange(index, 4);
            codes.Insert(index, new(OpCodes.Callvirt, method));
        }
        return codes;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.UpdateFaceUpOnBoardEffects))]
    private static IEnumerable<CodeInstruction> BetterHideShieldLogic(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        int start = codes.IndexOf(codes.Find(x => x.operand?.ToString() == "DiskCardGame.PlayableCardStatus get_Status()"));
        int end = codes.IndexOf(codes.Find(x => x.operand?.ToString() == "Void RenderCard()")) + 1;

        if (start > 0 && end > 0)
        {
            MethodBase method = AccessTools.Method(typeof(ShieldManager), nameof(ShieldManager.CorrectHiddenAbilityRender),
                new Type[] { typeof(PlayableCard) });

            codes.RemoveRange(start, end - start);
            codes.Insert(start, new(OpCodes.Call, method));
        }

        return codes;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.GetDistinctShownAbilities))]
    private static IEnumerable<CodeInstruction> RemoveSingleHiddenStacks(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        int start = -1, end = -1;
        object hidden = null;

        for (int i = codes.Count - 1; i >= 0; i--)
        {
            if (end == -1 && codes[i].opcode == OpCodes.Pop)
                end = i + 1;

            else if (start == -1 && codes[i].opcode == OpCodes.Ldftn)
                start = i;

            else if (hidden == null && codes[i].opcode == OpCodes.Ldfld)
            {
                hidden = codes[i].operand;
                break;
            }
        }

        if (start > 0 && end > 0)
        {
            MethodBase method = AccessTools.Method(typeof(ShieldManager), nameof(ShieldManager.HiddensOnlyRemoveStacks),
                new Type[] { typeof(List<Ability>), typeof(List<Ability>) });

            codes.RemoveRange(start, end - start);
            codes.Insert(start++, new(OpCodes.Ldfld, hidden));
            codes.Insert(start++, new(OpCodes.Callvirt, method));
            codes.Insert(start++, new(OpCodes.Stloc_1));
        }

        return codes;
    }

    private static List<CardSlot> EmptyList() => new(); // for transpiler logic (why can't i just pass an empty list?)

    private static List<Ability> HiddensOnlyRemoveStacks(List<Ability> abilities, List<Ability> hiddenAbilities)
    {
        // remove all abilities that hide the entire stack
        abilities.RemoveAll(x => hiddenAbilities.Contains(x) && !x.GetHideSingleStacks());
        foreach (var ab in hiddenAbilities.Where(x => x.GetHideSingleStacks()))
            abilities.Remove(ab);
        return abilities;
    }
    private static void CorrectHiddenAbilityRender(PlayableCard card)
    {
        foreach (var com in card.GetComponents<DamageShieldBehaviour>())
        {
            //Debug.Log($"Hidden start: {card.Status.hiddenAbilities.Count(x => x == com.Ability)}");
            if (com.HasShields())
            {
                if (com.Ability.GetHideSingleStacks())
                {
                    // if there are more hidden shields than there should be
                    if (com.NumShields <= card.Status.hiddenAbilities.Count(x => x == com.Ability))
                    {
                        for (int i = 0; i < com.NumShields; i++)
                        {
                            card.Status.hiddenAbilities.Remove(com.Ability);
                        }
                    }
                }   
                else
                {
                    card.Status.hiddenAbilities.Remove(com.Ability);
                }
                //Debug.Log($"Hidden Removed: {card.Status.hiddenAbilities.Count(x => x == com.Ability)}");
                break;
            }
            else
            {
                if (com.Ability.GetHideSingleStacks())
                {
                    int shieldsLost = com.StartingNumShields - com.NumShields;
                    if (card.Status.hiddenAbilities.Count(x => x == com.Ability) < shieldsLost)
                    {
                        // if there are less hidden shields than there should be
                        for (int i = 0; i < shieldsLost; i++)
                        {
                            card.Status.hiddenAbilities.Add(com.Ability);
                        }
                    }
                }
                else if (!card.Status.hiddenAbilities.Contains(com.Ability))
                {
                    card.Status.hiddenAbilities.Add(com.Ability);
                }
                //Debug.Log($"Hidden Added: {card.Status.hiddenAbilities.Count(x => x == com.Ability)}");
                break;
            }
        }
        card.RenderCard();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AddTemporaryMod))]
    private static void AddTemporaryShieldCount(PlayableCard __instance, CardModificationInfo mod)
    {
        foreach (Ability ability in mod.abilities)
        {
            // check that this ability is associated with a shield-giving sigil
            if (AbilityManager.AllAbilities.AbilityByID(ability).AbilityBehavior?.IsSubclassOf(typeof(DamageShieldBehaviour)) ?? false)
            {
                // if this card already has an instance of the behaviour, update that behaviour's shield count
                if (__instance.TriggerHandler.triggeredAbilities.Exists(x => x.Item1 == ability))
                {
                    DamageShieldBehaviour behaviour = __instance.TriggerHandler.triggeredAbilities.Find(x => x.Item1 == ability).Item2 as DamageShieldBehaviour;
                    behaviour.AddShields(1);
                    //InscryptionAPIPlugin.Logger.LogInfo($"Add: {__instance.Info.name} {behaviour.NumShields} <-- {behaviour.NumShields - 1}");
                }
            }
        }
    }
    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.RemoveTemporaryMod))]
    private static void RemoveTemporaryShieldCount(PlayableCard __instance, CardModificationInfo mod)
    {
        foreach (Ability ability in mod.abilities)
        {
            // check that this ability is associated with a shield-giving sigil
            if (AbilityManager.AllAbilities.AbilityByID(ability).AbilityBehavior?.IsSubclassOf(typeof(DamageShieldBehaviour)) ?? false)
            {
                // if this card has an instance of the behaviour, update that behaviour's shield count
                if (__instance.TriggerHandler.triggeredAbilities.Exists(x => x.Item1 == ability))
                {
                    DamageShieldBehaviour behaviour = __instance.TriggerHandler.triggeredAbilities.Find(x => x.Item1 == ability).Item2 as DamageShieldBehaviour;
                    behaviour.RemoveShields(1);
                    //Debug.Log($"Remove: {__instance.Info.name} {behaviour.NumShields} <-- {behaviour.NumShields + 1}");
                }
            }
        }
    }
    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.RemoveTemporaryMod))]
    private static void ClearTemporaryLatchSigils(PlayableCard __instance, CardModificationInfo mod)
    {
        // play the ClearLatchAbility animation for Act 3 if this is the last latch sigil
        if (mod.fromLatch && !__instance.AllCardModificationInfos().Exists(x => x.fromLatch))
            __instance.StartCoroutine(__instance.Anim.ClearLatchAbility());
    }

    [HarmonyPostfix, HarmonyPatch(typeof(DiskCardAnimationController), nameof(DiskCardAnimationController.ClearLatchAbility))]
    private static IEnumerator DisableLatchModule(IEnumerator result, DiskCardAnimationController __instance)
    {
        yield return result;
        if (__instance.latchModule != null)
        {
            __instance.latchModule.gameObject.SetActive(false); // disable the module object so it doesn't replay the animation on death
            GameObject baseObj = __instance.latchModule.gameObject.FindChild("Base");
            if (baseObj != null)
            {
                // fixes the latch module animation when re-applying latch sigil to a card
                baseObj.SetActive(true);
                baseObj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }
    }
}