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

    private static IEnumerator AddRandomSigil(PlayableCard card)
    {
        Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle++;
        yield return new WaitForSeconds(0.15f);
        card.Anim.PlayTransformAnimation();
        yield return new WaitForSeconds(0.15f);
        card.GetComponent<RandomAbility>().AddMod();
    }
    private static Ability GetRandomAbility(PlayableCard card)
    {
        List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(opponentUsable: true, 0, 5, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
        learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || card.HasAbility(x));
        if (learnedAbilities.Count > 0)
            return learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, SaveManager.SaveFile.GetCurrentRandomSeed() + Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle)];

        return Ability.Sharp;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(RandomAbility), nameof(RandomAbility.ChooseAbility))]
    private static void OpponentChooseAbility(RandomAbility __instance, ref Ability __result)
    {
        if (__instance.Card.OpponentCard)
            __result = GetRandomAbility(__instance.Card);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Opponent), nameof(Opponent.QueueCard))]
    private static IEnumerator ActivateRandomOnQueue(IEnumerator enumerator, Opponent __instance, CardSlot slot)
    {
        yield return enumerator;

        PlayableCard card = __instance.Queue.Find(x => x.QueuedSlot == slot);
        if (!card)
            yield break;

        if (card.HasAbility(Ability.RandomAbility) && !card.Status.hiddenAbilities.Contains(Ability.RandomAbility))
            yield return AddRandomSigil(card);
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
            yield return AddRandomSigil(card);
            yield return new WaitForSeconds(0.5f);
        }
    }
    [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TransformIntoCard))]
    private static IEnumerator ActivateRandomOnEvolve(IEnumerator enumerator, PlayableCard __instance, CardInfo evolvedInfo)
    {
        yield return enumerator;
        if (!__instance)
            yield break;

        if (evolvedInfo.HasAbility(Ability.RandomAbility) && !__instance.Status.hiddenAbilities.Contains(Ability.RandomAbility))
            yield return AddRandomSigil(__instance);
    }
    [HarmonyPrefix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.AddTemporaryMod))]
    private static void ActivateOnAddTempMod(PlayableCard __instance, ref CardModificationInfo mod)
    {
        if (mod.HasAbility(Ability.RandomAbility))
        {
            for (int i = 0; i < mod.abilities.Count;i++)
            {
                if (mod.abilities[i] == Ability.RandomAbility)
                {
                    Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle++;
                    mod.abilities[i] = GetRandomAbility(__instance);
                }
            }
        }
    }
}
