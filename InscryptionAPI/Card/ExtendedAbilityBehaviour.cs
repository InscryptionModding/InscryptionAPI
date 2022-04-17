using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Triggers;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public abstract class ExtendedAbilityBehaviour : AbilityBehaviour, IAttackModification, IActivateWhenFacedown, IOnCardPassiveAttackBuffs, IOnCardPassiveHealthBuffs
{
    // This section handles attack slot management

    public virtual bool TriggerWhenFacedown => false;
    public virtual bool ShouldTriggerWhenFaceDown(Trigger trigger, object[] otherArgs) => TriggerWhenFacedown;
    public virtual bool ShouldTriggerCustomWhenFaceDown(Type customTrigger) => TriggerWhenFacedown;
    public virtual bool RespondsToGetOpposingSlots() => false;

    public virtual List<CardSlot> GetOpposingSlots(List<CardSlot> originalSlots, List<CardSlot> otherAddedSlots) => new();

    public virtual bool RemoveDefaultAttackSlot() => false;

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
        return 0;
    }

    public virtual bool RespondsToCardPassiveAttackBuffs(PlayableCard card, int currentValue)
    {
        return ProvidesPassiveAttackBuff;
    }

    public virtual int CollectCardPassiveAttackBuffs(PlayableCard card, int currentValue)
    {
        if (card.OpponentCard == Card.OpponentCard)
        {
            int[] result = GetPassiveAttackBuffs();
            if (result != null && card.Slot.Index < result.Length)
            {
                return currentValue + result[card.Slot.Index];
            }
        }
        return currentValue;
    }

    public virtual bool RespondsToCardPassiveHealthBuffs(PlayableCard card, int currentValue)
    {
        return ProvidesPassiveHealthBuff;
    }

    public virtual int CollectCardPassiveHealthBuffs(PlayableCard card, int currentValue)
    {
        if (card.OpponentCard == Card.OpponentCard)
        {
            int[] result = GetPassiveHealthBuffs();
            if (result != null && card.Slot.Index < result.Length)
            {
                return currentValue + result[card.Slot.Index];
            }
        }
        return currentValue;
    }

    public virtual bool RespondsToModifyAttackSlots(PlayableCard card, List<CardSlot> currentSlots, bool didRemoveDefaultSlot) => RespondsToGetOpposingSlots();

    public List<CardSlot> CollectModifyAttackSlots(PlayableCard card, List<CardSlot> originalSlots, List<CardSlot> currentSlots, ref bool didRemoveDefaultSlot)
    {
        didRemoveDefaultSlot = RemoveDefaultAttackSlot();
        return GetOpposingSlots(originalSlots, currentSlots);
    }

    public bool BringsOriginalSlotBack(PlayableCard card) => false;

    public bool RespondsToGetAttackSlotCount(PlayableCard card) => false;

    public int CollectGetAttackSlotCount(PlayableCard card) => 0;
}