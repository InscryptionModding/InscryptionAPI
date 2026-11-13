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
    /// <remarks>This is instance-based: it will use the CWT in <see cref="AbilityManager"/> to get the *instance* of <see cref="FullAbility"/>
    /// that corresponds to this *instance* of <see cref="FullAbility"/>. If either of these are clones that may get GCd, then
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
    /// Retrieves the original, unmodified rulebook description of the given <see cref="AbilityInfo"/>.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <returns>The original description for this ability.</returns>
    public static string GetBaseRulebookDescription(this AbilityInfo info)
    {
        FullAbility fullAbility = AllAbilities.Find(x => x.Info == info);
        if (fullAbility == default(FullAbility))
            return null;

        return fullAbility.BaseRulebookDescription;
    }

    /// <summary>
    /// Sets the icon texture for the ability.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the <see cref="AbilityInfo"/> has not yet been added to the AbilityManager</exception>
    /// <remarks>You cannot do this unless the ability has been registered with the API. Unless the API knows about this
    /// ability, it will not have the required information to be able to process the texture, so an exception will be thrown
    /// if you try to do this to an instance of <see cref="AbilityInfo"/> that did not get processed through the API.</remarks>
    public static AbilityInfo SetIcon(this AbilityInfo info, Texture2D icon)
    {
        FullAbility ability = info.GetFullAbility();
        if (ability == null)
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via Add().");

        ability.SetIcon(icon);
        return info;
    }

    /// <summary>
    /// Sets the icon texture for the ability.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    public static void SetIcon(this FullAbility info, Texture2D icon) {
        info.Texture = icon;
    }

    /// <summary>
    /// Sets the flipped texture for the ability (used when the ability belongs to the opponent).
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the <see cref="AbilityInfo"/> has not yet been added to the AbilityManager</exception>
    /// <remarks>You cannot do this unless the ability has been registered with the API. Unless the API knows about this
    /// ability, it will not have the required information to be able to process the texture, so an exception will be thrown
    /// if you try to do this to an instance of <see cref="AbilityInfo"/> that did not get processed through the API.</remarks>
    public static AbilityInfo SetCustomFlippedTexture(this AbilityInfo info, Texture2D icon)
    {
        FullAbility ability = info.GetFullAbility();
        if (ability == null)
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via Add().");

        ability.SetCustomFlippedTexture(icon);
        return info;
    }

    /// <summary>
    /// Sets the flipped texture for the ability (used when the ability belongs to the opponent).
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to set the texture for.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    public static void SetCustomFlippedTexture(this FullAbility info, Texture2D icon)
    {
        info.CustomFlippedTexture = icon;
        info.Info.customFlippedIcon = true;
    }

    /// <summary>
    /// Sets the icon that will be displayed for this ability icon in Act 2.
    /// </summary>
    /// <param name="info">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="icon">A 17x17 or 22x10 texture containing the icon; for regular and activated sigil icons respectively.</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetPixelAbilityIcon(this AbilityInfo info, Texture2D icon, FilterMode? filterMode = null)
    {
        TextureHelper.SpriteType spriteType = icon.width == 22 ? TextureHelper.SpriteType.PixelActivatedAbilityIcon : TextureHelper.SpriteType.PixelAbilityIcon;

        info.pixelIcon = TextureHelper.ConvertTexture(icon, spriteType, filterMode ?? FilterMode.Point);
        return info;
    }

    /// <summary>
    /// Adds one or more metacategories to the ability. Duplicate categories will not be added.
    /// </summary>
    /// <param name="info">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="categories">The metacategories to add.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo AddMetaCategories(this AbilityInfo info, params AbilityMetaCategory[] categories)
    {
        info.metaCategories ??= new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    /// <summary>
    /// Helper method: automatically adds the <c>Part1Modular</c> and <c>Part1Rulebook</c> metacategories to the ability.
    /// </summary>
    /// <param name="info">The instance of <see cref="AbilityInfo"/>.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetDefaultPart1Ability(this AbilityInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part1Modular, AbilityMetaCategory.Part1Rulebook);
    }

    /// <summary>
    /// Helper method: automatically adds the custom metacategory <c>Part2Modular</c> to the ability.
    /// </summary>
    /// <param name="info">The instance of <see cref="AbilityInfo"/>.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetDefaultPart2Ability(this AbilityInfo info) => info.AddMetaCategories(Part2Modular);

    /// <summary>
    /// Helper method: automatically adds the <c>Part3Modular</c>, <c>Part3BuildACard</c>, and <c>Part3Rulebook</c> metacategories to the ability.
    /// </summary>
    /// <param name="info">The instance of <see cref="AbilityInfo"/>.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetDefaultPart3Ability(this AbilityInfo info)
    {
        return info.AddMetaCategories(AbilityMetaCategory.Part3Modular, AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3BuildACard);
    }

    /// <summary>
    /// Sets the text displayed when this ability is marked as learned.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="lines">The text to display, represented by strings.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetAbilityLearnedDialogue(this AbilityInfo abilityInfo, params string[] lines)
    {
        if (lines.Length > 0)
        {
            List<DialogueEvent.Line> list = new();
            foreach (var line in lines)
                list.Add(new() { text = line });

            abilityInfo.abilityLearnedDialogue = new() { lines = list };
        }

        return abilityInfo;
    }
    /// <summary>
    /// Sets the text displayed when this ability is marked as learned.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="lines">The text to display, represented by <c>DialogueEvent.Line</c>'s.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetAbilityLearnedDialogue(this AbilityInfo abilityInfo, params DialogueEvent.Line[] lines)
    {
        if (lines.Length > 0)
            abilityInfo.abilityLearnedDialogue = new() { lines = lines.ToList() };

        return abilityInfo;
    }

    /// <summary>
    /// Sets the text displayed whenever <c>OnAbilityTriggered</c> is called by this ability in Act 2.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="triggerText">The text to display when <c>OnAbilityTriggered</c> is called.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetGBCTriggerText(this AbilityInfo abilityInfo, string triggerText)
    {
        abilityInfo.triggerText = triggerText;
        return abilityInfo;
    }
    /// <summary>
    /// Sets the text displayed whenever <c>OnAbilityTriggered</c> is called by this ability in Act 2.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="triggerText">The text to display when <c>OnAbilityTriggered</c> is called.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetGBCTriggerText(this FullAbility fullAbility, string triggerText) {
        fullAbility.Info.triggerText = triggerText;
        return fullAbility;
    }

    /// <summary>
    /// Sets the power level of the ability, used in some game logic like determining the opponent totem's ability.
    /// Vanilla power levels range from -3 to 5, and values above or below are ignored in most cases.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="powerLevel">The ability's power level. Should be equal to or between -3 and 5.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetPowerlevel(this AbilityInfo abilityInfo, int powerLevel)
    {
        abilityInfo.powerLevel = powerLevel;
        return abilityInfo;
    }
    /// <summary>
    /// Sets the power level of the ability, used in some game logic like determining the opponent totem's ability.
    /// Vanilla power levels range from -3 to 5, and values above or below are ignored in most cases.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="powerLevel">The ability's power level. Should be equal to or between -3 and 5.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetPowerlevel(this FullAbility fullAbility, int powerLevel) {
        fullAbility.Info.powerLevel = powerLevel;
        return fullAbility;
    }
    /// <summary>
    /// Sets the rulebook description for this ability.
    /// </summary>
    /// <param name="abilityInfo">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="description">The ability's rulebook name.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetRulebookDescription(this AbilityInfo abilityInfo, string description)
    {
        abilityInfo.rulebookDescription = description;
        return abilityInfo;
    }
    /// <summary>
    /// Sets the rulebook description for this ability.
    /// </summary>
    /// <param name="fullAbility">The <see cref="FullAbility"/> to access.</param>
    /// <param name="description">The ability's rulebook name.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetRulebookDescription(this FullAbility fullAbility, string description) {
        fullAbility.Info.rulebookDescription = description;
        return fullAbility;
    }
    /// <summary>
    /// Sets the rulebook name for this ability.
    /// </summary>
    /// <param name="abilityInfo">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="name">The ability's rulebook name.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetRulebookName(this AbilityInfo abilityInfo, string name)
    {
        abilityInfo.rulebookName = name;
        return abilityInfo;
    }
    /// <summary>
    /// Sets the rulebook name for this ability.
    /// </summary>
    /// <param name="fullAbility">The <see cref="FullAbility"/> to access.</param>
    /// <param name="name">The ability's rulebook name.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetRulebookName(this FullAbility fullAbility, string name) {
        fullAbility.Info.rulebookName = name;
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability is an activated ability.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="activated">If the ability is activated.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetActivated(this AbilityInfo abilityInfo, bool activated = true)
    {
        abilityInfo.activated = activated;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability is an activated ability.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="activated">If the ability is activated.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetActivated(this FullAbility fullAbility, bool activated = true) {
        fullAbility.Info.activated = activated;
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability is passive (will not trigger).
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="passive">If the ability is passive.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetPassive(this AbilityInfo abilityInfo, bool passive = true)
    {
        abilityInfo.passive = passive;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability is passive (will not trigger).
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="passive">If the ability is passive.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetPassive(this FullAbility fullAbility, bool passive = true) {
        fullAbility.Info.passive = passive;
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability can be used by the opponent.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="opponentUsable">If the ability is usable by the opponent.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetOpponentUsable(this AbilityInfo abilityInfo, bool opponentUsable = true)
    {
        abilityInfo.opponentUsable = opponentUsable;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability can be used by the opponent.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="opponentUsable">If the ability is usable by the opponent.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetOpponentUsable(this FullAbility fullAbility, bool opponentUsable = true) {
        fullAbility.Info.opponentUsable = opponentUsable;
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability is a conduit.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="conduit">If the ability is a conduit.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetConduit(this AbilityInfo abilityInfo, bool conduit = true)
    {
        abilityInfo.conduit = conduit;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability is a conduit.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="conduit">If the ability is a conduit.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetConduit(this FullAbility fullAbility, bool conduit = true) {
        fullAbility.Info.conduit = conduit;
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability is a conduit cell.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="conduitCell">If the ability is a conduit cell.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetConduitCell(this AbilityInfo abilityInfo, bool conduitCell = true)
    {
        abilityInfo.conduitCell = conduitCell;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability is a conduit cell.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="conduitCell">If the ability is a conduit cell.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetConduitCell(this FullAbility fullAbility, bool conduitCell = true) {
        fullAbility.Info.conduitCell = conduitCell;
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability can stack on a card, triggering once for each stack.
    /// Optional parameter for setting the ability to only trigger once per stack when a card evolves (only affects abilities that can stack).
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="triggersOncePerStack">Whether or not to prevent double triggering.</param>
    /// <param name="canStack">If this ability should stack or not.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetCanStack(this AbilityInfo abilityInfo, bool canStack = true, bool triggersOncePerStack = false)
    {
        abilityInfo.canStack = canStack;
        abilityInfo.SetTriggersOncePerStack(triggersOncePerStack);
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability can stack on a card, triggering once for each stack.
    /// Optional parameter for setting the ability to only trigger once per stack when a card evolves (only affects abilities that can stack).
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="triggersOncePerStack">Whether or not to prevent double triggering.</param>
    /// <param name="canStack">If this ability should stack or not.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetCanStack(this FullAbility fullAbility, bool canStack = true, bool triggersOncePerStack = false) {
        fullAbility.Info.SetCanStack(canStack, triggersOncePerStack);
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability's icon should be flipped upside-down when it's on an opponent card.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="flipY">If the icon should be flipped.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetFlipYIfOpponent(this AbilityInfo abilityInfo, bool flipY = true)
    {
        abilityInfo.flipYIfOpponent = flipY;
        return abilityInfo;
    }
    /// <summary>
    /// Sets whether or not the ability's icon should be flipped upside-down when it's on an opponent card.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="flipY">If the icon should be flipped.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetFlipYIfOpponent(this FullAbility fullAbility, bool flipY = true) {
        fullAbility.Info.SetFlipYIfOpponent(flipY);
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability's icon's colour should be overridden, and what the override colour should be.
    /// The colour override only applies to the default ability icons; totem and merge icons are unaffected.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="hasOverride">If the ability icon's colour should be overridden.</param>
    /// <param name="colorOverride">The colour that will override the icon. Only applies if hasOverride is true.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetHasColorOverride(this AbilityInfo abilityInfo, bool hasOverride, Color colorOverride = default)
    {
        abilityInfo.hasColorOverride = hasOverride;
        if (hasOverride)
            abilityInfo.colorOverride = colorOverride;

        return abilityInfo;
    }

    /// <summary>
    /// Sets whether or not the ability's icon's colour should be overridden, and what the override colour should be.
    /// The colour override only applies to the default ability icons; totem and merge icons are unaffected.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="hasOverride">If the ability icon's colour should be overridden.</param>
    /// <param name="colorOverride">The colour that will override the icon. Only applies if hasOverride is true.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetHasColorOverride(this FullAbility fullAbility, bool hasOverride, Color colorOverride = default) {
        fullAbility.Info.SetHasColorOverride(hasOverride, colorOverride);
        return fullAbility;
    }

    /// <summary>
    /// Sets whether or not the ability's name should precede its description in Act 2.
    /// If false, only the ability's description will be shown.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="keyword">If the ability's name should precede its Act 2 description.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetKeywordAbility(this AbilityInfo abilityInfo, bool keyword = true)
    {
        abilityInfo.keywordAbility = keyword;
        return abilityInfo;
    }

    /// <summary>
    /// Sets whether or not the ability's name should precede its description in Act 2.
    /// If false, only the ability's description will be shown.
    /// </summary>
    /// <param name="fullAbility">The instance of <see cref="FullAbility"/>.</param>
    /// <param name="keyword">If the ability's name should precede its Act 2 description.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetKeywordAbility(this FullAbility fullAbility, bool keyword = true) {
        fullAbility.Info.keywordAbility = keyword;
        return fullAbility;
    }

    /// <summary>
    /// Resets the AbilityInfo's rulebook description to its vanilla version.
    /// Useful for anyone messing with altering descriptions.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo ResetDescription(this AbilityInfo abilityInfo)
    {
        abilityInfo.rulebookDescription = AllAbilities.AbilityByID(abilityInfo.ability).BaseRulebookDescription;
        return abilityInfo;
    }

    /// <summary>
    /// Marks the given ability as appearing in the rulebook during Act 1.
    /// </summary>
    /// <param name="full">The <see cref="FullAbility"/> to access.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetPart1Rulebook(this FullAbility full) {
        full.Info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
        return full;
    }
    /// <summary>
    /// Marks the given ability as appearing in the rulebook during Act 3.
    /// </summary>
    /// <param name="full">The <see cref="FullAbility"/> to access.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetPart3Rulebook(this FullAbility full) {
        full.Info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
        return full;
    }
    /// <summary>
    /// Marks the given ability as appearing in the rulebook during Grimora's act.
    /// </summary>
    /// <param name="full">The <see cref="FullAbility"/> to access.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetGrimoraRulebook(this FullAbility full) {
        full.Info.AddMetaCategories(AbilityMetaCategory.GrimoraRulebook);
        return full;
    }
    /// <summary>
    /// Marks the given ability as appearing in the rulebook during Magnificus's act.
    /// </summary>
    /// <param name="full">The <see cref="FullAbility"/> to access.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetMagnificusRulebook(this FullAbility full) {
        full.Info.AddMetaCategories(AbilityMetaCategory.MagnificusRulebook);
        return full;
    }

    /// <summary>
    /// Checks if the given <see cref="AbilityInfo"/> has all given metacategories.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="categories">The metacategories to check for.</param>
    /// <returns>True if this ability has all the given metacategory.</returns>
    public static bool HasMetaCategories(this AbilityInfo info, params AbilityMetaCategory[] categories)
    {
        foreach (AbilityMetaCategory app in categories)
            if (!info.HasMetaCategory(app))
                return false;

        return true;
    }

    /// <summary>
    /// Checks if the given <see cref="AbilityInfo"/> has the given metacategory.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="category">The metacategory to check for.</param>
    /// <returns>True if this ability has the given metacategory.</returns>
    public static bool HasMetaCategory(this AbilityInfo info, AbilityMetaCategory category)
    {
        return info.metaCategories.Contains(category);
    }

    #region Stat Icons
    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/>.</param>
    /// <param name="icon">A 49x49 texture containing the icon.</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetIcon(this StatIconInfo info, Texture2D icon, FilterMode? filterMode = null) {
        info.iconGraphic = icon;
        if (filterMode.HasValue)
            info.iconGraphic.filterMode = filterMode.Value;
        return info;
    }

    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/>.</param>
    /// <param name="pathToArt">The path to a 49x49 texture containing the icon on disk.</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetIcon(this StatIconInfo info, string pathToArt, FilterMode? filterMode = null) {
        info.iconGraphic = TextureHelper.GetImageAsTexture(pathToArt);
        if (filterMode.HasValue)
            info.iconGraphic.filterMode = filterMode.Value;
        return info;
    }

    /// <summary>
    /// Set the stat icon's rulebook name and description. Does not make the stat icon appear in the Rulebook.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/></param>
    /// <param name="rulebookName">The stat icon's rulebook name.</param>
    /// <param name="rulebookDescription">The stat icon's rulebook description.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetRulebookInfo(this StatIconInfo info, string rulebookName, string rulebookDescription = null) {
        info.rulebookName = rulebookName;
        info.rulebookDescription = rulebookDescription;
        return info;
    }
    /// <summary>
    /// Sets the stat icon's <c>appliesToAttack</c> and <c>appliesToHealth</c> fields. Note these fields don't make the stat icon affect the stat; you still need to implement that logic.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/></param>
    /// <param name="appliesToAttack">If the stat icon should cover a card's Power.</param>
    /// <param name="appliesToHealth">If the stat icon should cover a card's Health.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetAppliesToStats(this StatIconInfo info, bool appliesToAttack, bool appliesToHealth) {
        info.appliesToAttack = appliesToAttack;
        info.appliesToHealth = appliesToHealth;
        return info;
    }

    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand in Act 2.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/>.</param>
    /// <param name="icon">A 16x8 texture containing the icon.</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetPixelIcon(this StatIconInfo info, Texture2D icon, FilterMode? filterMode = null) {
        info.pixelIconGraphic = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelStatIcon, filterMode ?? FilterMode.Point);
        return info;
    }
    /// <summary>
    /// Sets the icon that will be displayed for this stat icon when the card is in the player's hand in Act 2.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/>.</param>
    /// <param name="pathToArt">The file path to the 16x8 texture file containing the icon.</param>
    /// <param name="filterMode">The filter mode for the icon texture. Leave this at its default value unless you have a specific reason to change it.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetPixelIcon(this StatIconInfo info, string pathToArt, FilterMode? filterMode = null) {
        Texture2D tex = TextureHelper.GetImageAsTexture(pathToArt, filterMode ?? FilterMode.Point);
        info.pixelIconGraphic = TextureHelper.ConvertTexture(tex, TextureHelper.SpriteType.PixelStatIcon);
        return info;
    }

    /// <summary>
    /// Adds one or more metacategories to the stati icon. Duplicate categories will not be added.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/>.</param>
    /// <param name="categories">The metacategories to add.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo AddMetaCategories(this StatIconInfo info, params AbilityMetaCategory[] categories) {
        info.metaCategories ??= new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    /// <summary>
    /// Helper method: automatically adds the <c>Part1Rulebook</c> metacategory to the stat icon.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/>.</param>
    /// <returns>The same stati icon so a chain can continue.</returns>
    public static StatIconInfo SetDefaultPart1Ability(this StatIconInfo info) {
        return info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
    }

    /// <summary>
    /// Helper method: automatically adds the <c>Part3Rulebook</c> metacategory to the stat icon.
    /// </summary>
    /// <param name="info">The instance of <see cref="StatIconInfo"/>.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetDefaultPart3Ability(this StatIconInfo info) {
        return info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
    }
    #endregion

    #region TriggersOncePerStack
    /// <summary>
    /// Sets the ability to only ever trigger once per stack. This prevents abilities from triggering twice per stack after a card evolves.
    /// This only affects cards that evolve into a card that possesses the same stackable ability (eg, default evolutions).
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="triggersOncePerStack">Whether or not to prevent double triggering.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetTriggersOncePerStack(this AbilityInfo abilityInfo, bool triggersOncePerStack = true)
    {
        abilityInfo.SetExtendedProperty("TriggersOncePerStack", triggersOncePerStack);
        return abilityInfo;
    }

    /// <summary>
    /// Gets the value of <c>TriggersOncePerStack</c>. Returns false if <c>TriggersOncePerStack</c> has not been set.
    /// </summary>
    /// <param name="ability">Ability to access.</param>
    /// <returns>True if double triggering is enabled. False otherwise.</returns>
    public static bool GetTriggersOncePerStack(this Ability ability)
    {
        AbilityInfo abilityInfo = AllAbilityInfos.AbilityByID(ability);
        return abilityInfo.GetTriggersOncePerStack();
    }
    /// <summary>
    /// Gets the value of <c>TriggersOncePerStack</c>. Returns false if <c>TriggersOncePerStack</c> has not been set.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <returns>True if double triggering is enabled. False otherwise.</returns>
    public static bool GetTriggersOncePerStack(this AbilityInfo abilityInfo)
    {
        return abilityInfo.GetExtendedPropertyAsBool("TriggersOncePerStack") ?? false;
    }
    #endregion

    #region HideSingleStacks
    /// <summary>
    /// Sets an ability to only have one stack of it be hidden whenever the ability is added to a card status's hiddenAbilities field.
    /// Adding the same ability to hiddenAbilities will hide more stacks. Only affects cards that can stack.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <param name="hideSingleStacks">Whether all stacks of this ability will be hidden when added to hiddenAbilities.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetHideSingleStacks(this AbilityInfo abilityInfo, bool hideSingleStacks = true)
    {
        abilityInfo.SetExtendedProperty("HideSingleStacks", hideSingleStacks);
        return abilityInfo;
    }

    /// <summary>
    /// Gets the value of <c>HideSingleStacks</c>. Returns false if <c>HideSingleStacks</c> has not been set.
    /// </summary>
    /// <param name="ability">Ability to access.</param>
    /// <returns>Whether single stacks of this ability will be hidden when added to <c>hiddenAbilities</c>.</returns>
    public static bool GetHideSingleStacks(this Ability ability)
    {
        return AllAbilityInfos.AbilityByID(ability).GetHideSingleStacks();
    }
    /// <summary>
    /// Gets the value of <c>HideSingleStacks</c>. Returns false if <c>HideSingleStacks</c> has not been set.
    /// </summary>
    /// <param name="abilityInfo">The instance of <see cref="AbilityInfo"/>.</param>
    /// <returns>Whether single stacks of this ability will be hidden when added to <c>hiddenAbilities</c>.</returns>
    public static bool GetHideSingleStacks(this AbilityInfo abilityInfo)
    {
        return abilityInfo.GetExtendedPropertyAsBool("HideSingleStacks") ?? false;
    }
    #endregion

    #region ExtendedProperties

    /// <summary>
    /// Adds a custom property value to the <see cref="AbilityInfo"/>.
    /// </summary>
    /// <param name="info"><see cref="AbilityInfo"/> to access.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetExtendedProperty(this AbilityInfo info, string propertyName, object value)
    {
        info.GetAbilityExtensionTable()[propertyName] = value?.ToString();
        return info;
    }
    /// <summary>
    /// Adds a custom property value to the <see cref="FullAbility"/>'s info field - intended as a shorthand for when modders are first adding abilities to the game.
    /// </summary>
    /// <param name="fullAbility"><see cref="FullAbility"/> object to access.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same <see cref="FullAbility"/> so a chain can continue.</returns>
    public static FullAbility SetExtendedProperty(this FullAbility fullAbility, string propertyName, object value)
    {
        fullAbility.Info.GetAbilityExtensionTable()[propertyName] = value?.ToString();
        return fullAbility;
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
    /// <param name="info"><see cref="AbilityInfo"/> to access.</param>
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
    /// <param name="info"><see cref="AbilityInfo"/> to access.</param>
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
    /// <param name="info"><see cref="AbilityInfo"/> to access.</param>
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
    /// <param name="info"><see cref="AbilityInfo"/> to access.</param>
    /// <param name="propertyName">Property name to get value of.</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean.</returns>
    public static bool? GetExtendedPropertyAsBool(this AbilityInfo info, string propertyName)
    {
        info.GetAbilityExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }

    #endregion
}
