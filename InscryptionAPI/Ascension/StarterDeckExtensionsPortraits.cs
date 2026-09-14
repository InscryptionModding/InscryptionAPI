using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Ascension;

public static partial class StarterDeckExtensions
{
    #region iconTexture
    /// <summary>
    /// Sets the icon texture for the starter deck.
    /// </summary>
    /// <param name="info">StarterDeckInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory).</param>
    /// <returns>The same StarterDeckInfo so a chain can continue.</returns>
    public static StarterDeckInfo SetIconTexture(this StarterDeckInfo info, string pathToArt)
    {
        try
        {
            return info.SetIconTexture(TextureHelper.GetImageAsTexture(pathToArt));
        }
        catch (FileNotFoundException fnfe)
        {
            throw new ArgumentException($"Image file not found for card \"{info.name}\"!", fnfe);
        }
    }

    /// <summary>
    /// Sets the icon texture for the starter deck.
    /// </summary>
    /// <param name="info">StarterDeckInfo to access.</param>
    /// <param name="portrait">The texture containing the emission.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same StarterDeckInfo so a chain can continue.</returns>
    public static StarterDeckInfo SetIconTexture(this StarterDeckInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetIconTexture(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the icon texture for the starter deck.
    /// </summary>
    /// <param name="info">StarterDeckInfo to access.</param>
    /// <param name="sprite">The sprite containing the emission.</param>
    /// <returns>The same StarterDeckInfo so a chain can continue.</returns>
    public static StarterDeckInfo SetIconTexture(this StarterDeckInfo info, Sprite sprite)
    {
        if (info.iconSprite == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting normal portrait!");

        info.iconSprite.RegisterEmissionForSprite(sprite);

        return info;
    }
    #endregion
}
