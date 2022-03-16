using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public abstract class ExtendedAbilityBehaviour : AbilityBehaviour
{
    // This section handles attack slot management

    public virtual bool RespondsToGetOpposingSlots() => false;

    public virtual List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots) => new();

    public virtual bool RemoveDefaultAttackSlot() => false;

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
    [HarmonyPostfix]
    private static void UpdateOpposingSlots(ref PlayableCard __instance, ref List<CardSlot> __result)
    {
        bool isAttackingDefaultSlot = !__instance.HasTriStrike() && !__instance.HasAbility(Ability.SplitStrike);
        CardSlot defaultSlot = __instance.Slot.opposingSlot;

        List<CardSlot> alteredOpposingSlots = new List<CardSlot>();
        bool removeDefaultAttackSlot = false;

        List<ExtendedAbilityBehaviour> behaviours = __instance.GetComponents<ExtendedAbilityBehaviour>().Where(x => x.RespondsToGetOpposingSlots()).ToList();
        foreach (ExtendedAbilityBehaviour component in behaviours)
        {
            alteredOpposingSlots.AddRange(component.GetOpposingSlots(__result, new(alteredOpposingSlots)));
            removeDefaultAttackSlot = removeDefaultAttackSlot || component.RemoveDefaultAttackSlot();
        }

        if (alteredOpposingSlots.Count > 0)
            __result.AddRange(alteredOpposingSlots);

        if (isAttackingDefaultSlot && removeDefaultAttackSlot)
            __result.Remove(defaultSlot);
    }

    // This section handles passive attack/health buffs

    private static readonly ConditionalWeakTable<PlayableCard, List<ExtendedAbilityBehaviour>> AttackBuffAbilities = new();
    private static readonly ConditionalWeakTable<PlayableCard, List<ExtendedAbilityBehaviour>> HealthBuffAbilities = new();

    private static List<ExtendedAbilityBehaviour> GetAttackBuffs(PlayableCard card)
    {
        if (AttackBuffAbilities.TryGetValue(card, out var returnValue))
            return returnValue;

        returnValue = card.GetComponents<ExtendedAbilityBehaviour>().Where(x => x.ProvidesPassiveAttackBuff).ToList();
        AttackBuffAbilities.Add(card, returnValue);
        return returnValue;
    }

    private static List<ExtendedAbilityBehaviour> GetHealthBuffs(PlayableCard card)
    {
        if (HealthBuffAbilities.TryGetValue(card, out var returnValue))
            return returnValue;

        returnValue = card.GetComponents<ExtendedAbilityBehaviour>().Where(x => x.ProvidesPassiveHealthBuff).ToList();
        HealthBuffAbilities.Add(card, returnValue);
        return returnValue;
    }

    public virtual bool ProvidesPassiveAttackBuff => false;
    public virtual bool ProvidesPassiveHealthBuff => false;

    public virtual int[] GetPassiveAttackBuffs() => null;
    public virtual int[] GetPassiveHealthBuffs() => null;

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
    [HarmonyPostfix]
    private static void AddPassiveAttackBuffs(ref PlayableCard __instance, ref int __result)
    {
        if (__instance.slot == null)
            return;

        List<CardSlot> slots = __instance.OpponentCard
            ? BoardManager.Instance.opponentSlots
            : BoardManager.Instance.playerSlots;

        foreach (CardSlot slot in slots.Where(s => s.Card != null))
        {
            foreach (var b in GetAttackBuffs(slot.Card).Where(ab => ab != null))
            {
                int[] buffs = b.GetPassiveAttackBuffs();
                if (buffs == null)
                    continue;

                for (int i = 0; i < buffs.Length; i++)
                {
                    if (__instance.slot.Index == i)
                        __result += buffs[i];
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveHealthBuffs))]
    [HarmonyPostfix]
    private static void AddPassiveHealthBuffs(ref PlayableCard __instance, ref int __result)
    {
        if (__instance.slot == null)
            return;

        List<CardSlot> slots = __instance.OpponentCard
            ? BoardManager.Instance.opponentSlots
            : BoardManager.Instance.playerSlots;

        foreach (CardSlot slot in slots.Where(s => s.Card != null))
        {
            foreach (ExtendedAbilityBehaviour b in GetHealthBuffs(slot.Card))
            {
                int[] buffs = b.GetPassiveHealthBuffs();
                if (buffs == null)
                    continue;

                for (int i = 0; i < buffs.Length; i++)
                {
                    if (__instance.slot.Index == i)
                        __result += buffs[i];
                }
            }
        }
    }
}
