using DiskCardGame;
using System.Collections;
using UnityEngine;

namespace InscryptionAPI.Slots;

/// <summary>
/// Base class for all slot modification behaviors
/// </summary>
public abstract class SlotModificationGainAbilityBehaviour : SlotModificationBehaviour
{
    /// <summary>
    /// Ability cards placed in this slot will gain.
    /// </summary>
    protected abstract Ability AbilityToGain { get; }

    private string TemporaryModId => $"SlotModification{AbilityToGain}{Slot.IsPlayerSlot}{Slot.Index}";

    private CardModificationInfo GetSlotAbilityMod(PlayableCard card, bool create = false)
    {
        card.temporaryMods ??= new();
        CardModificationInfo mod = card.TemporaryMods.FirstOrDefault(m => m != null && !string.IsNullOrEmpty(m.singletonId) && m.singletonId.Equals(TemporaryModId));
        if (mod == null && create)
            mod = new(AbilityToGain) { singletonId = TemporaryModId };

        return mod;
    }

    /// <summary>
    /// If this trigger should respond to other cards being assigned to card slots.
    /// </summary>
    /// <returns>Always returns true.</returns>
    public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard) => true;

    /// <summary>
    /// Logic for when a card is played on this slot.
    /// </summary>
    /// <param name="otherCard">The card played on this slot.</param>
    public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
    {
        if (otherCard == null || otherCard.Slot == null)
            yield break;

        bool shouldHaveThisTempMod = Slot.Card == otherCard;

        var mod = GetSlotAbilityMod(otherCard);

        if (shouldHaveThisTempMod && mod == null)
        {
            mod = GetSlotAbilityMod(otherCard, true);
            otherCard.AddTemporaryMod(mod);
            ResourcesManager.Instance.ForceGemsUpdate(); // just in case
        }

        if (!shouldHaveThisTempMod && mod != null)
        {
            otherCard.RemoveTemporaryMod(mod);
            ResourcesManager.Instance.ForceGemsUpdate(); // just in case
        }

        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Removes the given ability from the occupying card when this slot modification is removed from the slot.
    /// </summary>
    public override IEnumerator Cleanup(SlotModificationManager.ModificationType replacement)
    {
        if (Slot.Card != null)
        {
            var mod = GetSlotAbilityMod(Slot.Card);
            if (mod != null)
            {
                Slot.Card.RemoveTemporaryMod(mod);
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield break;
    }

    /// <summary>
    /// Gives the ability to gain to the occupying card that this slot modification was just added to.
    /// </summary>
    public override IEnumerator Setup()
    {
        if (Slot.Card != null)
        {
            var mod = GetSlotAbilityMod(Slot.Card);
            if (mod == null)
            {
                Slot.Card.RemoveTemporaryMod(mod);
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield break;
    }
}