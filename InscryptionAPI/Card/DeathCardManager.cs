using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Card;

/// <summary>
/// Class for handling custom death cards.
/// </summary>
[HarmonyPatch]
public static class DeathCardManager
{
    /// <summary>
    /// The prefix used by the API to demark custom death cards added through the DeathCardManager.
    /// </summary>
    public const string CardPrefix = "apiDeathCard";
    /// <summary>
    /// A list of all API-added death cards
    /// </summary>
    public static List<CardInfo> APIDeathCards => CardManager.AllCardsCopy.FindAll(x => x.ModPrefixIs(CardPrefix));
    private static readonly List<CardModificationInfo> NewDefaultDeathCards = new();
    private static readonly List<CardModificationInfo> NewAscensionDeathMods = new();

    /// <summary>
    /// Adds a death card to the list of default death cards available at the start of each run.
    /// </summary>
    /// <param name="deathCardMod">The CardModificationInfo containing the death card data.</param>
    /// <param name="addToAscension">If true, will add deathCardMod to the list of ascension death cards rather than the default (Part 1) pool.</param>
    public static void AddDefaultDeathCard(CardModificationInfo deathCardMod, bool addToAscension = false)
    {
        if (addToAscension)
            NewAscensionDeathMods.Add(deathCardMod);
        else
            NewDefaultDeathCards.Add(deathCardMod);
    }
    private static bool MatchingDeathCardInfo(DeathCardInfo info, DeathCardInfo compareAgainst)
    {
        return info != null && compareAgainst != null &&
            info.eyesIndex == compareAgainst.eyesIndex &&
            info.headType == compareAgainst.headType &&
            info.lostEye == compareAgainst.lostEye &&
            info.mouthIndex == compareAgainst.mouthIndex;
    }
    /// <summary>
    /// Gets the name of an API-added death card from the given CardModificationInfo. Can also create a new death card using the given CardModificationInfo.
    /// </summary>
    /// <param name="mod">The CardModificationInfo to access.</param>
    /// <param name="createNewCard">Whether or not to create a new death card with the given name. Appends a number for duplicate card names.</param>
    /// <returns>The internal name of the death card associated with the given CardModificationInfo.</returns>
    public static string GetAPIDeathCardName(CardModificationInfo mod, bool createNewCard = false)
    {
        string cleanId = mod.CleanId(); // use the clean singleton id to reduce chance of duplicate name
        string name = $"{CardPrefix}_{(string.IsNullOrWhiteSpace(cleanId) ? "" : cleanId + "_")}{mod.nameReplacement}";

        // if there is by some disaster duplicate names, add a number to the end to differentiate them
        if (createNewCard)
        {
            int cardsWithName = APIDeathCards.Count(x => x.name == name);
            if (cardsWithName > 0)
                name += "#" + (1 + cardsWithName);
        }
        // if not creating a new card, check if there are cards with duplicate display names
        else if (APIDeathCards.Count(x => x.displayedName == mod.nameReplacement) > 1)
        {
            List<CardInfo> possibleCards = APIDeathCards.FindAll(
                x => x.displayedName == mod.nameReplacement &&
                x.Mods.Exists(y => MatchingDeathCardInfo(y.deathCardInfo, mod.deathCardInfo)));

            if (possibleCards.Count == 0)
                InscryptionAPIPlugin.Logger.LogError($"Could not find card [{mod.nameReplacement}] with matching deathCardInfo!");
            else
                name = possibleCards[0].name;
        }
        return name;
    }
    internal static CardInfo CreateCustomDeathCard(CardModificationInfo mod)
    {
        // since death cards are just card mods applied to the same base template card,
        // extended properties and custom cards will apply to EVERY death card
        // ergo, we must create new, actual cards to house our custom costs and such and things
        // using New() seems to break things, so we do it like this
        CardInfo newInfo = ScriptableObject.CreateInstance<CardInfo>()
            .SetName(GetAPIDeathCardName(mod, true))
            .SetBasic(mod.nameReplacement, mod.attackAdjustment, mod.healthAdjustment)
            .SetCost(mod.bloodCostAdjustment, mod.bonesCostAdjustment, mod.energyCostAdjustment, mod.addGemCost)
            .AddAbilities(mod.abilities.ToArray())
            .AddAppearances(CardAppearanceBehaviour.Appearance.DynamicPortrait)
            .AddTraits(Trait.DeathcardCreationNonOption)
            .SetOnePerDeck();

        newInfo.animatedPortrait = CardLoader.GetCardByName("!DEATHCARD_BASE").animatedPortrait;
        newInfo.Mods.Add(new() { singletonId = mod.CleanId(), deathCardInfo = mod.deathCardInfo });

        // set the custom costs to the card info
        foreach (string customCost in CardModificationInfoManager.GetCustomCostsFromId(mod.singletonId))
        {
            string[] splitCost = customCost.Split(',');
            newInfo.SetExtendedProperty(splitCost[0], splitCost[1]);
        }
        CardManager.Add(CardPrefix, newInfo);

        return newInfo;
    }
    internal static void AddCustomDeathCards()
    {
        List<CardModificationInfo> infos = NewDefaultDeathCards.Concat(SaveManager.SaveFile.deathCardMods).ToList().FindAll(x => x.HasCustomCostsId());
        foreach (CardModificationInfo mod in infos)
            CreateCustomDeathCard(mod);
    }

    #region Patches
    [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.GetChoosableDeathcardMods))]
    [HarmonyPostfix]
    private static void AddDuplicateNameCards(SaveFile __instance, List<CardModificationInfo> __result)
    {
        if (!SaveFile.IsAscension)
        {
            List<CardModificationInfo> list = new(__instance.deathCardMods);
            // list.RemoveAll(x => !x.HasCustomCosts());
            list.RemoveAll(
                x => __result.Contains(x) ||
                RunState.Run.playerDeck.Cards.Exists(
                    y => y.displayedName == x.nameReplacement &&
                    y.Mods.Exists(z => MatchingDeathCardInfo(z.deathCardInfo, x.deathCardInfo))));

            __result.AddRange(list);
        }
    }
    [HarmonyPatch(typeof(CardLoader), nameof(CardLoader.CreateDeathCard))]
    [HarmonyPostfix]
    private static void ReplaceWithAPIDeathCard(ref CardInfo __result, CardModificationInfo deathCardMod)
    {
        // if the death card has custom costs, replace it with the api card corresponding to it
        if (deathCardMod.HasCustomCostsId())
        {
            CardInfo card = CardLoader.GetCardByName(GetAPIDeathCardName(deathCardMod));
            if (card != null)
                __result = card;
        }
    }
    [HarmonyPatch(typeof(DefaultDeathCards), nameof(DefaultDeathCards.CreateDefaultCardMods))]
    [HarmonyPostfix]
    private static void AddNewDefaultDeathCards(ref List<CardModificationInfo> __result)
    {
        foreach (CardModificationInfo deathCard in NewDefaultDeathCards)
            __result.Add(deathCard);
    }
    [HarmonyPatch(typeof(DefaultDeathCards), nameof(DefaultDeathCards.CreateAscensionCardMods))]
    [HarmonyPostfix]
    private static void AddNewAscensionDeathMods(ref List<CardModificationInfo> __result)
    {
        foreach (CardModificationInfo deathCard in NewAscensionDeathMods)
            __result.Add(deathCard);

        __result.RemoveAll(x => x.abilities.Exists(a => !ProgressionData.LearnedAbility(a)));
    }
    #endregion
}
