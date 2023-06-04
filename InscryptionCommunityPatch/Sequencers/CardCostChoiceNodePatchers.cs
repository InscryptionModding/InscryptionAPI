using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Sequencers;

[HarmonyPatch]
internal static class ChoiceNodePatch
{
    [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.GetCardbackTexture))]
    [HarmonyPostfix]
    private static void CardSingleChoicesSequencer_GetCardbackTexture(ref Texture __result, CardChoice choice)
    {
        switch (choice.resourceType)
        {
            case ResourceType.Energy:
                __result = TextureHelper.GetImageAsTexture("energyCost.png", typeof(ChoiceNodePatch).Assembly);
                break;
            case ResourceType.Gems:
                __result = TextureHelper.GetImageAsTexture(MoxTextureName(choice.resourceAmount), typeof(ChoiceNodePatch).Assembly);
                break;
        }
    }

    [HarmonyPatch(typeof(Part1CardChoiceGenerator), "GenerateCostChoices")]
    [HarmonyPostfix]
    private static void Part1CardChoiceGenerator_GenerateCostChoices(ref List<CardChoice> __result, int randomSeed)
    {
        var list = __result;
        if (GetRandomChoosableEnergyCard(randomSeed++) != null)
            __result.Add(new CardChoice() { resourceType = ResourceType.Energy });

        int moxIndex = GetRandomMoxIndex(randomSeed++);
        if (moxIndex > 0)
            __result.Add(new CardChoice() { resourceType = ResourceType.Gems, resourceAmount = moxIndex });

        while (list.Count > 3)
            list.RemoveAt(SeededRandom.Range(0, list.Count, randomSeed++));

        __result = list;

    }

    [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.CostChoiceChosen))]
    [HarmonyPostfix]
    private static IEnumerator PostfixGameLogicPatch(IEnumerator enumerator, CardSingleChoicesSequencer __instance, SelectableCard card)
    {
        if (card.ChoiceInfo.resourceType == ResourceType.Energy || card.ChoiceInfo.resourceType == ResourceType.Gems)
        {
            CardInfo cardInfo = new();
            if (card.ChoiceInfo.resourceType == ResourceType.Energy)
                cardInfo = GetRandomChoosableEnergyCard(SaveManager.SaveFile.GetCurrentRandomSeed());

            if (card.ChoiceInfo.resourceType == ResourceType.Gems)
            {
                GemType gemType = card.ChoiceInfo.resourceAmount switch { 3 => GemType.Blue, 2 => GemType.Orange, _ => GemType.Green };
                cardInfo = GetRandomChoosableMoxCard(SaveManager.SaveFile.GetCurrentRandomSeed(), gemType);
            }

            card.SetInfo(cardInfo);
            card.SetFaceDown(false, false);
            card.SetInteractionEnabled(false);
            yield return __instance.TutorialTextSequence(card);
            card.SetCardbackToDefault();
            yield return __instance.WaitForCardToBeTaken(card);
            yield break;
        }
        else
        {
            yield return enumerator;
        }
    }

    public static CardInfo GetRandomChoosableEnergyCard(int randomSeed)
    {
        List<CardInfo> list = CardLoader.GetUnlockedCards(CardMetaCategory.ChoiceNode, CardTemple.Nature).FindAll((CardInfo x) => x.energyCost > 0);
        if (list.Count == 0)
            return null;
        else
            return CardLoader.Clone(list[SeededRandom.Range(0, list.Count, randomSeed)]);
    }

    public static CardInfo GetRandomChoosableMoxCard(int randomSeed, GemType gem)
    {
        List<CardInfo> list = CardLoader.GetUnlockedCards(CardMetaCategory.ChoiceNode, CardTemple.Nature).FindAll((CardInfo x) => x.gemsCost.Count > 0 && x.gemsCost.Contains(gem));
        if (list.Count == 0)
            return null;
        else
            return CardLoader.Clone(list[SeededRandom.Range(0, list.Count, randomSeed)]);
    }
    public static int GetRandomMoxIndex(int randomSeed)
    {
        List<CardInfo> list = CardLoader.GetUnlockedCards(CardMetaCategory.ChoiceNode, CardTemple.Nature).FindAll((CardInfo x) => x.gemsCost.Count > 0);

        if (list.Count == 0)
            return 0;

        List<int> moxIndeces = new();
        if (list.Exists(x => x.gemsCost.Contains(GemType.Green)))
            moxIndeces.Add(1);
        if (list.Exists(x => x.gemsCost.Contains(GemType.Orange)))
            moxIndeces.Add(2);
        if (list.Exists(x => x.gemsCost.Contains(GemType.Blue)))
            moxIndeces.Add(3);

        return moxIndeces[SeededRandom.Range(0, moxIndeces.Count, randomSeed)];
    }
    public static string MoxTextureName(int index) => "moxCost" + (index switch { 1 => "Green", 2 => "Orange", 3 => "Blue", _ => "" }) + ".png";
}