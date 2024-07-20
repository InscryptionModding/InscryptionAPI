using System.Collections;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Saves;
using UnityEngine;
using UnityEngine.UIElements;

namespace InscryptionAPI.Slots;

[HarmonyPatch]
/// <summary>
/// Contains extension methods to simplify slot modification management
/// </summary>
public static class SlotModificationExtensions
{
    /// <summary>
    /// Assigns a new slot modification to a slot.
    /// </summary>
    /// <param name="slot">The slot to assign the modification to</param>
    /// <param name="modType">The modification type to assign</param>
    public static IEnumerator SetSlotModification(this CardSlot slot, SlotModificationManager.ModificationType modType)
    {
        if (slot == null)
            yield break;

        SlotModificationManager.Info defn = SlotModificationManager.AllModificationInfos.FirstOrDefault(m => m.ModificationType == modType);

        // Set the ability behaviour
        var oldSlotModification = slot.GetComponent<SlotModificationBehaviour>();
        if (oldSlotModification != null)
        {
            yield return oldSlotModification.Cleanup(modType);
            CustomCoroutine.WaitOnConditionThenExecute(() => GlobalTriggerHandler.Instance.StackSize == 0, () => GameObject.Destroy(oldSlotModification));
            SlotModificationManager.Instance.SlotReceivers.Remove(slot);
        }

        if (defn != null && defn.SlotBehaviour != null)
        {
            SlotModificationBehaviour newBehaviour = slot.gameObject.AddComponent(defn.SlotBehaviour) as SlotModificationBehaviour;

            SlotModificationManager.Instance.SlotReceivers[slot] = new(modType, newBehaviour);
            yield return newBehaviour.Setup();
        }

        // Set the texture and/or sprite
        CardTemple temple = SaveManager.SaveFile.GetSceneAsCardTemple() ?? CardTemple.Nature;

        if (defn == null)
        {
            slot.ResetSlotTexture();
        }
        else if (slot is PixelCardSlot pcs)
        {
            pcs.SetSlotSprite(defn);
        }
        else
        {
            if (defn.Texture == null || !defn.Texture.ContainsKey(temple))
                slot.ResetSlotTexture();
            else
                slot.SetTexture(defn.Texture[temple]);
        }
    }

    /// <summary>
    /// Gets the current modification of a slot
    /// </summary>
    public static SlotModificationManager.ModificationType GetSlotModification(this CardSlot slot)
    {
        return slot == null
            ? SlotModificationManager.ModificationType.NoModification
            : SlotModificationManager.Instance.SlotReceivers.ContainsKey(slot)
            ? SlotModificationManager.Instance.SlotReceivers[slot].Item1
            : SlotModificationManager.ModificationType.NoModification;
    }

    private static void SetSlotSprite(this PixelCardSlot slot, SlotModificationManager.Info defn)
    {
        if (defn == null)
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Resetting slot {slot.Index} to default because mod info was null");
            slot.ResetSlotSprite();
            return;
        }

        if (defn.PixelBoardSlotSprites == null)
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Resetting slot {slot.Index} to default because mod info did not contain pixel slot info");
            slot.ResetSlotSprite();
            return;
        }

        var triggeringNPC = GBCEncounterManager.Instance?.GetTriggeringNPC();
        if (triggeringNPC == null)
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Doing nothing to slot {slot.Index} because the triggering NPC was null");
            return;
        }

        if (!defn.PixelBoardSlotSprites.ContainsKey(triggeringNPC.BattleBackgroundTheme))
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Resetting slot {slot.Index} to default because pixel slot info did not contain a definition for {triggeringNPC.BattleBackgroundTheme}");
            slot.ResetSlotSprite();
            return;
        }

        var spriteSet = defn.PixelBoardSlotSprites[triggeringNPC.BattleBackgroundTheme];
        if (spriteSet == null)
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Resetting slot {slot.Index} to default because pixel slot info had a null definition for {triggeringNPC.BattleBackgroundTheme}");
            slot.ResetSlotSprite();
            return;
        }

        var specificSprites = spriteSet.specificSlotSprites.Find(s => s.playerSlot == slot.IsPlayerSlot && s.index == slot.Index);

        if (specificSprites == null)
            slot.SetSprites(spriteSet.slotDefault, spriteSet.slotHighlight, slot.IsPlayerSlot && spriteSet.flipPlayerSlotSpriteY, false);
        else
            slot.SetSprites(specificSprites.slotDefault, specificSprites.slotHighlight, slot.IsPlayerSlot && spriteSet.flipPlayerSlotSpriteY, false);
    }

    private static void ResetSlotSprite(this PixelCardSlot slot)
    {
        var triggeringNPC = GBCEncounterManager.Instance?.GetTriggeringNPC();
        if (triggeringNPC == null)
            return;

        var spriteSet = PixelBoardSpriteSetter.Instance.themeSpriteSets.Find(s => s.id == triggeringNPC.BattleBackgroundTheme);
        if (spriteSet == null)
            return;

        var specificSprites = spriteSet.specificSlotSprites.Find(s => s.playerSlot == slot.IsPlayerSlot && s.index == slot.Index);

        if (specificSprites != null)
            slot.SetSprites(specificSprites.slotDefault, specificSprites.slotHighlight, slot.IsPlayerSlot && spriteSet.flipPlayerSlotSpriteY, false);
        else
            slot.SetSprites(spriteSet.slotDefault, spriteSet.slotHighlight, slot.IsPlayerSlot && spriteSet.flipPlayerSlotSpriteY, false);
    }

    /// <summary>
    /// Resets a slot's texture back to the default texture for that slot based on the current act.
    /// </summary>
    public static void ResetSlotTexture(this CardSlot slot)
    {
        if (slot is PixelCardSlot pcs)
        {
            pcs.ResetSlotSprite();
            return;
        }

        CardTemple temple = SaveManager.SaveFile.GetSceneAsCardTemple() ?? CardTemple.Nature;

        Dictionary<CardTemple, List<Texture>> lookup = slot.IsOpponentSlot() ? SlotModificationManager.OpponentOverrideSlots : SlotModificationManager.PlayerOverrideSlots;
        var newTexture = SlotModificationManager.DefaultSlotTextures[temple];
        if (lookup.ContainsKey(temple))
        {
            // Get the texture overrides
            var textureChoices = lookup[temple];
            int idx = slot.Index;
            if (idx >= textureChoices.Count)
            {
                // Try to guess what the best index would be
                int slotCount = BoardManager.Instance.PlayerSlotsCopy.Count;
                if (slot.Index == slotCount - 1) // the last slot
                    idx = textureChoices.Count - 1;
                else // Use the next to last slot
                    idx = textureChoices.Count - 2;
            }
            if (idx < 0)
                idx = 0;

            if (textureChoices[idx] != null)
                newTexture = textureChoices[idx];
        }

        slot.SetTexture(newTexture);
    }
}