using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public static partial class StarterDeckExtensions
{
    private static Sprite GetPortrait(Texture2D portrait, TextureHelper.SpriteType spriteType, FilterMode? filterMode = null)
    {
        return portrait.ConvertTexture(spriteType, filterMode ?? FilterMode.Point);
    }

    /// <summary>
    /// Sets the Unlock Level of a Starter Deck.
    /// </summary>
    /// <param name="info">The StarterDeckInfo of the Deck.</param>
    /// <param name="unlockLevel">The Unlock Level to set the Starter Deck with.</param>
    public static void SetUnlockLevel(this StarterDeckInfo info, int unlockLevel)
    {
        StarterDeckManager.FullStarterDeck fullStarterDeck = StarterDeckManager.AllDecks.FirstOrDefault(x => x.Info == info);
        if (fullStarterDeck != null)
        {
            fullStarterDeck.UnlockLevel = unlockLevel;
        }
    }

    /// <summary>
    /// Gets the Unlock Level associated with a Starter deck.
    /// </summary>
    /// <param name="info">The StarterDeckInfo of the Deck.</param>
    /// <returns>Returns the Unlock Level, if it fails it will return <see cref="int.MaxValue"/>.</returns>
    public static int GetUnlockLevel(this StarterDeckInfo info)
    {
        StarterDeckManager.FullStarterDeck fullStarterDeck = StarterDeckManager.AllDecks.FirstOrDefault(x => x.Info == info);
        if (fullStarterDeck != null)
        {
            return fullStarterDeck.UnlockLevel;
        }
        return int.MaxValue;
    }
}