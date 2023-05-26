using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Changes the Rulebook to display the correct number of turns for Fledgling

[HarmonyPatch]
internal static class EvolveDescriptionFixes
{
    private static int EvolveTurns = -1;

    #region Rulebook
    [HarmonyPostfix, HarmonyPatch(typeof(AbilityInfo), nameof(AbilityInfo.LocalizedRulebookDescription), MethodType.Getter)]
    private static void ChangeLocalisedDescription(AbilityInfo __instance, ref string __result)
    {
        if (__instance.ability == Ability.Evolve && EvolveTurns != -1)
            __result = LocalizedReplacement(__result, EvolveTurns.ToString());
    }
    [HarmonyPrefix, HarmonyPatch(typeof(RuleBookController), nameof(RuleBookController.SetShown))]
    private static void ResetAlteredDescriptions(bool shown)
    {
        if (!shown)
            EvolveTurns = -1;
    }
    [HarmonyPrefix, HarmonyPatch(typeof(RuleBookController), nameof(RuleBookController.OpenToAbilityPage))]
    private static void UpdateRulebookEvolve(PlayableCard card)
    {
        if (card && card.HasAbility(Ability.Evolve))
        {
            int turnsToEvolve = GetTurnsToEvolve(card, card.Info);
            if (turnsToEvolve == 1)
                return;

            EvolveTurns = turnsToEvolve;
        }
    }
    #endregion

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
    [HarmonyPostfix]
    private static IEnumerator UpdatePreviewOnUpkeep(IEnumerator enumerator)
    {
        yield return enumerator;
        if (SaveManager.SaveFile.IsPart2)
        {
            foreach (CardSlot slot in Singleton<PixelBoardManager>.Instance.AllSlotsCopy.Where(x => x.Card && x.Card.HasAbility(Ability.Evolve)))
            {
                // since the preview box doesn't store the specific card instance
                // this is the best we can do in terms of dynamically updating it
                // jank guaranteed, sorry-not-really
                if (CardPreviewPanel.Instance.nameText.Text == slot.Card.Info.DisplayedNameLocalized)
                {
                    Singleton<CardPreviewPanel>.Instance.DisplayCard(slot.Card.RenderInfo, slot.Card);
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardPreviewPanel), nameof(CardPreviewPanel.DisplayCard))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> DisplayBetterEvolveDescription(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        for (int i = codes.Count - 1; i >= 0; i--)
        {
            if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == "Void DisplayDescription(DiskCardGame.CardInfo, System.Collections.Generic.List`1[DiskCardGame.Ability])")
            {
                MethodBase customMethod = AccessTools.Method(typeof(EvolveDescriptionFixes), nameof(EvolveDescriptionFixes.DisplayDescription),
                    new Type[] { typeof(CardPreviewPanel), typeof(CardInfo), typeof(List<Ability>), typeof(PlayableCard) });
                codes[i] = new(OpCodes.Call, customMethod);
                codes.Insert(i, new(OpCodes.Ldarg_2));
                break;
            }
        }

        return codes;
    }

    private static void DisplayDescription(CardPreviewPanel instance, CardInfo cardInfo, List<Ability> abilities, PlayableCard card)
    {
        instance.DisplayDescription(cardInfo, abilities);
        string gbcDescriptionLocalized = GetUpdatedDescription(instance.descriptionText.hiddenText.Text, card, cardInfo, abilities);

        if (gbcDescriptionLocalized != instance.descriptionText.hiddenText.Text)
            instance.descriptionText.SetText(gbcDescriptionLocalized);
    }

    private static string LocalizedReplacement(string str, string turns)
    {
        string str2 = Localization.CurrentLanguage switch
        {
            Language.English => str.Replace("turn", "turns"),
            Language.French => str.Replace("tour", "tours"),
            Language.Italian => str.Replace("turno", "turni"),
            Language.German => str.Replace("runde", "runden"),
            Language.Spanish => str.Replace("turno", "turnos"),
            Language.BrazilianPortuguese => str.Replace("turno", "turnos"),
            _ => str,
        };
        return Localization.CurrentLanguage switch
        {
            Language.BrazilianPortuguese => str2.Replace("um", turns),
            Language.Korean => str2.Replace("한", turns),
            _ => str2.Replace("1", turns),
        };
    }

    private static string GetUpdatedDescription(string result, PlayableCard card, CardInfo info, List<Ability> abilities)
    {
        if (abilities.Contains(Ability.Evolve))
        {
            int turnsToEvolve = GetTurnsToEvolve(card, info);

            if (turnsToEvolve == 1)
                return result;

            string evolveDescription = info.FormatDescriptionText(
                AbilitiesUtil.GetInfo(Ability.Evolve).LocalizedRulebookDescription, info.DisplayedNameLocalized);

            string newEvolveDescription = LocalizedReplacement(evolveDescription, turnsToEvolve.ToString());

            return result.Replace(evolveDescription, newEvolveDescription);
        }

        return result;
    }

    private static int GetTurnsToEvolve(PlayableCard card, CardInfo cardInfo)
    {
        int turnsInPlay = card?.GetComponentInChildren<Evolve>()?.numTurnsInPlay ?? 0;
        return Mathf.Max(1, (cardInfo.evolveParams == null ? 1 : cardInfo.evolveParams.turnsToEvolve) - turnsInPlay);
    }
}