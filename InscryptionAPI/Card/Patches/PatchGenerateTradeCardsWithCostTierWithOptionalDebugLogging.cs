using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using UnityEngine;

namespace InscryptionAPI.Card.Patches;

/// <summary>
/// A Patch in which injects Loggers and Validation Check Logging into 'GenerateTradeCardsWithCostTier' before it runs to try and debug errors related to it.
/// </summary>
[HarmonyPatch(typeof(TradeCardsForPelts), "GenerateTradeCardsWithCostTier")]
internal static class PatchGenerateTradeCardsWithCostTierWithOptionalDebugLogging
{
    /// <summary>
    /// Makes our system Log 'GenerateTradeCardsWithCostTier' related issues.
    /// </summary>
    /// <param name="numCards">The Number Of Cards to Generate.</param>
    /// <param name="tier">The Tier of Cards that should be Generated.</param>
    /// <param name="randomSeed">The RandomSeed associated with the Generation.</param>
    [HarmonyPrefix]
    private static void Prefix(int numCards, int tier, int randomSeed)
    {
        try
        {
            LogGenerateTradeCardsWithCostTierState(numCards, tier, randomSeed);
        }
        catch (Exception ex)
        {
            if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
            {
                InscryptionAPI.InscryptionAPIPlugin.Logger.LogError($"[TradeCardsForPelts Debug] Prefix logging failed. " + $"The original GenerateTradeCardsWithCostTier method will still run. " + $"Exception: {ex}");
            }
        }
    }

    /// <summary>
    /// A function use to Log relevant info and checks for 'GenerateTradeCardsWithCostTier'.
    /// </summary>
    /// <param name="numCards">The Number Of Cards to Generate.</param>
    /// <param name="requestedTier">The Tier of Cards that should be Generated.</param>
    /// <param name="randomSeed">The RandomSeed associated with the Generation.</param>
    private static void LogGenerateTradeCardsWithCostTierState(int numCards, int requestedTier, int randomSeed)
    {
        bool flag = requestedTier > 0;
        int effectiveTier = Mathf.Max(1, requestedTier);

        if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
        {
            InscryptionAPI.InscryptionAPIPlugin.Logger.LogDebug($"===== GenerateTradeCardsWithCostTier pre-run ===== " + $"numCards={numCards}, " + $"requestedTier={requestedTier}, " + $"effectiveTier={effectiveTier}, " + $"randomSeed={randomSeed}, " + $"flag={flag}");
        }
        List<CardInfo> learnedCards = CardLoader.LearnedCards;

        if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
        {
            InscryptionAPI.InscryptionAPIPlugin.Logger.LogDebug($"[TradeCardsForPelts Debug] LearnedCards count before vanilla filtering: {learnedCards.Count}");
        }
        for (int index = 0; index < learnedCards.Count; index++)
        {
            CardInfo card = learnedCards[index];

            if (card == null)
            {
                if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
                {
                    InscryptionAPI.InscryptionAPIPlugin.Logger.LogError($"[TradeCardsForPelts Debug] NULL CardInfo in CardLoader.LearnedCards " + $"at index={index}.");
                }
                continue;
            }

            CardTemple temple;
            int costTier;

            try
            {
                temple = card.temple;
                costTier = card.CostTier;
            }
            catch (Exception ex)
            {
                if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
                {
                    InscryptionAPI.InscryptionAPIPlugin.Logger.LogError($"[TradeCardsForPelts Debug] Failed while evaluating vanilla outer predicate | " + $"index={index} | " + $"card={GetCardName(card)} | " + $"exception={ex}");
                }
                continue;
            }
            
            if (temple != CardTemple.Nature || costTier != effectiveTier)
                continue;

            if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
            {
                InscryptionAPI.InscryptionAPIPlugin.Logger.LogDebug($"[TradeCardsForPelts Debug] Vanilla ability-filter candidate | " + $"index={index} | " + $"card={GetCardName(card)} | " + $"temple={temple} | " + $"costTier={costTier} | " + $"baseAbilities=[{FormatAbilities(card.abilities)}] | " + $"mods={card.mods?.Count ?? 0}");
            }
            List<Ability> effectiveAbilities;

            try
            {
                effectiveAbilities = card.Abilities;
            }
            catch (Exception ex)
            {
                if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
                {
                    InscryptionAPI.InscryptionAPIPlugin.Logger.LogError($"[TradeCardsForPelts Debug] card.Abilities threw while evaluating the " + $"same property vanilla will use | " + $"index={index} | " + $"card={GetCardName(card)} | " + $"exception={ex}");
                }
                continue;
            }

            if (effectiveAbilities == null)
            {
                if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
                {
                    InscryptionAPI.InscryptionAPIPlugin.Logger.LogError($"[TradeCardsForPelts Debug] NULL effective CardInfo.Abilities list | " + $"index={index} | " + $"card={GetCardName(card)} | " + $"tier={effectiveTier} | " + $"mods={card.mods?.Count ?? 0}");
                }
                continue;
            }
            if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
            {
                InscryptionAPI.InscryptionAPIPlugin.Logger.LogDebug($"[TradeCardsForPelts Debug] Effective abilities | " + $"index={index} | " + $"card={GetCardName(card)} | " + $"abilities=[{FormatAbilities(effectiveAbilities)}]");
            }
            for (int abilityIndex = 0; abilityIndex < effectiveAbilities.Count; abilityIndex++)
            {
                Ability ability = effectiveAbilities[abilityIndex];
                string abilityName = GetGuidAndKey(ability);

                AbilityInfo abilityInfo;

                try
                {
                    abilityInfo = AbilitiesUtil.GetInfo(ability);
                }
                catch (Exception ex)
                {
                    if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
                    {
                        InscryptionAPI.InscryptionAPIPlugin.Logger.LogError($"[TradeCardsForPelts Debug] AbilitiesUtil.GetInfo threw | " + $"card={GetCardName(card)} | " + $"tier={effectiveTier} | " + $"abilityIndex={abilityIndex} | " + $"ability={abilityName} | " + $"rawValue={(int)ability} | " + $"exception={ex}");
                    }
                    continue;
                }

                if (abilityInfo == null)
                {
                    if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
                    {
                        InscryptionAPI.InscryptionAPIPlugin.Logger.LogError($"[TradeCardsForPelts Debug] VANILLA NRE CANDIDATE: " + $"AbilitiesUtil.GetInfo returned null | " + $"card={GetCardName(card)} | " + $"tier={effectiveTier} | " + $"abilityIndex={abilityIndex} | " + $"ability={abilityName} | " + $"rawValue={(int)ability} | " + $"baseAbilities=[{FormatAbilities(card.abilities)}] | " + $"effectiveAbilities=[{FormatAbilities(effectiveAbilities)}] | " + $"mods={card.mods?.Count ?? 0} | " + $"vanillaExpression=!AbilitiesUtil.GetInfo(a).opponentUsable");
                    }
                    continue;
                }

                if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
                {
                    InscryptionAPI.InscryptionAPIPlugin.Logger.LogDebug($"[TradeCardsForPelts Debug] Ability lookup OK | " + $"card={GetCardName(card)} | " + $"tier={effectiveTier} | " +
                        $"abilityIndex={abilityIndex} | " + $"ability={abilityName} | " + $"rawValue={(int)ability} | " + $"opponentUsable={abilityInfo.opponentUsable}");
                }
            }
        }
        if (InscryptionAPI.InscryptionAPIPlugin.configEnableTraderCostTierBugDebugLogging.Value)
        {
            InscryptionAPI.InscryptionAPIPlugin.Logger.LogDebug("===== End GenerateTradeCardsWithCostTier pre-run =====");
        }
    }

    /// <summary>
    /// Formats the Passed Abilities as a String.
    /// </summary>
    /// <param name="abilities">The Abilities to cast into the String.</param>
    /// <returns>A Formatted Abilities String.</returns>
    private static string FormatAbilities(IEnumerable<Ability> abilities)
    {
        if (abilities == null)
            return "null";

        return string.Join(", ", abilities.Select(GetGuidAndKey));
    }

    /// <summary>
    /// Gets a GUID and Key associated with a Ability from the passed in Ability Value.
    /// </summary>
    /// <param name="ability">The Ability we want to get the String Name of.</param>
    /// <returns>A String representing the Ability either via the GUID system or the Enum System.</returns>
    private static string GetGuidAndKey(Ability ability)
    {
        if (GuidManager.TryGetGuidAndKeyEnumValue(ability, out string guid, out string key))
            return $"{guid}_{key}";

        return ability.ToString();
    }

    /// <summary>
    /// Gets the Card Name and DisplayName of the passed in Card.
    /// </summary>
    /// <param name="card">The Card to get the Name of.</param>
    /// <returns>A string of the Card's Name and DisplayName.</returns>
    private static string GetCardName(CardInfo card)
    {
        if (card == null)
            return "null";

        string internalName = string.IsNullOrEmpty(card.name) ? "null" : card.name;
        string displayedName = string.IsNullOrEmpty(card.DisplayedNameEnglish)
            ? "null"
            : card.DisplayedNameEnglish;

        return $"{internalName} ({displayedName})";
    }
}