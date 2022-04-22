using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Card;
using System.Collections;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class PassiveAttackBuffPatches
{
    private static int AbilityCount (this PlayableCard card, Ability ability)
    {
        int count = 0;
        count += card.Info.Abilities.Count(a => a == ability);
        count += AbilitiesUtil.GetAbilitiesFromMods(card.TemporaryMods).Count(a => a == ability);
        count -= card.TemporaryMods.SelectMany(m => m.negateAbilities).Count(a => a == ability);
        if (AbilitiesUtil.GetInfo(ability).canStack)
            return count;
        else
            return count > 0 ? 1 : 0; // If it's not stackable, you get at most one
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
    [HarmonyPostfix]
    private static IEnumerator AlwaysInstantiateConduitManager(IEnumerator sequence)
    {
        if (ConduitCircuitManager.Instance == null)
            BoardManager.Instance.gameObject.AddComponent<ConduitCircuitManager>();

        yield return sequence;
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
    [HarmonyPrefix]
    private static bool BetterAttackBuffs(ref int __result, ref PlayableCard __instance)
    {
        // This combines all of our original patches into a single method that just replaces
        // the original result
        if (__instance.OnBoard)
        {
            // Check BuffNeighbors:
            foreach (CardSlot neighbor in BoardManager.Instance.GetAdjacentSlots(__instance.slot).Where(s => s.Card != null))
                if (neighbor.Card != null)
                    __result += neighbor.Card.AbilityCount(Ability.BuffNeighbours);

            // Deal with buff and debuff enemy
            if (!__instance.HasAbility(Ability.MadeOfStone))
            {
                // We have to handle giant cards separately (i.e., the moon)
                if (__instance.Info.HasTrait(Trait.Giant))
                {    
                    foreach (CardSlot slot in BoardManager.Instance.GetSlots(__instance.OpponentCard))
                    {
                        if (slot.Card != null)
                        {
                            __result += slot.Card.AbilityCount(Ability.BuffEnemy);
                            __result -= slot.Card.AbilityCount(Ability.DebuffEnemy);
                        }
                    }
                }
                else if (__instance.Slot.opposingSlot.Card != null)
                {
                    __result += __instance.Slot.opposingSlot.Card.AbilityCount(Ability.BuffEnemy);
                    __result -= __instance.Slot.opposingSlot.Card.AbilityCount(Ability.DebuffEnemy);
                }
            }

            if (ConduitCircuitManager.Instance != null) // No need to check save file location - this lets conduits work in all acts
            {
                List<PlayableCard> conduitsForSlot = ConduitCircuitManager.Instance.GetConduitsForSlot(__instance.Slot);
                foreach (PlayableCard card in conduitsForSlot)
                    __result += card.AbilityCount(Ability.ConduitBuffAttack);

                if (conduitsForSlot.Count > 0)
                    __result += 2 * __instance.AbilityCount(Ability.CellBuffSelf);
            }

            if (__instance.Info.HasTrait(Trait.Gem))
                foreach (CardSlot slot in BoardManager.Instance.GetSlots(!__instance.OpponentCard))
                    if (slot.Card != null)
                        __result += slot.Card.AbilityCount(Ability.BuffGems);
        }

        if (__instance.Info.Gemified && ResourcesManager.Instance.HasGem(GemType.Orange))
            __result += 1;

        return false;
    }
}