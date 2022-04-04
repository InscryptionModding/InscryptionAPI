using HarmonyLib;
using DiskCardGame;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class EmpowerStackableBuffs
{
    // This patch makes it so that IF passive attack buffs are made stackable, they will function correctly
    // It does NOT make them actually stackable in the metadata
    private static int ExcessAbilityCount(this PlayableCard card, Ability ability)
    {
        // Always return 0 if the ability does not stack
        if (!AbilitiesUtil.GetInfo(ability).canStack)
            return 0;

        // Here, we're just counting the excess buffs - the number beyond 1.
        // We know that the original code already counted once
        // We just need to know how many beyond 1 there was

        int count = card.Info.Abilities
                    .Concat(AbilitiesUtil.GetAbilitiesFromMods(card.TemporaryMods))
                    .Where(ab => ab == ability)
                    .Count();

        if (count >= 2)
            return count - 1;
        else
            return 0;
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
    [HarmonyPostfix]
    private static void MakeAttackBuffsStack(ref int __result, ref PlayableCard __instance)
    {
        // Right now, attack buffs don't stack because this method just checks if the
        // buff exists, and doens't check how many of them you have.
        // Let's change that.
        if (__instance.OnBoard)
        {
            foreach (CardSlot cardSlot in Singleton<BoardManager>.Instance.GetAdjacentSlots(__instance.Slot))
                if (cardSlot.Card != null)
                    __result += cardSlot.Card.ExcessAbilityCount(Ability.BuffNeighbours);

            // Deal with buff and debuff enemy
            // We have to handle giant cards separately (i.e., the moon)
            if (__instance.Info.HasTrait(Trait.Giant))
            {
                if (!__instance.HasAbility(Ability.MadeOfStone))
                {
                    foreach (CardSlot slot in BoardManager.Instance.GetSlots(true))
                    {
                        if (slot.Card != null)
                        {
                            __result += slot.Card.ExcessAbilityCount(Ability.BuffEnemy) - slot.Card.ExcessAbilityCount(Ability.DebuffEnemy);
                        }
                    }
                }
            }
            else if (__instance.Slot.opposingSlot.Card != null)
            {
                __result += __instance.Slot.opposingSlot.Card.ExcessAbilityCount(Ability.BuffEnemy);
                
                if (!__instance.HasAbility(Ability.MadeOfStone))
                    __result -= __instance.Slot.opposingSlot.Card.ExcessAbilityCount(Ability.DebuffEnemy);
            }

            if (ConduitCircuitManager.Instance != null) // No need to check save file location
            {
                List<PlayableCard> conduitsForSlot = ConduitCircuitManager.Instance.GetConduitsForSlot(__instance.Slot);
                foreach (PlayableCard conduit in conduitsForSlot)
                    __result += conduit.ExcessAbilityCount(Ability.ConduitBuffAttack);
            }

            if (__instance.Info.HasTrait(Trait.Gem))
                foreach (CardSlot slot in BoardManager.Instance.GetSlots(!__instance.OpponentCard))
                    if (slot.Card != null)
                        __result += slot.Card.ExcessAbilityCount(Ability.BuffGems);
        }
    }
}