using DiskCardGame;

namespace InscryptionAPI.Slots;

/// <summary>
/// Base class for all slot modification behaviors
/// </summary>
public class SlotModificationInteractable : AlternateInputInteractable
{
    private CardSlot slot;
    /// <summary>
    /// The mouse cursor icon that is used when hovering over this slot.
    /// </summary>
    public override CursorType CursorType => CursorType.Inspect;
    /// <summary>
    /// The slot modification ID whose rulebook entry this interactable will point to.
    /// </summary>
    public SlotModificationManager.ModificationType ModType { get; set; }

    /// <summary>
    /// Initialises this interactable's data.
    /// </summary>
    /// <param name="modType">The slot modification ID whose rulebook entry this interactable will point to.</param>
    /// <param name="cardSlot">The <see cref="CardSlot"/> this interactable is attached to.</param>
    public void AssignSlotModification(SlotModificationManager.ModificationType modType, CardSlot cardSlot)
    {
        ModType = modType;
        slot = cardSlot;
        base.SetEnabled(true);
    }

    /// <summary>
    /// Opens the rulebook page for this interactable's <see cref="ModType"/>.
    /// </summary>
    public override void OnAlternateSelectStarted()
    {
        //InscryptionAPIPlugin.Logger.LogDebug($"Open to slot mod page: [{ModType}] on slot [{slot}]");
        RuleBookController.Instance.OpenToItemPage(SlotModificationManager.SLOT_PAGEID + (int)ModType, false);
    }
}