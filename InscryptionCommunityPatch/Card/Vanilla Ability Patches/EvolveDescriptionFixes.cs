using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal static class EvolveDescriptionFixes
{
    private static int EvolveTurns = -1;

    [HarmonyPostfix, HarmonyPatch(typeof(AbilityInfo), nameof(AbilityInfo.LocalizedRulebookDescription), MethodType.Getter)]
    private static void ChangeLocalisedDescription(AbilityInfo __instance, ref string __result)
    {
        if (__instance.ability == Ability.Evolve && EvolveTurns != -1)
            __result = LocalizedReplacement(__result, EvolveTurns.ToString());
    }
    [HarmonyPrefix, HarmonyPatch(typeof(RuleBookController), nameof(RuleBookController.SetShown))]
    private static bool ResetAlteredDescriptions(bool shown)
    {
        if (!shown)
            EvolveTurns = -1;

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(RuleBookController), nameof(RuleBookController.OpenToAbilityPage))]
    private static bool UpdateRulebookEvolve(PlayableCard card)
    {
        if (card && card.HasAbility(Ability.Evolve))
        {
            int turnsToEvolve = GetTurnsToEvolve(card);
            if (turnsToEvolve == 1)
                return true;

            EvolveTurns = turnsToEvolve;
        }
        return true;
    }

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
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        for (int i = codes.Count - 1; i >= 0; i--)
        {
            if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == "Void DisplayDescription(DiskCardGame.CardInfo, System.Collections.Generic.List`1[DiskCardGame.Ability])")
            {
                MethodBase customMethod = AccessTools.Method(typeof(EvolveDescriptionFixes), nameof(EvolveDescriptionFixes.DisplayBetterDescription),
                    new Type[] { typeof(CardPreviewPanel), typeof(CardInfo), typeof(List<Ability>), typeof(PlayableCard) });
                codes[i] = new(OpCodes.Call, customMethod);
                codes.Insert(i, new(OpCodes.Ldarg_2));
                break;
            }
        }

        return codes;
    }

    private static string LocalizedReplacement(string str, string turns)
    {
        string str2 = LocalizedTurn(str);
        return LocalizedNum(str2, turns);
    }
    private static string LocalizedNum(string str, string turns)
    {
        return Localization.CurrentLanguage switch
        {
            Language.BrazilianPortuguese => str.Replace("um", turns),// don't know why this uses the phonetic forme when basically every other languages is just the numeral
            Language.Korean => str.Replace("한", turns),// this might break the grammar, I apologise
            _ => str.Replace("1", turns),
        };
    }
    private static string LocalizedTurn(string str)
    {
        // Turkist, Russian, Japanese, Chinese Simplified I think will be fine left as-is?
        // Korean, Chinese I have no idea
        return Localization.CurrentLanguage switch
        {
            Language.English => str.Replace("turn", "turns"),
            Language.French => str.Replace("tour", "tours"),
            Language.Italian => str.Replace("turno", "turni"),
            Language.German => str.Replace("runde", "runden"),
            Language.Spanish => str.Replace("turno", "turnos"),
            Language.BrazilianPortuguese => str.Replace("turno", "turnos"),
            _ => str,
        };
    }
    private static int GetTurnsToEvolve(PlayableCard card)
    {
        int turnsInPlay = card.GetComponentInChildren<Evolve>()?.numTurnsInPlay ?? 0;
        return Mathf.Max(1, (card.Info.evolveParams == null ? 1 : card.Info.evolveParams.turnsToEvolve) - turnsInPlay);
    }

    private static void DisplayBetterDescription(CardPreviewPanel instance, CardInfo cardInfo, List<Ability> abilities, PlayableCard card)
    {
        instance.glitchedDescription.SetActive(cardInfo.name == "!CORRUPTED");
        string gBCDescriptionLocalized = GetBetterGBCDescription(card, cardInfo, abilities);
        instance.descriptionText.SetText(gBCDescriptionLocalized);
    }
    private static string GetBetterGBCDescription(PlayableCard card, CardInfo info, List<Ability> abilities)
    {
        string result = info.GetGBCDescriptionLocalized(abilities);

        if (abilities.Contains(Ability.Evolve))
        {
            int turnsToEvolve = GetTurnsToEvolve(card);

            if (turnsToEvolve == 1)
                return result;

            string evolveDescription = info.FormatDescriptionText(
                AbilitiesUtil.GetInfo(Ability.Evolve).LocalizedRulebookDescription, info.DisplayedNameLocalized);

            string newEvolveDescription = evolveDescription.Replace("1", turnsToEvolve.ToString()).Replace("turn", "turns");

            result = result.Replace(evolveDescription, newEvolveDescription);
        }

        return result;
    }
}