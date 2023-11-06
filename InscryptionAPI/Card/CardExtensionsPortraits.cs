using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Card;

public static partial class CardExtensions
{
    #region Main
    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortrait(this CardInfo info, string pathToArt)
    {
        try
        {
            return info.SetPortrait(TextureHelper.GetImageAsTexture(pathToArt));
        }
        catch (FileNotFoundException fnfe)
        {
            throw new ArgumentException($"Image file not found for card \"{info.name}\"!", fnfe);
        }
    }

    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="emission">The texture containing the emission.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>.</returns>
    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, Texture2D emission, FilterMode? filterMode = null)
    {
        info.SetPortrait(portrait, filterMode);
        info.SetEmissivePortrait(emission, filterMode);
        return info;
    }

    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="sprite">The sprite containing the card portrait.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortrait(this CardInfo info, Sprite sprite)
    {
        info.portraitTex = sprite;

        if (!string.IsNullOrEmpty(info.name))
            info.portraitTex.name = info.name + "_portrait";

        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <param name="pathToEmission">The path to the .png file containing the emission artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, string pathToArt, string pathToEmission)
    {
        info.SetPortrait(pathToArt);
        info.SetEmissivePortrait(pathToEmission);
        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="emission">The texture containing the emission.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, Texture2D portrait, Texture2D emission, FilterMode? filterMode = null)
    {
        info.SetPortrait(portrait, filterMode);
        info.SetEmissivePortrait(emission, filterMode);
        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait.</param>
    /// <param name="emission">The sprite containing the emission.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, Sprite portrait, Sprite emission)
    {
        info.SetPortrait(portrait);
        info.SetEmissivePortrait(emission);
        return info;
    }

    #endregion

    #region Alt
    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetAltPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetAltPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, Sprite portrait)
    {
        info.alternatePortrait = portrait;
        if (!string.IsNullOrEmpty(info.name))
            info.alternatePortrait.name = info.name + "_altportrait";

        TextureHelper.TryReuseEmission(info, info.alternatePortrait);

        return info;
    }

    #endregion

    #region Emissive
    /// <summary>
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, string pathToArt)
    {
        try
        {
            return info.SetEmissivePortrait(TextureHelper.GetImageAsTexture(pathToArt));
        }
        catch (FileNotFoundException fnfe)
        {
            throw new ArgumentException($"Image file not found for card \"{info.name}\"!", fnfe);
        }
    }

    /// <summary>
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the emission.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissivePortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="sprite">The sprite containing the emission.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, Sprite sprite)
    {
        if (info.portraitTex == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting normal portrait!");

        info.portraitTex.RegisterEmissionForSprite(sprite);

        return info;
    }

    public static Sprite GetEmissivePortrait(this CardInfo info) => info.portraitTex.GetEmissionSprite();

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the alternate portrait has been set (SetAltPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetEmissiveAltPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the alternate portrait has been set (SetAltPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the emission.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissiveAltPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the alternate portrait has been set (SetAltPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the emission.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, Sprite portrait)
    {
        if (info.alternatePortrait == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting the alternate portrait!");

        info.alternatePortrait.RegisterEmissionForSprite(portrait);

        return info;
    }

    public static Sprite GetEmissiveAltPortrait(this CardInfo info) => info.alternatePortrait.GetEmissionSprite();

    #endregion

    #region Pixel
    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelPortrait(portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode ?? default));
    }

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, Sprite portrait)
    {
        info.pixelPortrait = portrait;

        if (!string.IsNullOrEmpty(info.name))
            info.pixelPortrait.name = info.name + "_pixelportrait";

        return info;
    }

    public static Sprite GetPixelPortrait(this CardInfo info)
    {
        return info.pixelPortrait;
    }

    #endregion

    #region LostTail
    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetLostTailPortrait(this CardInfo info, string pathToArt)
    {
        if (info.tailParams == null)
            throw new InvalidOperationException("Cannot set lost tail portrait without tail params being set first");

        info.tailParams.SetLostTailPortrait(pathToArt, info);

        return info;
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetLostTailPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        if (info.tailParams == null)
            throw new InvalidOperationException("Cannot set lost tail portrait without tail params being set first");

        info.tailParams.SetLostTailPortrait(portrait, info, filterMode);

        return info;
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Tail to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue.</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, string pathToArt, CardInfo owner)
    {
        owner.tailParams = info;
        return info.SetLostTailPortrait(TextureHelper.GetImageAsTexture(pathToArt), owner);
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Tail to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue.</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, Texture2D portrait, CardInfo owner, FilterMode? filterMode = null)
    {
        var tailSprite = !filterMode.HasValue
                             ? portrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
                             : portrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);

        return info.SetLostTailPortrait(tailSprite, owner);
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Tail to access.</param>
    /// <param name="portrait">The sprite containing the card portrait.</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue.</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, Sprite portrait, CardInfo owner)
    {
        info.tailLostPortrait = portrait;

        if (!string.IsNullOrEmpty(owner.name))
            info.tailLostPortrait.name = owner.name + "_tailportrait";

        TextureHelper.TryReuseEmission(owner, info.tailLostPortrait);

        return info;
    }

    #endregion

    #region Pixel Alt
    /// <summary>
    /// Sets the card's pixel alternate portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelAlternatePortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelAlternatePortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    /// <summary>
    /// Sets the card's pixel alternate portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelAlternatePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelAlternatePortrait(portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode ?? default));
    }
    /// <summary>
    /// Sets the card's pixel alternate portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The sprite containing the card portrait.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelAlternatePortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_pixelalternateportrait";

        info.GetAltPortraits().PixelAlternatePortrait = portrait;
        return info;
    }

    public static Sprite GetPixelAlternatePortrait(this CardInfo info) => info.PixelAlternatePortrait();
    #endregion

    #region Steel Trap Alt
    /// <summary>
    /// Sets the card's alternate steep trap portrait. This portrait is only used when the Steel Trap ability triggers.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetSteelTrapPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetSteelTrapPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    public static CardInfo SetSteelTrapPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetSteelTrapPortrait(portrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode ?? default));
    }

    public static CardInfo SetSteelTrapPortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_steeltrapportrait";

        info.GetAltPortraits().SteelTrapPortrait = portrait;
        return info;
    }

    /// <summary>
    /// Sets the card's alternate steep trap portrait. This portrait is only used when the Steel Trap ability triggers in Act 2.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelSteelTrapPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelSteelTrapPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    /// <summary>
    /// Sets the card's alternate steep trap portrait. This portrait is only used when the Steel Trap ability triggers in Act 2.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelSteelTrapPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelSteelTrapPortrait(portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode ?? default));
    }
    public static CardInfo SetPixelSteelTrapPortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_pixelsteeltrapportrait";

        info.GetAltPortraits().PixelSteelTrapPortrait = portrait;
        return info;
    }

    /// <summary>
    /// Sets the emissive steel trap portrait for the card. This can only be done after the default steel trap portrait has been set (SetSteelTrapPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveSteelTrapPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetEmissiveSteelTrapPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    public static CardInfo SetEmissiveSteelTrapPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissiveSteelTrapPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }
    public static CardInfo SetEmissiveSteelTrapPortrait(this CardInfo info, Sprite portrait)
    {
        if (info.SteelTrapPortrait() == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting the default steel trap portrait!");

        info.SteelTrapPortrait().RegisterEmissionForSprite(portrait);
        return info;
    }
    public static Sprite GetEmissiveSteelTrapPortrait(this CardInfo info) => info.SteelTrapPortrait()?.GetEmissionSprite();
    #endregion

    #region Shield Alt
    /// <summary>
    /// Sets the card's alternate broken shield portrait. This portrait is only used when a card with a shield-giving sigil loses all its shields.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBrokenShieldPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetBrokenShieldPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    public static CardInfo SetBrokenShieldPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetBrokenShieldPortrait(portrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode ?? default));
    }
    public static CardInfo SetBrokenShieldPortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_brokenshieldportrait";

        info.GetAltPortraits().BrokenShieldPortrait = portrait;
        return info;
    }

    /// <summary>
    /// Sets the card's alternate pixel broken shield portrait. This portrait is only used in Act 2.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelBrokenShieldPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelBrokenShieldPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    public static CardInfo SetPixelBrokenShieldPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelBrokenShieldPortrait(portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode ?? default));
    }
    public static CardInfo SetPixelBrokenShieldPortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_pixelbrokenshieldportrait";

        info.GetAltPortraits().BrokenShieldPortrait = portrait;
        return info;
    }

    /// <summary>
    /// Sets the emissive broken shield portrait for the card. This can only be done after the default broken shield portrait has been set (SetBrokenShieldPortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveBrokenShieldPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetEmissiveBrokenShieldPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    public static CardInfo SetEmissiveBrokenShieldPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissiveBrokenShieldPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }
    public static CardInfo SetEmissiveBrokenShieldPortrait(this CardInfo info, Sprite portrait)
    {
        if (info.BrokenShieldPortrait() == null)
            throw new InvalidOperationException($"Cannot set the emissive portrait before setting the default broken shield portrait!");

        info.BrokenShieldPortrait().RegisterEmissionForSprite(portrait);
        return info;
    }
    public static Sprite GetEmissiveBrokenShieldPortrait(this CardInfo info) => info.BrokenShieldPortrait()?.GetEmissionSprite();
    #endregion

    #region Sacrifice Alt
    /// <summary>
    /// Sets the card's alternate sacricable portrait. This portrait is only used when a card with this CardInfo is on the board when the player is selecting sacrifices.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetSacrificablePortrait(this CardInfo info, string pathToArt)
    {
        return info.SetSacrificablePortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    public static CardInfo SetSacrificablePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetSacrificablePortrait(portrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode ?? default));
    }
    public static CardInfo SetSacrificablePortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_sacrificableportrait";

        info.GetAltPortraits().SacrificablePortrait = portrait;
        return info;
    }

    /// <summary>
    /// Sets the card's pixel sacrificable portrait. This portrait is only used in Act 2.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPixelSacrificablePortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelSacrificablePortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    public static CardInfo SetPixelSacrificablePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelSacrificablePortrait(portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode ?? default));
    }
    public static CardInfo SetPixelSacrificablePortrait(this CardInfo info, Sprite portrait)
    {
        if (!string.IsNullOrEmpty(info.name))
            portrait.name = info.name + "_pixelsacrificableportrait";

        info.GetAltPortraits().PixelSacrificablePortrait = portrait;
        return info;
    }

    /// <summary>
    /// Sets the emissive sacrifcable portrait for the card. This can only be done after the sacrificable portrait has been set (SetSacrifablePortrait).
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEmissiveSacrificablePortrait(this CardInfo info, string pathToArt)
    {
        return info.SetEmissiveSacrificablePortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }
    public static CardInfo SetEmissiveSacrificablePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissiveSacrificablePortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }
    public static CardInfo SetEmissiveSacrificablePortrait(this CardInfo info, Sprite portrait)
    {
        if (info.SteelTrapPortrait() == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting the default sacrifice portrait!");

        info.SteelTrapPortrait().RegisterEmissionForSprite(portrait);
        return info;
    }
    public static Sprite GetEmissiveSacrificablePortrait(this CardInfo info) => info.SteelTrapPortrait().GetEmissionSprite();
    #endregion

    /// <summary>
    /// Sets the animated portrait for the given CardInfo.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="portrait">The to check for.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetAnimatedPortrait(this CardInfo info, GameObject portrait)
    {
        info.animatedPortrait = portrait;
        return info;
    }

    public static bool HasAlternatePortrait(this PlayableCard card) => card.Info.HasAlternatePortrait();
    public static bool HasAlternatePortrait(this CardInfo info) => info.alternatePortrait != null;
    public static bool HasPixelAlternatePortrait(this CardInfo info) => info.PixelAlternatePortrait() != null;
    public static bool HasSteelTrapPortrait(this CardInfo info) => info.SteelTrapPortrait() != null;
    public static bool HasPixelSteelTrapPortrait(this CardInfo info) => info.PixelSteelTrapPortrait() != null;
    public static bool HasBrokenShieldPortrait(this CardInfo info) => info.BrokenShieldPortrait() != null;
    public static bool HasPixelBrokenShieldPortrait(this CardInfo info) => info.PixelBrokenShieldPortrait() != null;
    public static bool HasSacrificablePortrait(this CardInfo info) => info.SacrificablePortrait() != null;
    public static bool HasPixelSacrificablePortrait(this CardInfo info) => info.PixelSacrificablePortrait() != null;
}