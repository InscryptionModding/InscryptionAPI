using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class RandomAbilityPatches
{
    [HarmonyPostfix, HarmonyPatch(typeof(RandomAbility), nameof(RandomAbility.ChooseAbility))]
    private static void OpponentChooseAbility(RandomAbility __instance, ref Ability __result) => __result = GetRandomAbility(__instance.Card);

    [HarmonyPostfix, HarmonyPatch(typeof(RandomAbility), nameof(RandomAbility.OnDrawn))]
    private static IEnumerator Act2Random(IEnumerator enumerator, RandomAbility __instance)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            yield return enumerator;
            yield break;
        }

        CardModificationInfo cardModificationInfo = new(__instance.ChooseAbility()) { negateAbilities = new() { __instance.Ability } };

        CardInfo info = __instance.Card.Info.Clone() as CardInfo;
        info.Mods = new(__instance.Card.Info.Mods) { cardModificationInfo };
        __instance.Card.SetInfo(info);
        yield return __instance.LearnAbility(0.5f);
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
    private static IEnumerator ActivateOnEvolve(IEnumerator enumerator, PlayableCard __instance, CardInfo evolvedInfo)
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
            for (int i = 0; i < mod.abilities.Count; i++)
            {
                if (mod.abilities[i] == Ability.RandomAbility)
                {
                    Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle++;
                    mod.abilities[i] = GetRandomAbility(__instance);
                }
            }
        }
    }

    private static IEnumerator AddRandomSigil(PlayableCard card)
    {
        var component = card.GetComponent<RandomAbility>();
        if (component == null)
            yield break;

        Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle++;

        if (SaveManager.SaveFile.IsPart2)
        {
            CardModificationInfo cardModificationInfo = new(component.ChooseAbility()) { negateAbilities = new() { component.Ability } };

            CardInfo info = card.Info.Clone() as CardInfo;
            info.Mods = new(card.Info.Mods) { cardModificationInfo };
            card.SetInfo(info);
            yield break;
        }

        yield return new WaitForSeconds(0.15f);
        card.Anim.PlayTransformAnimation();
        yield return new WaitForSeconds(0.15f);
        component.AddMod();
    }
    private static Ability GetRandomAbility(PlayableCard card)
    {
        List<Ability> learnedAbilities = new();
        bool isOpponent = card.OpponentCard;
        if (!SaveManager.SaveFile.IsPart2)
            learnedAbilities = AbilitiesUtil.GetLearnedAbilities(opponentUsable: isOpponent, 0, 5, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
        else
        {
            learnedAbilities = AbilitiesUtil.GetLearnedAbilities(opponentUsable: isOpponent, 0, 5, AbilityManager.Part2Modular);
            learnedAbilities.RemoveAll(x => x == Ability.RandomConsumable);
        }

        learnedAbilities.RemoveAll((Ability x) => x == Ability.RandomAbility || card.HasAbility(x));
        if (learnedAbilities.Count > 0)
            return learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, SaveManager.SaveFile.GetCurrentRandomSeed() + Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle)];

        return Ability.Sharp;
    }
}
