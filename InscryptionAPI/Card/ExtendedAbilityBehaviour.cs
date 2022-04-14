using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public abstract class ExtendedAbilityBehaviour : AbilityBehaviour, IGetOpposingSlots, IActivateWhenFacedown, IPassiveAttackBuff, IPassiveHealthBuff
{
    // This section handles attack slot management

    public virtual bool TriggerWhenFacedown => false;
    public virtual bool ShouldTriggerWhenFaceDown(Trigger trigger, object[] otherArgs) => TriggerWhenFacedown;
    public virtual bool ShouldTriggerCustomWhenFaceDown(Type customTrigger) => TriggerWhenFacedown;

    public virtual bool RespondsToGetOpposingSlots() => false;

    public virtual List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots) => new();

    public virtual bool RemoveDefaultAttackSlot() => false;

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetOpposingSlots))]
    [HarmonyPostfix]
    private static void UpdateOpposingSlots(ref PlayableCard __instance, ref List<CardSlot> __result)
    {
        bool isAttackingDefaultSlot = !__instance.HasTriStrike() && !__instance.HasAbility(Ability.SplitStrike);
        CardSlot defaultslot = __instance.Slot.opposingSlot;

        List<CardSlot> alteredOpposings = new List<CardSlot>();
        bool removeDefaultAttackSlot = false;

        foreach (IGetOpposingSlots component in CustomTriggerFinder.FindTriggersOnCard<IGetOpposingSlots>(__instance))
        {
            if (component.RespondsToGetOpposingSlots())
            {
                alteredOpposings.AddRange(component.GetOpposingSlots(__result, new(alteredOpposings)));
                removeDefaultAttackSlot = removeDefaultAttackSlot || component.RemoveDefaultAttackSlot();  
            }
        }
        
        if (alteredOpposings.Count > 0) 
            __result.AddRange(alteredOpposings);

        if (isAttackingDefaultSlot && removeDefaultAttackSlot)
            __result.Remove(defaultslot);
    }

    // This section handles passive attack/health buffs

    private static ConditionalWeakTable<PlayableCard, List<ExtendedAbilityBehaviour>> AttackBuffAbilities = new();
    private static ConditionalWeakTable<PlayableCard, List<ExtendedAbilityBehaviour>> HealthBuffAbilities = new();

    private static List<ExtendedAbilityBehaviour> GetAttackBuffs(PlayableCard card)
    {
        List<ExtendedAbilityBehaviour> retval;
        if (AttackBuffAbilities.TryGetValue(card, out retval))
            return retval;

        retval = card.GetComponents<ExtendedAbilityBehaviour>().Where(x => x.ProvidesPassiveAttackBuff).ToList();
        AttackBuffAbilities.Add(card, retval);
        return retval;
    }

    private static List<ExtendedAbilityBehaviour> GetHealthBuffs(PlayableCard card)
    {
        List<ExtendedAbilityBehaviour> retval;
        if (HealthBuffAbilities.TryGetValue(card, out retval))
            return retval;

        retval = card.GetComponents<ExtendedAbilityBehaviour>().Where(x => x.ProvidesPassiveHealthBuff).ToList();
        HealthBuffAbilities.Add(card, retval);
        return retval;
    }

    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual bool ProvidesPassiveAttackBuff => false;

    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual bool ProvidesPassiveHealthBuff => false;

    [Obsolete("Use IPassiveAttackBuff instead")]
    public virtual int[] GetPassiveAttackBuffs() => null;

    [Obsolete("Use IPassiveHealthBuff instead")]
    public virtual int[] GetPassiveHealthBuffs() => null;

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveAttackBuffs))]
    [HarmonyPostfix]
    private static void AddPassiveAttackBuffs(ref PlayableCard __instance, ref int __result)
    {
        if (__instance.slot == null)
            return;

        foreach (IPassiveAttackBuff buffer in BoardManager.Instance.CardsOnBoard.SelectMany(c => CustomTriggerFinder.FindTriggersOnCard<IPassiveAttackBuff>(c)))
            __result += buffer.GetPassiveAttackBuff(__instance);
    }

    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.GetPassiveHealthBuffs))]
    [HarmonyPostfix]
    private static void AddPassiveHealthBuffs(ref PlayableCard __instance, ref int __result)
    {
        if (__instance.slot == null)
            return;

        foreach (IPassiveHealthBuff buffer in BoardManager.Instance.CardsOnBoard.SelectMany(c => CustomTriggerFinder.FindTriggersOnCard<IPassiveHealthBuff>(c)))
            __result += buffer.GetPassiveHealthBuff(__instance);
    }

    public virtual int GetPassiveAttackBuff(PlayableCard target)
    {
        if (ProvidesPassiveAttackBuff)
        {
            if (target.OpponentCard == this.Card.OpponentCard)
            {
                int[] result = GetPassiveAttackBuffs();
                if (result != null && target.Slot.Index < result.Length)
                {
                    return result[target.Slot.Index];
                }
            }
        }
        return 0;
    }

    public virtual int GetPassiveHealthBuff(PlayableCard target)
    {
        if (ProvidesPassiveHealthBuff)
        {
            if (target.OpponentCard == this.Card.OpponentCard)
            {
                int[] result = GetPassiveHealthBuffs();
                if (result != null && target.Slot.Index < result.Length)
                {
                    return result[target.Slot.Index];
                }
            }
        }
        return 0;
    }
}