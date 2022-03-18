using System.Collections;
using DiskCardGame;
using UnityEngine;
using HarmonyLib;
using InscryptionAPI.Helpers;

namespace InscryptionCommunityPatch.Sequencers;

[HarmonyPatch]
internal static class ChoiceNodePatch
{
    [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.GetCardbackTexture))]
    [HarmonyPostfix]
    public static void CardSingleChoicesSequencer_GetCardbackTexture(ref Texture __result, CardChoice choice)
    {
        switch (choice.resourceType)
        {
            case ResourceType.Energy:
                __result = TextureHelper.GetImageAsTexture("energyCost.png", typeof(ChoiceNodePatch).Assembly);
                break;
            case ResourceType.Gems:
                __result = TextureHelper.GetImageAsTexture("gemCost.png", typeof(ChoiceNodePatch).Assembly);
                break;
        }
    }

    [HarmonyPatch(typeof(Part1CardChoiceGenerator), "GenerateCostChoices")]
    [HarmonyPostfix]
    public static void Part1CardChoiceGenerator_GenerateCostChoices(ref List<CardChoice> __result, int randomSeed)
    {
        var list = __result;
        if (GetRandomChoosableEnergyCard(randomSeed++) != null)
            __result.Add(new CardChoice() { resourceType = ResourceType.Energy });

        if (GetRandomChoosableMoxCard(randomSeed++) != null)
            __result.Add(new CardChoice() { resourceType = ResourceType.Gems });
        
        while (list.Count > 3)
            list.RemoveAt(SeededRandom.Range(0, list.Count, randomSeed++));

        __result = list;

    }

    [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.CostChoiceChosen))]
    [HarmonyPostfix]
    public static IEnumerator PostfixGameLogicPatch(IEnumerator enumerator, CardSingleChoicesSequencer __instance, SelectableCard card)
    {
        if (card.ChoiceInfo.resourceType == ResourceType.Energy || card.ChoiceInfo.resourceType == ResourceType.Gems)
        {
            CardInfo cardInfo = new CardInfo();
            if (card.ChoiceInfo.resourceType == ResourceType.Energy)
                cardInfo = GetRandomChoosableEnergyCard(SaveManager.SaveFile.GetCurrentRandomSeed());
            
            if (card.ChoiceInfo.resourceType == ResourceType.Gems)
                cardInfo = GetRandomChoosableMoxCard(SaveManager.SaveFile.GetCurrentRandomSeed());
            
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

    public static CardInfo GetRandomChoosableMoxCard(int randomSeed)
    {
        List<CardInfo> list = CardLoader.GetUnlockedCards(CardMetaCategory.ChoiceNode, CardTemple.Nature).FindAll((CardInfo x) => x.gemsCost.Count > 0);
        if (list.Count == 0)
            return null;
        else
            return CardLoader.Clone(list[SeededRandom.Range(0, list.Count, randomSeed)]);
    }
}