using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using Sirenix.Serialization.Utilities;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class RandomAbilityPatches
{
    private static int GetRandomSeed()
    {
        return SaveManager.SaveFile.GetCurrentRandomSeed() + Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle;
    }
    private static Ability ChooseAbility(PlayableCard card)
    {
        List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(opponentUsable: true, 0, 5, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
        learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || card.HasAbility(x));
        if (learnedAbilities.Count > 0)
            return learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, GetRandomSeed())];

        return Ability.Sharp;
    }

    [HarmonyPatch(typeof(Opponent), nameof(Opponent.QueueCard))]
    [HarmonyPostfix]
    private static IEnumerator ActivateRandomOnQueue(IEnumerator enumerator, Opponent __instance, CardInfo cardInfo)
    {
        yield return enumerator;

        PlayableCard card = __instance.Queue.Find(x => x.Info == cardInfo);
        if (!card)
            yield break;

        if (card.HasAbility(Ability.RandomAbility) && !card.Status.hiddenAbilities.Contains(Ability.RandomAbility))
        {
            yield return new WaitForSeconds(0.15f);
            card.Anim.PlayTransformAnimation();
	        yield return new WaitForSeconds(0.15f);
            card.Status.hiddenAbilities.Add(Ability.RandomAbility);
            CardModificationInfo cardModificationInfo = new(ChooseAbility(card));
            CardModificationInfo cardModificationInfo2 = card.TemporaryMods.Find((CardModificationInfo x) => x.HasAbility(Ability.RandomAbility));
            
            cardModificationInfo2 ??= card.Info.Mods.Find((CardModificationInfo x) => x.HasAbility(Ability.RandomAbility));

            if (cardModificationInfo2 != null)
            {
                cardModificationInfo.fromTotem = cardModificationInfo2.fromTotem;
                cardModificationInfo.fromCardMerge = cardModificationInfo2.fromCardMerge;
            }
            card.AddTemporaryMod(cardModificationInfo);
        }
    }
    [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
    [HarmonyPostfix]
    private static IEnumerator ActivateRandomOnResolve(IEnumerator enumerator, PlayableCard card)
    {
        yield return enumerator;

        if (!card || card.Dead)
            yield break;

        if (card.HasAbility(Ability.RandomAbility) && !card.Status.hiddenAbilities.Contains(Ability.RandomAbility))
        {
            yield return new WaitForSeconds(0.15f);
            card.Anim.PlayTransformAnimation();
            yield return new WaitForSeconds(0.15f);
            card.Status.hiddenAbilities.Add(Ability.RandomAbility);
            CardModificationInfo cardModificationInfo = new(ChooseAbility(card));
            CardModificationInfo cardModificationInfo2 = card.TemporaryMods.Find((CardModificationInfo x) => x.HasAbility(Ability.RandomAbility));

            cardModificationInfo2 ??= card.Info.Mods.Find((CardModificationInfo x) => x.HasAbility(Ability.RandomAbility));

            if (cardModificationInfo2 != null)
            {
                cardModificationInfo.fromTotem = cardModificationInfo2.fromTotem;
                cardModificationInfo.fromCardMerge = cardModificationInfo2.fromCardMerge;
            }
            card.AddTemporaryMod(cardModificationInfo);
            yield return new WaitForSeconds(0.5f);
        }
    }
    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TransformIntoCard))]
    private static IEnumerator ActivateRandomOnEvolve(IEnumerator enumerator, PlayableCard __instance, CardInfo evolvedInfo)
    {
        yield return enumerator;

        if (evolvedInfo.HasAbility(Ability.RandomAbility) && !__instance.Status.hiddenAbilities.Contains(Ability.RandomAbility))
        {
            yield return new WaitForSeconds(0.5f);
            __instance.Anim.PlayTransformAnimation();
            yield return new WaitForSeconds(0.15f);
            __instance.Status.hiddenAbilities.Add(Ability.RandomAbility);
            CardModificationInfo cardModificationInfo = new(ChooseAbility(__instance));
            CardModificationInfo cardModificationInfo2 = __instance.TemporaryMods.Find((CardModificationInfo x) => x.HasAbility(Ability.RandomAbility));

            cardModificationInfo2 ??= __instance.Info.Mods.Find((CardModificationInfo x) => x.HasAbility(Ability.RandomAbility));

            if (cardModificationInfo2 != null)
            {
                cardModificationInfo.fromTotem = cardModificationInfo2.fromTotem;
                cardModificationInfo.fromCardMerge = cardModificationInfo2.fromCardMerge;
            }
            __instance.AddTemporaryMod(cardModificationInfo);
        }
    }
}
