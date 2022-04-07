using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Card;

/// <summary>
/// Helper extension methods for abilities
/// </summary>
public static class AbilityExtensions
{
    /// <summary>
    /// Gets an ability based on its unique identifier
    /// </summary>
    /// <param name="abilities">The list of abiliites to search</param>
    /// <param name="id">The unique ID of the ability</param>
    /// <returns>The first ability with the given ID, or null if it was not found</returns>
    public static AbilityInfo AbilityByID(this IEnumerable<AbilityInfo> abilities, Ability id) => abilities.FirstOrDefault(x => x.ability == id);

    /// <summary>
    /// Gets an ability based on its unique identifier
    /// </summary>
    /// <param name="abilities">The list of abiliites to search</param>
    /// <param name="id">The unique ID of the ability</param>
    /// <returns>The first ability with the given ID, or null if it was not found</returns>
    public static AbilityManager.FullAbility AbilityByID(this IEnumerable<AbilityManager.FullAbility> abilities, Ability id) => abilities.FirstOrDefault(x => x.Id == id);

    /// <summary>
    /// For internal use only
    /// </summary>
    /// <remarks>This is instance-based: it will use the CWT in AbilityManager to get the *instance* of FullAbility
    /// that corresponds to this *instance* of AbilityInfo. If either of these are clones that may get GCd, then
    /// it would potentially be a mistake to use this helper. It should only be used internally when the implications
    /// are understood.</remarks>
    internal static AbilityManager.FullAbility GetFullAbility(this AbilityInfo info)
    {
        if (info == null)
            return null;

        AbilityManager.FullAbility retval = null;
        AbilityManager.FullAbility.ReverseMapper.TryGetValue(info, out retval);
        return retval;
    }

    /// <summary>
    /// Sets the icon texture for the ability
    /// </summary>
    /// <param name="info">The ability info to set the texture for</param>
    /// <param name="icon">A 49x49 texture containing the icon</param>
    /// <returns>The same ability info so a chain can continue</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the ability info has not yet been added to the AbilityManager</exception>
    /// <remarks>You cannot do this unless the ability has been registered with the API. Unless the API knows about this
    /// ability, it will not have the required information to be able to process the texture, so an exception will be thrown
    /// if you try to do this to an instance of AbilityInfo that did not get processed through the API.</remark>
    public static AbilityInfo SetIcon(this AbilityInfo info, Texture2D icon)
    {
        AbilityManager.FullAbility ability = info.GetFullAbility();
        if (ability == null)
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via AbilityManager.Add");

        ability.SetIcon(icon);
        return info;
    }

    /// <summary>
    /// Sets the flipped texture for the ability (used when the ability belongs to the opponent)
    /// </summary>
    /// <param name="info">The ability info to set the texture for</param>
    /// <param name="icon">A 49x49 texture containing the icon</param>
    /// <returns>The same ability info so a chain can continue</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the ability info has not yet been added to the AbilityManager</exception>
    /// <remarks>You cannot do this unless the ability has been registered with the API. Unless the API knows about this
    /// ability, it will not have the required information to be able to process the texture, so an exception will be thrown
    /// if you try to do this to an instance of AbilityInfo that did not get processed through the API.</remark>
    public static AbilityInfo SetCustomFlippedTexture(this AbilityInfo info, Texture2D icon)
    {
        AbilityManager.FullAbility ability = info.GetFullAbility();
        if (ability == null)
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via AbilityManager.Add");

        ability.SetCustomFlippedTexture(icon);
        return info;        
    }

    /// <summary>
    /// Sets the icon texture for the ability
    /// </summary>
    /// <param name="info">The ability info to set the texture for</param>
    /// <param name="icon">A 49x49 texture containing the icon</param>
    public static void SetIcon(this AbilityManager.FullAbility info, Texture2D icon)
    {
        info.Texture = icon;
    }

    /// <summary>
    /// Sets the flipped texture for the ability (used when the ability belongs to the opponent)
    /// </summary>
    /// <param name="info">The ability info to set the texture for</param>
    /// <param name="icon">A 49x49 texture containing the icon</param>
    public static void SetCustomFlippedTexture(this AbilityManager.FullAbility info, Texture2D icon)
    {
        info.CustomFlippedTexture = icon;
        info.Info.customFlippedIcon = true;
    }

    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand
    /// </summary>
    /// <param name="info">The instance of StatIconInfo</param>
    /// <param name="icon">A 49x49 texture containing the icon</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same stat icon so a chain can continue</returns>
    public static StatIconInfo SetIcon(this StatIconInfo info, Texture2D icon, FilterMode? filterMode = null)
    {
        info.iconGraphic = icon;
        if (filterMode.HasValue)
            info.iconGraphic.filterMode = filterMode.Value;
        return info;
    }
    
    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand
    /// </summary>
    /// <param name="info">The instance of StatIconInfo</param>
    /// <param name="pathToArt">The path to a 49x49 texture containing the icon on disk</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same stat icon so a chain can continue</returns>
    public static StatIconInfo SetIcon(this StatIconInfo info, string pathToArt, FilterMode? filterMode = null)
    {
        info.iconGraphic = TextureHelper.GetImageAsTexture(pathToArt);
        if (filterMode.HasValue)
            info.iconGraphic.filterMode = filterMode.Value;
        return info;
    }
    
    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand in GBC mode
    /// </summary>
    /// <param name="info">The instance of StatIconInfo</param>
    /// <param name="icon">A 16x8 texture containing the icon </param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same stat icon so a chain can continue</returns>
    public static StatIconInfo SetPixelIcon(this StatIconInfo info, Texture2D icon, FilterMode? filterMode = null)
    {
        if (!filterMode.HasValue)
            info.pixelIconGraphic = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelStatIcon);
        else
            info.pixelIconGraphic = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelStatIcon, filterMode.Value);
        return info;
    }

    /// <summary>
    /// Sets the icon that will be displayed for this ability icon in GBC mode
    /// </summary>
    /// <param name="info">The instance of AbilityInfo</param>
    /// <param name="icon">A 17x17 texture containing the icon </param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same ability so a chain can continue</returns>
    public static AbilityInfo SetPixelAbilityIcon(this AbilityInfo info, Texture2D icon, FilterMode? filterMode = null)
    {
        if (!filterMode.HasValue)
            info.pixelIcon = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelAbilityIcon);
        else
            info.pixelIcon = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelAbilityIcon, filterMode.Value);
        return info;
    }

    /// <summary>
    /// Adds one or more metacategories to the ability. Duplicate abilities will not be added
    /// </summary>
    /// <param name="info">The instance of AbilityInfo</param>
    /// <param name="categories">The metacategories to add</param>
    /// <returns>The same ability so a chain can continue</returns>
    public static AbilityInfo AddMetaCategories(this AbilityInfo info, params AbilityMetaCategory[] categories)
    {
        info.metaCategories = info.metaCategories ?? new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    /// <summary>
    /// Adds one or more metacategories to the stati icon. Duplicate abilities will not be added
    /// </summary>
    /// <param name="info">The instance of StatIconInfo</param>
    /// <param name="categories">The metacategories to add</param>
    /// <returns>The same stat icon so a chain can continue</returns>
    public static StatIconInfo AddMetaCategories(this StatIconInfo info, params AbilityMetaCategory[] categories)
    {
        info.metaCategories = info.metaCategories ?? new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    /// <summary>
    /// Helper method: automatically adds the Part1Modular and Part1Rulebook metacategories to the ability
    /// </summary>
    /// <param name="info">The instance of AbilityInfo</param>
    /// <returns>The same ability so a chain can continue</returns>
    public static AbilityInfo SetDefaultPart1Ability(this AbilityInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part1Modular, AbilityMetaCategory.Part1Rulebook);
    }

    /// <summary>
    /// Helper method: automatically adds the Part1Rulebook metacategories to the stat icon
    /// </summary>
    /// <param name="info">The instance of StatIconInfo</param>
    /// <returns>The same stati icon so a chain can continue</returns>
    public static StatIconInfo SetDefaultPart1Ability(this StatIconInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
    }

    /// <summary>
    /// Helper method: automatically adds the Part3Modular, Part3BuildACard, and Part3Rulebook metacategories to the ability
    /// </summary>
    /// <param name="info">The instance of AbilityInfo</param>
    /// <returns>The same ability so a chain can continue</returns>
    public static AbilityInfo SetDefaultPart3Ability(this AbilityInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part3Modular, AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3BuildACard);
    }

    /// <summary>
    /// Helper method: automatically adds the Part3Rulebook metacategories to the stat icon
    /// </summary>
    /// <param name="info">The instance of StatIconInfo</param>
    /// <returns>The same stati icon so a chain can continue</returns>
    public static StatIconInfo SetDefaultPart3Ability(this StatIconInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
    }
}
