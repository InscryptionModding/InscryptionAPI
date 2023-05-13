using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;
using static InscryptionAPI.Card.AbilityManager;

namespace InscryptionAPI.Card;

/// <summary>
/// Helper extension methods for abilities
/// </summary>
public static class AbilityExtensions
{
    /// <summary>
    /// Gets an ability based on its unique identifier.
    /// </summary>
    /// <param name="abilities">The list of abiliites to search.</param>
    /// <param name="id">The unique ID of the ability.</param>
    /// <returns>The first ability with the given ID, or null if it was not found.</returns>
    public static AbilityInfo AbilityByID(this IEnumerable<AbilityInfo> abilities, Ability id) => abilities.FirstOrDefault(x => x.ability == id);

    /// <summary>
    /// Gets an ability based on its unique identifier.
    /// </summary>
    /// <param name="abilities">The list of abiliites to search.</param>
    /// <param name="id">The unique ID of the ability.</param>
    /// <returns>The first ability with the given ID, or null if it was not found.</returns>
    public static FullAbility AbilityByID(this IEnumerable<FullAbility> abilities, Ability id) => abilities.FirstOrDefault(x => x.Id == id);

    /// <summary>
    /// For internal use only.
    /// </summary>
    /// <remarks>This is instance-based: it will use the CWT in AbilityManager to get the *instance* of FullAbility
    /// that corresponds to this *instance* of AbilityInfo. If either of these are clones that may get GCd, then
    /// it would potentially be a mistake to use this helper. It should only be used internally when the implications
    /// are understood.</remarks>
    internal static FullAbility GetFullAbility(this AbilityInfo info)
    {
        if (info == null)
            return null;

        FullAbility.ReverseMapper.TryGetValue(info, out FullAbility retval);
        return retval;
    }

    /// <summary>
    /// Sets the icon texture for the ability.
    /// </summary>
    /// <param name="info">The ability info to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    /// <returns>The same ability info so a chain can continue.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the ability info has not yet been added to the AbilityManager</exception>
    /// <remarks>You cannot do this unless the ability has been registered with the API. Unless the API knows about this
    /// ability, it will not have the required information to be able to process the texture, so an exception will be thrown
    /// if you try to do this to an instance of AbilityInfo that did not get processed through the API.</remark>
    public static AbilityInfo SetIcon(this AbilityInfo info, Texture2D icon)
    {
        FullAbility ability = info.GetFullAbility();
        if (ability == null)
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via Add().");

        ability.SetIcon(icon);
        return info;
    }

    /// <summary>
    /// Sets the flipped texture for the ability (used when the ability belongs to the opponent)
    /// </summary>
    /// <param name="info">The ability info to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    /// <returns>The same ability info so a chain can continue.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the ability info has not yet been added to the AbilityManager</exception>
    /// <remarks>You cannot do this unless the ability has been registered with the API. Unless the API knows about this
    /// ability, it will not have the required information to be able to process the texture, so an exception will be thrown
    /// if you try to do this to an instance of AbilityInfo that did not get processed through the API.</remark>
    public static AbilityInfo SetCustomFlippedTexture(this AbilityInfo info, Texture2D icon)
    {
        FullAbility ability = info.GetFullAbility();
        if (ability == null)
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via Add().");

        ability.SetCustomFlippedTexture(icon);
        return info;
    }

    /// <summary>
    /// Sets the icon texture for the ability
    /// </summary>
    /// <param name="info">The ability info to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    public static void SetIcon(this FullAbility info, Texture2D icon)
    {
        info.Texture = icon;
    }

    /// <summary>
    /// Sets the flipped texture for the ability (used when the ability belongs to the opponent)
    /// </summary>
    /// <param name="info">The ability info to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    public static void SetCustomFlippedTexture(this FullAbility info, Texture2D icon)
    {
        info.CustomFlippedTexture = icon;
        info.Info.customFlippedIcon = true;
    }

    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand
    /// </summary>
    /// <param name="info">The instance of StatIconInfo.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same stat icon so a chain can continue.</returns>
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
    /// <param name="info">The instance of StatIconInfo.</param>
    /// <param name="pathToArt">The path to a 49x49 texture containing the icon on disk.</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same stat icon so a chain can continue.</returns>
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
    /// <param name="info">The instance of StatIconInfo.</param>
    /// <param name="icon">A 16x8 texture containing the icon .</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same stat icon so a chain can continue.</returns>
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
    /// <param name="info">The instance of AbilityInfo.</param>
    /// <param name="icon">A 17x17 texture containing the icon .</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same ability so a chain can continue.</returns>
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
    /// <param name="info">The instance of AbilityInfo.</param>
    /// <param name="categories">The metacategories to add.</param>
    /// <returns>The same ability so a chain can continue.</returns>
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
    /// <param name="info">The instance of StatIconInfo.</param>
    /// <param name="categories">The metacategories to add.</param>
    /// <returns>The same stat icon so a chain can continue.</returns>
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
    /// <param name="info">The instance of AbilityInfo.</param>
    /// <returns>The same ability so a chain can continue.</returns>
    public static AbilityInfo SetDefaultPart1Ability(this AbilityInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part1Modular, AbilityMetaCategory.Part1Rulebook);
    }

    /// <summary>
    /// Helper method: automatically adds the Part1Rulebook metacategories to the stat icon
    /// </summary>
    /// <param name="info">The instance of StatIconInfo.</param>
    /// <returns>The same stati icon so a chain can continue.</returns>
    public static StatIconInfo SetDefaultPart1Ability(this StatIconInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
    }

    /// <summary>
    /// Helper method: automatically adds the Part3Modular, Part3BuildACard, and Part3Rulebook metacategories to the ability
    /// </summary>
    /// <param name="info">The instance of AbilityInfo.</param>
    /// <returns>The same ability so a chain can continue.</returns>
    public static AbilityInfo SetDefaultPart3Ability(this AbilityInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part3Modular, AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3BuildACard);
    }

    /// <summary>
    /// Helper method: automatically adds the Part3Rulebook metacategories to the stat icon
    /// </summary>
    /// <param name="info">The instance of StatIconInfo.</param>
    /// <returns>The same stat icon so a chain can continue.</returns>
    public static StatIconInfo SetDefaultPart3Ability(this StatIconInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
    }

    /// <summary>
    /// Sets whether or not the ability is an activated ability.
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <param name="activated">If the ability is activated.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetActivated(this AbilityInfo abilityInfo, bool activated = true)
    {
        abilityInfo.activated = activated;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability is passive (will not trigger).
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <param name="passive">If the ability is passive.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetPassive(this AbilityInfo abilityInfo, bool passive = true)
    {
        abilityInfo.passive = passive;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability can be used by the opponent.
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <param name="opponentUsable">If the ability is usable by the opponent.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetOpponentUsable(this AbilityInfo abilityInfo, bool opponentUsable = true)
    {
        abilityInfo.opponentUsable = opponentUsable;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability is a conduit.
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <param name="conduit">If the ability is a conduit.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetConduit(this AbilityInfo abilityInfo, bool conduit = true)
    {
        abilityInfo.conduit = conduit;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability is a conduit cell.
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <param name="conduitCell">If the ability is a conduit cell.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetConduitCell(this AbilityInfo abilityInfo, bool conduitCell = true)
    {
        abilityInfo.conduitCell = conduitCell;
        return abilityInfo;
    }

    /// <summary>
    /// Sets whether or not the ability can stack on a card, triggering once for each stack.
    /// Optional parameter for setting the ability to only trigger once per stack when a card evolves (only affects abilities that can stack).
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <param name="triggersOncePerStack">Whether or not to prevent double triggering.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetCanStack(this AbilityInfo abilityInfo, bool canStack = true, bool triggersOncePerStack = false)
    {
        abilityInfo.canStack = canStack;
        abilityInfo.SetTriggersOncePerStack(triggersOncePerStack);
        return abilityInfo;
    }

    /// <summary>
    /// Sets the ability to only ever trigger once per stack. This prevents abilities from triggering twice per stack after a card evolves.
    /// This only affects cards that evolve into a card that possesses the same stackable ability (eg, default evolutions).
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <param name="triggersOncePerStack">Whether or not to prevent double triggering.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetTriggersOncePerStack(this AbilityInfo abilityInfo, bool triggersOncePerStack = true)
    {
        abilityInfo.SetExtendedProperty("TriggersOncePerStack", triggersOncePerStack);
        return abilityInfo;
    }

    /// <summary>
    /// Gets the value of TriggersOncePerStack. Returns false if TriggersOncePerStack has not been set.
    /// </summary>
    /// <param name="abilityInfo">Ability to access.</param>
    /// <returns>Whether double triggering is disabled.</returns>
    public static bool GetTriggersOncePerStack(this Ability ability)
    {
        AbilityInfo abilityInfo = AllAbilityInfos.AbilityByID(ability);
        return abilityInfo.GetTriggersOncePerStack();
    }
    /// <summary>
    /// Gets the value of TriggersOncePerStack. Returns false if TriggersOncePerStack has not been set.
    /// </summary>
    /// <param name="abilityInfo">The instance of AbilityInfo.</param>
    /// <returns>Whether double triggering is disabled.</returns>
    public static bool GetTriggersOncePerStack(this AbilityInfo abilityInfo)
    {
        return abilityInfo.GetExtendedPropertyAsBool("TriggersOncePerStack") ?? false;
    }

    #region ExtendedProperties

    /// <summary>
    /// Adds a custom property value to the ability.
    /// </summary>
    /// <param name="info">Ability to access.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same AbilityInfo so a chain can continue.</returns>
    public static AbilityInfo SetExtendedProperty(this AbilityInfo info, string propertyName, object value)
    {
        info.GetAbilityExtensionTable()[propertyName] = value?.ToString();
        return info;
    }

    /// <summary>
    /// Gets a custom property value from the card.
    /// </summary>
    /// <param name="ability">Ability to access.</param>
    /// <param name="propertyName">The name of the property to get the value of.</param>
    /// <returns>The retrieved property if it exists.</returns>
    public static string GetExtendedProperty(this Ability ability, string propertyName)
    {
        AbilityInfo info = AllAbilityInfos.AbilityByID(ability);
        return info.GetExtendedProperty(propertyName);
    }
    /// <summary>
    /// Gets a custom property value from the card.
    /// </summary>
    /// <param name="info">Ability to access.</param>
    /// <param name="propertyName">The name of the property to get the value of.</param>
    /// <returns>The retrieved property if it exists.</returns>
    public static string GetExtendedProperty(this AbilityInfo info, string propertyName)
    {
        info.GetAbilityExtensionTable().TryGetValue(propertyName, out var ret);
        return ret;
    }

    /// <summary>
    /// Gets a custom property as an int (can by null)
    /// </summary>
    /// <param name="ability">Ability to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int.</returns>
    public static int? GetExtendedPropertyAsInt(this Ability ability, string propertyName)
    {
        AbilityInfo info = AllAbilityInfos.AbilityByID(ability);
        return info.GetExtendedPropertyAsInt(propertyName);
    }
    /// <summary>
    /// Gets a custom property as an int (can by null)
    /// </summary>
    /// <param name="info">Ability to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int.</returns>
    public static int? GetExtendedPropertyAsInt(this AbilityInfo info, string propertyName)
    {
        info.GetAbilityExtensionTable().TryGetValue(propertyName, out var str);
        return int.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a float (can by null)
    /// </summary>
    /// <param name="ability">Ability to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float.</returns>
    public static float? GetExtendedPropertyAsFloat(this Ability ability, string propertyName)
    {
        AbilityInfo info = AllAbilityInfos.AbilityByID(ability);
        return info.GetExtendedPropertyAsFloat(propertyName);
    }
    /// <summary>
    /// Gets a custom property as a float (can by null)
    /// </summary>
    /// <param name="info">Ability to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float.</returns>
    public static float? GetExtendedPropertyAsFloat(this AbilityInfo info, string propertyName)
    {
        info.GetAbilityExtensionTable().TryGetValue(propertyName, out var str);
        return float.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a boolean (can be null)
    /// </summary>
    /// <param name="ability">Ability to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean.</returns>
    public static bool? GetExtendedPropertyAsBool(this Ability ability, string propertyName)
    {
        AbilityInfo info = AllAbilityInfos.AbilityByID(ability);
        return info.GetExtendedPropertyAsBool(propertyName);
    }
    /// <summary>
    /// Gets a custom property as a boolean (can be null)
    /// </summary>
    /// <param name="info">Ability to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean.</returns>
    public static bool? GetExtendedPropertyAsBool(this AbilityInfo info, string propertyName)
    {
        info.GetAbilityExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }

    #endregion
}
