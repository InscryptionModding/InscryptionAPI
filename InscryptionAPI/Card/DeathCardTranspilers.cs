using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Saves;
using MonoMod.Cil;
using Sirenix.Serialization.Utilities;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace InscryptionAPI.Card;

[HarmonyPatch]
internal static class AddCostsToDeathCards
{
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(DeathCardCreationSequencer), nameof(DeathCardCreationSequencer.CreateCardSequence));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*  we want to change this
          
            yield return this.SelectCardFromChoices(costChoices, doTutorialDialogue: true, ChoiceType.Cost, delegate (CardInfo c)
            {
                generatedDeathCardMod.bloodCostAdjustment = c.BloodCost;
                generatedDeathCardMod.bonesCostAdjustment = c.BonesCost;
                });
            
            so that it also sets the costs for Energy, Mox, and any custom costs
            we also want to insert this as the end so death cards with custom costs will work correctly
            
            if (generatedDeathCardMod.HasCustomCosts())
                CardManager.CreateCustomDeathCard(generatedDeathCardMod);

            also-also, we want to replace this

            List<CardInfo> from = list.FindAll(x => x.Abilities.Count > 0 && x.Abilities.Count <= 2);

            with

            from = ... && x.Abilities.Count <= 8);
        */

        List<CodeInstruction> codes = new(instructions);
        int startIdx = -1, endIdx = -1;
        object op_8__1 = null;
        object op_deathCardmod = null;

        for (int ii = 0; ii < codes.Count; ii++)
        {
            if (codes[ii].opcode == OpCodes.Stloc_S && codes[ii].operand.ToString() == "System.Collections.Generic.List`1[DiskCardGame.CardInfo] (5)")
            {
                MethodInfo allowOverTwoSigils = AccessTools.Method(
                    typeof(AddCostsToDeathCards), nameof(AddCostsToDeathCards.AllowOverTwoSigils),
                    new Type[] { typeof(List<CardInfo>) });
                codes.RemoveRange(ii - 10, 10);
                codes.Insert(ii - 10, new(OpCodes.Call, allowOverTwoSigils));
                break;
            }
        }
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand.ToString() == "DiskCardGame.DeathCardCreationSequencer+<>c__DisplayClass18_0 <>8__1")
                op_8__1 = codes[i].operand;

            else if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand.ToString() == "DiskCardGame.CardModificationInfo generatedDeathCardMod")
                op_deathCardmod = codes[i].operand;

            else if (codes[i].opcode == OpCodes.Ldc_I4_5)
            {
                startIdx = i - 5;
                endIdx = i - 2;
                break;
            }
        }

        if (startIdx > -1 && endIdx > -1 && op_deathCardmod != null)
        {
            MethodInfo addAllCosts = AccessTools.Method(
                typeof(AddCostsToDeathCards), nameof(AddCostsToDeathCards.AddAllCosts),
                new Type[] {
                    typeof(DeathCardCreationSequencer), typeof(List<CardInfo>), typeof(bool),
                    typeof(DeathCardCreationSequencer.ChoiceType), typeof(CardModificationInfo)
                });

            codes.RemoveRange(startIdx, endIdx - startIdx);
            codes.Insert(startIdx, new(OpCodes.Ldfld, op_deathCardmod));
            codes.Insert(startIdx + 1, new(OpCodes.Callvirt, addAllCosts));
        }

        startIdx = -1;
        for (int i = codes.Count - 1; i > 0; i--)
        {
            if (codes[i].opcode == OpCodes.Callvirt)
            {
                startIdx = i + 1;
                break;
            }
        }
        if (startIdx > -1 && op_8__1 != null && op_deathCardmod != null)
        {
            MethodInfo createCustomCard = AccessTools.Method(
                typeof(AddCostsToDeathCards), nameof(AddCostsToDeathCards.CreateCustomCard),
                new Type[] {
                    typeof(CardModificationInfo)
                });

            codes.Insert(startIdx, new(OpCodes.Ldarg_0));
            codes.Insert(startIdx + 1, new(OpCodes.Ldfld, op_8__1));
            codes.Insert(startIdx + 2, new(OpCodes.Ldfld, op_deathCardmod));
            codes.Insert(startIdx + 3, new(OpCodes.Call, createCustomCard));
        }
        return codes;
    }
    private static List<CardInfo> AllowOverTwoSigils(List<CardInfo> list) => list.FindAll(x => x.Abilities.Count > 0 && x.Abilities.Count <= 8);
    private static void CreateCustomCard(CardModificationInfo generatedDeathCardMod)
    {
        if (generatedDeathCardMod.HasCustomCostsId())
        {
            CardModificationInfo mod = SaveManager.SaveFile.deathCardMods.Find(x => x == generatedDeathCardMod);
            DeathCardManager.CreateCustomDeathCard(mod);
        }
    }
    private static IEnumerator AddAllCosts(
        DeathCardCreationSequencer instance, List<CardInfo> costChoices, bool doTutorialDialogue,
        DeathCardCreationSequencer.ChoiceType choiceType, CardModificationInfo generatedDeathCardMod)
    {
        yield return instance.SelectCardFromChoices(costChoices, doTutorialDialogue, choiceType, delegate (CardInfo c)
        {
            generatedDeathCardMod.bloodCostAdjustment = c.BloodCost;
            generatedDeathCardMod.bonesCostAdjustment = c.BonesCost;
            generatedDeathCardMod.energyCostAdjustment = c.EnergyCost;
            generatedDeathCardMod.addGemCost = c.GemsCost;

            string customCosts = "";
            foreach (KeyValuePair<string, string> valuePair in c.GetCardExtensionTable())
            {
                if (valuePair.Key.ToLowerInvariant().Contains("cost") && !valuePair.Value.IsNullOrWhitespace())
                {
                    if (customCosts != "")
                        customCosts += "_";

                    customCosts += $"{valuePair.Key},{valuePair.Value}";
                }
            }

            if (customCosts != "")
                generatedDeathCardMod.singletonId = $"[CustomCosts:{customCosts}]";
        });
    }
}

[HarmonyPatch]
internal static class ChangeDeathCardExamineDialogue
{
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(DeathCardCreationSequencer), nameof(DeathCardCreationSequencer.SelectCardFromChoices));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*  we want to change this
         *  
         *  case ChoiceType.Cost:
         *      examinedDialogue = (long-ol' line a code)
         *      break;
         *  
         *  so that it also accounts for Energy, Mox, and custom costs
        */

        List<CodeInstruction> codes = new(instructions);

        //TranspilerHelpers.LogCodeInscryptions(codes);
        int start = -1;
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldstr)
            {
                string opStr = codes[i].operand.ToString();
                if (opStr == "A cost of [c:bR]{0} blood[c:] from the [c:bR]{1}[c:].")
                    start = i - 7;
                else if (opStr == "A cost of... [c:bR]free[c:]... from the [c:bR]{0}[c:].")
                {
                    codes.RemoveRange(start, (i + 2) - start);
                    for (int j = start; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Callvirt && codes[j].operand.ToString() == "System.String get_DisplayedNameLocalized()")
                        {
                            MethodInfo customMethod = AccessTools.Method(
                                typeof(ChangeDeathCardExamineDialogue), nameof(ChangeDeathCardExamineDialogue.NewCostDialogue),
                                new Type[] { typeof(CardInfo) });
                            codes.RemoveRange(j, 2);
                            codes.Insert(j, new(OpCodes.Call, customMethod));
                            break;
                        }
                    }
                    break;
                }
            }

        }
        for (int ii = 0; ii < codes.Count; ii++)
        {
            if (codes[ii].opcode == OpCodes.Stloc_S && codes[ii].operand.ToString() == "System.String (10)")
            {
                codes.Insert(ii - 9, new(OpCodes.Ldarg_0)); // need to add this now so examinedDialogue is set properly
                ii++;
                start = ii - 6;
                continue;
            }
                
            if (codes[ii].opcode == OpCodes.Ldstr && codes[ii].operand.ToString() == "A [c:bR]Sigil of {0}[c:] and a [c:bR]Sigil of {1}[c:] from the [c:bR]{2}[c:].")
            {
                MethodInfo customMethod = AccessTools.Method(
                    typeof(ChangeDeathCardExamineDialogue), nameof(ChangeDeathCardExamineDialogue.NewAbilitiesDialogue),
                    new Type[] { typeof(CardInfo) });

                codes.RemoveRange(start, (ii + 10) - start);
                codes.Insert(start, new(OpCodes.Call, customMethod));
                break;
            }
        }

        return codes;
    }

    private static string NewCostDialogue(CardInfo info)
    {
        string finalDialogue = "A cost of" + "{A}" + " from the [c:bR]{0}[c:].";

        string blood = info.BloodCost <= 0 ? "" : string.Format(" [c:bR]{0} blood[c:]", info.BloodCost);
        string bones = info.BonesCost <= 0 ? "" : (info.BonesCost == 1 ? " [c:bR]1 bone[c:]" : string.Format(" [c:bR]{0} bones[c:]", info.BonesCost));
        string energy = info.EnergyCost <= 0 ? "" : string.Format(" [c:bR]{0} energy[c:]", info.EnergyCost);
        string gems = info.GemsCost.Count <= 0 ? "" : (info.GemsCost.Count == 1 ? "{G}" : string.Format(" [c:bR]{0} gems[c:]", info.GemsCost.Count));
        if (gems == "{G}")
        {
            string c = info.GemsCost.Contains(GemType.Green) ? " green" : (info.GemsCost.Contains(GemType.Blue) ? " blue" : "n orange");
            gems = string.Format(gems.Replace("{G}", $" [c:bR]a{c} gem[c:]"));
        }

        List<string> customCosts = new();
        Dictionary<string, string> properties = info.GetCardExtensionTable();
        if (properties.Count > 0)
        {
            foreach (KeyValuePair<string, string> valuePair in properties)
            {
                if (valuePair.Key.ToLowerInvariant().Contains("cost") && !valuePair.Value.IsNullOrWhitespace())
                {
                    string lowerKey = Regex.Replace(valuePair.Key, "(\\B[A-Z])", " $1").ToLowerInvariant();
                    lowerKey = " [c:bR]{0} " + lowerKey.Replace(" cost", "") + "[c:]";

                    customCosts.Add(string.Format(lowerKey, valuePair.Value));
                }
            }
        }

        List<string> costs = new() { blood, bones, energy, gems };
        customCosts.ForEach(x => costs.Add(x));
        costs.RemoveAll(x => x == "");

        // if there are no costs
        if (costs.Count == 0)
            return string.Format(Localization.Translate(finalDialogue.Replace("{A}", "... [c:bR]free[c:]...")), info.DisplayedNameLocalized);

        string totalCosts = $"{costs.PopFirst()}";

        if (costs.Count == 0)
            return string.Format(Localization.Translate(finalDialogue.Replace("{A}", totalCosts)), info.DisplayedNameLocalized);

        if (costs.Count > 1)
        {
            totalCosts += ",";

            for (int i = 0; i < costs.Count - 1; i++)
                totalCosts += costs[i] + ",";
        }

        totalCosts += " and" + costs.Last();
        return string.Format(Localization.Translate(finalDialogue.Replace("{A}", totalCosts)), info.DisplayedNameLocalized);
    }

    private static string NewAbilitiesDialogue(CardInfo info)
    {
        string arg = Localization.Translate(AbilitiesUtil.GetInfo(info.Abilities[0]).rulebookName);
        if (info.NumAbilities == 1)
            return string.Format(Localization.Translate("A [c:bR]Sigil of {0}[c:] from the [c:bR]{1}[c:]."), arg, info.DisplayedNameLocalized);

        if (info.NumAbilities > 1)
        {
            string arg2 = Localization.Translate(AbilitiesUtil.GetInfo(info.Abilities[1]).rulebookName);
            if (info.NumAbilities == 2)
                return string.Format(Localization.Translate("A [c:bR]Sigil of {0}[c:] and a [c:bR]Sigil of {1}[c:] from the [c:bR]{2}[c:]."), arg, arg2, info.DisplayedNameLocalized);

            string arg3 = Localization.Translate(AbilitiesUtil.GetInfo(info.Abilities[2]).rulebookName);
            if (info.NumAbilities == 3)
                return string.Format(Localization.Translate("A [c:bR]Sigil of {0}[c:], a [c:bR]Sigil of {1}[c:], and a [c:bR]Sigil of {2}[c:] from the [c:bR]{3}[c:]."), arg, arg2, arg3, info.DisplayedNameLocalized);
          
        }

        return string.Format(Localization.Translate("A [c:bR]multitude of sigils[c:] from the [c:bR]{0}[c:]."), info.DisplayedNameLocalized);
    }
}