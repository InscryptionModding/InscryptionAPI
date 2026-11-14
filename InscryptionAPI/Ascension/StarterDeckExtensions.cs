using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Ascension;

/// <summary>
/// Class that stores extensions related to starter decks.
/// </summary>
public static partial class StarterDeckExtensions
{
    private static Sprite GetPortrait(Texture2D portrait, TextureHelper.SpriteType spriteType, FilterMode? filterMode = null)
    {
        return portrait.ConvertTexture(spriteType, filterMode ?? FilterMode.Point);
    }

    /// <summary>
    /// Sets this starter deck's unlock level.
    /// </summary>
    /// <param name="info">The <see cref="StarterDeckInfo"/> of the Deck.</param>
    /// <param name="unlockLevel">The challenge level required before this starter deck is available.</param>
    public static void SetUnlockLevel(this StarterDeckInfo info, int unlockLevel)
    {
        StarterDeckManager.FullStarterDeck fullStarterDeck = StarterDeckManager.AllDecks.FirstOrDefault(x => x.Info == info);
        if (fullStarterDeck != null)
        {
            fullStarterDeck.UnlockLevel = unlockLevel;
        }
    }

    /// <summary>
    /// Gets the unlock level for the given <see cref="StarterDeckInfo"/>.
    /// </summary>
    /// <param name="info">The <see cref="StarterDeckInfo"/> to access.</param>
    /// <returns>Returns the starter deck's unlock level, or <c>-1</c> if an error occurs.</returns>
    public static int GetUnlockLevel(this StarterDeckInfo info)
    {
        StarterDeckManager.FullStarterDeck fullStarterDeck = StarterDeckManager.AllDecks.FirstOrDefault(x => x.Info == info);
        if (fullStarterDeck != null)
        {
            return fullStarterDeck.UnlockLevel;
        }
        return -1;
    }
}