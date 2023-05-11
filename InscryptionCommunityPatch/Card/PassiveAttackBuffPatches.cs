using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class PassiveAttackBuffPatches
{
    private static int AbilityCount(this PlayableCard card, Ability ability)
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

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveHealthBuffs))]
    [HarmonyPrefix]
    private static bool BetterHealthBuffs(ref int __result, ref PlayableCard __instance)
    {
        __result = 0;
        if (__instance.Info.Gemified)
        {
            if (__instance.OpponentCard)
            {
                if (Singleton<OpponentGemsManager>.Instance && Singleton<OpponentGemsManager>.Instance.HasGem(GemType.Green))
                    __result += 2;
            }
            else if (ResourcesManager.Instance.HasGem(GemType.Green))
                __result += 2;
        }
        return false;
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
            __result += __instance.Slot.GetAdjacentCards().Sum(playableCard => playableCard.AbilityCount(Ability.BuffNeighbours));

            // Deal with buff and debuff enemy
            // We have to handle giant cards separately (i.e., the moon)
            if (__instance.HasTrait(Trait.Giant))
            {
                foreach (CardSlot slot in BoardManager.Instance.GetSlots(__instance.OpponentCard).Where(slot => slot.Card))
                {
                    __result += slot.Card.AbilityCount(Ability.BuffEnemy);
                    if (__instance.LacksAbility(Ability.MadeOfStone))
                        __result -= slot.Card.AbilityCount(Ability.DebuffEnemy);
                }
            }
            else if (__instance.HasOpposingCard())
            {
                __result += __instance.OpposingCard().AbilityCount(Ability.BuffEnemy);
                if (__instance.LacksAbility(Ability.MadeOfStone))
                    __result -= __instance.OpposingCard().AbilityCount(Ability.DebuffEnemy);
            }

            if (ConduitCircuitManager.Instance != null) // No need to check save file location - this lets conduits work in all acts
            {
                List<PlayableCard> conduitsForSlot = ConduitCircuitManager.Instance.GetConduitsForSlot(__instance.Slot);
                __result += conduitsForSlot.Sum(playableCard => playableCard.AbilityCount(Ability.ConduitBuffAttack));

                if (conduitsForSlot.Count > 0)
                    __result += 2 * __instance.AbilityCount(Ability.CellBuffSelf);
            }

            if (__instance.Info.HasTrait(Trait.Gem))
                __result += BoardManager.Instance
                    .GetSlots(__instance.IsPlayerCard())
                    .Where(slot => slot.Card)
                    .Sum(slot => slot.Card.AbilityCount(Ability.BuffGems));
        }

        if (__instance.Info.Gemified)
        {
            if (__instance.OpponentCard)
            {
                if (Singleton<OpponentGemsManager>.Instance && Singleton<OpponentGemsManager>.Instance.HasGem(GemType.Orange))
                    __result++;
            }
            else if (Singleton<ResourcesManager>.Instance.HasGem(GemType.Orange))
                __result++;
        }

        return false;
    }
}