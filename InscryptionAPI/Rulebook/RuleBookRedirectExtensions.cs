using DiskCardGame;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using InscryptionAPI.Items;
using InscryptionAPI.Slots;
using UnityEngine;
using static InscryptionAPI.RuleBook.RuleBookManager;

namespace InscryptionAPI.RuleBook;

/// <summary>
/// Utility class that adds extensions to aid in adding rulebook redirects (hyperlinks) to supported game objects.
/// </summary>
public static class RuleBookRedirectExtensions
{
    #region AbilityInfo
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of another ability.
    /// </summary>
    /// <param name="fullAbility">The <see cref="AbilityManager.FullAbility"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityManager.FullAbility"/> so a chain can continue.</returns>
    public static AbilityManager.FullAbility SetAbilityRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullAbility;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a stat icon.
    /// </summary>
    /// <param name="fullAbility">The <see cref="AbilityManager.FullAbility"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityManager.FullAbility"/> so a chain can continue.</returns>
    public static AbilityManager.FullAbility SetStatIconRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullAbility;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a boon.
    /// </summary>
    /// <param name="fullAbility">The <see cref="AbilityManager.FullAbility"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityManager.FullAbility"/> so a chain can continue.</returns>
    public static AbilityManager.FullAbility SetBoonRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullAbility;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of an item.
    /// </summary>
    /// <param name="fullAbility">The <see cref="AbilityManager.FullAbility"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityManager.FullAbility"/> so a chain can continue.</returns>
    public static AbilityManager.FullAbility SetItemRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullAbility;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a custom rulebook section.
    /// </summary>
    /// <param name="fullAbility">The <see cref="AbilityManager.FullAbility"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityManager.FullAbility"/> so a chain can continue.</returns>
    public static AbilityManager.FullAbility SetUniqueRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullAbility;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a slot modification.
    /// </summary>
    /// <param name="fullAbility">The <see cref="AbilityManager.FullAbility"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityManager.FullAbility"/> so a chain can continue.</returns>
    public static AbilityManager.FullAbility SetSlotRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullAbility;
    }

    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of another ability.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetAbilityRedirect(this AbilityInfo info, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        AbilityManager.FullAbility full = AbilityManager.BaseGameAbilities.Concat(AbilityManager.NewAbilities).FirstOrDefault(x => x.Info.ability == info.ability);
        if (full != null)
        {
            full.SetAbilityRedirect(clickableText, abilityRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the AbilityInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a stat icon.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetStatIconRedirect(this AbilityInfo info, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        AbilityManager.FullAbility full = AbilityManager.BaseGameAbilities.Concat(AbilityManager.NewAbilities).FirstOrDefault(x => x.Info.ability == info.ability);
        if (full != null)
        {
            full.SetStatIconRedirect(clickableText, statIconRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the AbilityInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a boon.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetBoonRedirect(this AbilityInfo info, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        AbilityManager.FullAbility full = AbilityManager.BaseGameAbilities.Concat(AbilityManager.NewAbilities).FirstOrDefault(x => x.Info.ability == info.ability);
        if (full != null)
        {
            full.SetBoonRedirect(clickableText, boonRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the AbilityInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of an item.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetItemRedirect(this AbilityInfo info, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        AbilityManager.FullAbility full = AbilityManager.BaseGameAbilities.Concat(AbilityManager.NewAbilities).FirstOrDefault(x => x.Info.ability == info.ability);
        if (full != null)
        {
            full.SetItemRedirect(clickableText, itemNameRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the AbilityInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a custom rulebook section.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetUniqueRedirect(this AbilityInfo info, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        AbilityManager.FullAbility full = AbilityManager.BaseGameAbilities.Concat(AbilityManager.NewAbilities).FirstOrDefault(x => x.Info.ability == info.ability);
        if (full != null)
        {
            full.SetUniqueRedirect(clickableText, uniqueRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the AbilityInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a slot modification.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static AbilityInfo SetSlotRedirect(this AbilityInfo info, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        AbilityManager.FullAbility full = AbilityManager.BaseGameAbilities.Concat(AbilityManager.NewAbilities).FirstOrDefault(x => x.Info.ability == info.ability);
        if (full != null)
        {
            full.SetSlotRedirect(clickableText, slotMod, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the AbilityInfo hasn't been added to the API yet.");
        return info;
    }
    #endregion

    #region StatIconInfo
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of an ability.
    /// </summary>
    /// <param name="fullIcon">The <see cref="StatIconManager.FullStatIcon"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconManager.FullStatIcon"/> so a chain can continue.</returns>
    public static StatIconManager.FullStatIcon SetAbilityRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullIcon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of another stat icon.
    /// </summary>
    /// <param name="fullIcon">The <see cref="StatIconManager.FullStatIcon"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static StatIconManager.FullStatIcon SetStatIconRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullIcon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of a boon.
    /// </summary>
    /// <param name="fullIcon">The <see cref="StatIconManager.FullStatIcon"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconManager.FullStatIcon"/> so a chain can continue.</returns>
    public static StatIconManager.FullStatIcon SetBoonRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullIcon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of an item.
    /// </summary>
    /// <param name="fullIcon">The <see cref="StatIconManager.FullStatIcon"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconManager.FullStatIcon"/> so a chain can continue.</returns>
    public static StatIconManager.FullStatIcon SetItemRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullIcon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of a custom rulebook section.
    /// </summary>
    /// <param name="fullIcon">The <see cref="StatIconManager.FullStatIcon"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconManager.FullStatIcon"/> so a chain can continue.</returns>
    public static StatIconManager.FullStatIcon SetUniqueRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullIcon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of a slot modification.
    /// </summary>
    /// <param name="fullIcon">The <see cref="StatIconManager.FullStatIcon"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconManager.FullStatIcon"/> so a chain can continue.</returns>
    public static StatIconManager.FullStatIcon SetSlotRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullIcon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of an ability.
    /// </summary>
    /// <param name="info">The <see cref="StatIconInfo"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetAbilityRedirect(this StatIconInfo info, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        StatIconManager.FullStatIcon full = StatIconManager.AllStatIcons.Find(x => x.Id == info.iconType);
        if (full != null)
        {
            full.SetAbilityRedirect(clickableText, abilityRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the StatIconInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given ability to the page of a stat icon.
    /// </summary>
    /// <param name="info">The <see cref="AbilityInfo"/> to access.</param>
    /// <param name="clickableText">The text in this ability's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="AbilityInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetStatIconRedirect(this StatIconInfo info, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        StatIconManager.FullStatIcon full = StatIconManager.AllStatIcons.Find(x => x.Id == info.iconType);
        if (full != null)
        {
            full.SetStatIconRedirect(clickableText, statIconRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the StatIconInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of a boon.
    /// </summary>
    /// <param name="info">The <see cref="StatIconInfo"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetBoonRedirect(this StatIconInfo info, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        StatIconManager.FullStatIcon full = StatIconManager.AllStatIcons.Find(x => x.Id == info.iconType);
        if (full != null)
        {
            full.SetBoonRedirect(clickableText, boonRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the StatIconInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of an item.
    /// </summary>
    /// <param name="info">The <see cref="StatIconInfo"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetItemRedirect(this StatIconInfo info, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        StatIconManager.FullStatIcon full = StatIconManager.AllStatIcons.Find(x => x.Id == info.iconType);
        if (full != null)
        {
            full.SetItemRedirect(clickableText, itemNameRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the StatIconInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of a custom rulebook section.
    /// </summary>
    /// <param name="info">The <see cref="StatIconInfo"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetUniqueRedirect(this StatIconInfo info, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        StatIconManager.FullStatIcon full = StatIconManager.AllStatIcons.Find(x => x.Id == info.iconType);
        if (full != null)
        {
            full.SetUniqueRedirect(clickableText, uniqueRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the StatIconInfo hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given stat icon to the page of a slot modification.
    /// </summary>
    /// <param name="info">The <see cref="StatIconInfo"/> to access.</param>
    /// <param name="clickableText">The text in this stat icon's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="StatIconInfo"/> so a chain can continue.</returns>
    public static StatIconInfo SetSlotRedirect(this StatIconInfo info, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        StatIconManager.FullStatIcon full = StatIconManager.AllStatIcons.Find(x => x.Id == info.iconType);
        if (full != null)
        {
            full.SetSlotRedirect(clickableText, slotMod, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the StatIconInfo hasn't been added to the API yet.");
        return info;
    }
    #endregion

    #region BoonData
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of an ability.
    /// </summary>
    /// <param name="fullBoon">The <see cref="BoonManager.FullBoon"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonManager.FullBoon"/> so a chain can continue.</returns>
    public static BoonManager.FullBoon SetAbilityRedirect(this BoonManager.FullBoon fullBoon, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullBoon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of a stat icon.
    /// </summary>
    /// <param name="fullBoon">The <see cref="BoonManager.FullBoon"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonManager.FullBoon"/> so a chain can continue.</returns>
    public static BoonManager.FullBoon SetStatIconRedirect(this BoonManager.FullBoon fullBoon, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullBoon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of another boon.
    /// </summary>
    /// <param name="fullBoon">The <see cref="BoonManager.FullBoon"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonManager.FullBoon"/> so a chain can continue.</returns>
    public static BoonManager.FullBoon SetBoonRedirect(this BoonManager.FullBoon fullBoon, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullBoon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of an item.
    /// </summary>
    /// <param name="fullBoon">The <see cref="BoonManager.FullBoon"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonManager.FullBoon"/> so a chain can continue.</returns>
    public static BoonManager.FullBoon SetItemRedirect(this BoonManager.FullBoon fullBoon, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullBoon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of a custom rulebook section.
    /// </summary>
    /// <param name="fullBoon">The <see cref="BoonManager.FullBoon"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonManager.FullBoon"/> so a chain can continue.</returns>
    public static BoonManager.FullBoon SetUniqueRedirect(this BoonManager.FullBoon fullBoon, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullBoon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of a slot modification.
    /// </summary>
    /// <param name="fullBoon">The <see cref="BoonManager.FullBoon"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonManager.FullBoon"/> so a chain can continue.</returns>
    public static BoonManager.FullBoon SetSlotRedirect(this BoonManager.FullBoon fullBoon, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullBoon;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of an ability.
    /// </summary>
    /// <param name="info">The <see cref="BoonData"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonData"/> so a chain can continue.</returns>
    public static BoonData SetAbilityRedirect(this BoonData info, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        BoonManager.FullBoon full = BoonManager.AllFullBoons.Find(x => x.boon.type == info.type);
        if (full != null)
        {
            full.SetAbilityRedirect(clickableText, abilityRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the BoonData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of a stat icon.
    /// </summary>
    /// <param name="info">The <see cref="BoonData"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonData"/> so a chain can continue.</returns>
    public static BoonData SetStatIconRedirect(this BoonData info, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        BoonManager.FullBoon full = BoonManager.AllFullBoons.Find(x => x.boon.type == info.type);
        if (full != null)
        {
            full.SetStatIconRedirect(clickableText, statIconRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the BoonData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of another boon.
    /// </summary>
    /// <param name="info">The <see cref="BoonData"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonData"/> so a chain can continue.</returns>
    public static BoonData SetBoonRedirect(this BoonData info, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        BoonManager.FullBoon full = BoonManager.AllFullBoons.Find(x => x.boon.type == info.type);
        if (full != null)
        {
            full.SetBoonRedirect(clickableText, boonRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the BoonData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of an item.
    /// </summary>
    /// <param name="info">The <see cref="BoonData"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonData"/> so a chain can continue.</returns>
    public static BoonData SetItemRedirect(this BoonData info, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        BoonManager.FullBoon full = BoonManager.AllFullBoons.Find(x => x.boon.type == info.type);
        if (full != null)
        {
            full.SetItemRedirect(clickableText, itemNameRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the BoonData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of a custom rulebook section.
    /// </summary>
    /// <param name="info">The <see cref="BoonData"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonData"/> so a chain can continue.</returns>
    public static BoonData SetUniqueRedirect(this BoonData info, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        BoonManager.FullBoon full = BoonManager.AllFullBoons.Find(x => x.boon.type == info.type);
        if (full != null)
        {
            full.SetUniqueRedirect(clickableText, uniqueRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the BoonData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given boon to the page of a slot modification.
    /// </summary>
    /// <param name="info">The <see cref="BoonData"/> to access.</param>
    /// <param name="clickableText">The text in this boon's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="BoonData"/> so a chain can continue.</returns>
    public static BoonData SetSlotRedirect(this BoonData info, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        BoonManager.FullBoon full = BoonManager.AllFullBoons.Find(x => x.boon.type == info.type);
        if (full != null)
        {
            full.SetSlotRedirect(clickableText, slotMod, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the BoonData hasn't been added to the API yet.");
        return info;
    }
    #endregion

    #region ConsumableItemData
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of an ability.
    /// </summary>
    /// <param name="fullItem">The <see cref="ConsumableItemManager.FullConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemManager.FullConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemManager.FullConsumableItemData SetAbilityRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullItem;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a stat icon.
    /// </summary>
    /// <param name="fullItem">The <see cref="ConsumableItemManager.FullConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemManager.FullConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemManager.FullConsumableItemData SetStatIconRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullItem;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a boon.
    /// </summary>
    /// <param name="fullItem">The <see cref="ConsumableItemManager.FullConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemManager.FullConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemManager.FullConsumableItemData SetBoonRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullItem;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of an item.
    /// </summary>
    /// <param name="fullItem">The <see cref="ConsumableItemManager.FullConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemManager.FullConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemManager.FullConsumableItemData SetItemRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullItem;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a custom rulebook section.
    /// </summary>
    /// <param name="fullItem">The <see cref="ConsumableItemManager.FullConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemManager.FullConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemManager.FullConsumableItemData SetUniqueRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullItem;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a slot modification.
    /// </summary>
    /// <param name="fullItem">The <see cref="ConsumableItemManager.FullConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemManager.FullConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemManager.FullConsumableItemData SetSlotRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullItem;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of an ability.
    /// </summary>
    /// <param name="info">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetAbilityRedirect(this ConsumableItemData info, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        ConsumableItemManager.FullConsumableItemData full = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData == info);
        if (full != null)
        {
            full.SetAbilityRedirect(clickableText, abilityRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the ConsumableItemData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a stat icon.
    /// </summary>
    /// <param name="info">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetStatIconRedirect(this ConsumableItemData info, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        ConsumableItemManager.FullConsumableItemData full = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData == info);
        if (full != null)
        {
            full.SetStatIconRedirect(clickableText, statIconRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the ConsumableItemData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a boon.
    /// </summary>
    /// <param name="info">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetBoonRedirect(this ConsumableItemData info, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        ConsumableItemManager.FullConsumableItemData full = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData == info);
        if (full != null)
        {
            full.SetBoonRedirect(clickableText, boonRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the ConsumableItemData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of an item.
    /// </summary>
    /// <param name="info">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetItemRedirect(this ConsumableItemData info, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        ConsumableItemManager.FullConsumableItemData full = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData == info);
        if (full != null)
        {
            full.SetItemRedirect(clickableText, itemNameRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the ConsumableItemData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a custom rulebook section.
    /// </summary>
    /// <param name="info">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetUniqueRedirect(this ConsumableItemData info, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        ConsumableItemManager.FullConsumableItemData full = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData == info);
        if (full != null)
        {
            full.SetUniqueRedirect(clickableText, uniqueRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the ConsumableItemData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given item to the page of a slot modification.
    /// </summary>
    /// <param name="info">The <see cref="ConsumableItemData"/> to access.</param>
    /// <param name="clickableText">The text in this item's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="ConsumableItemData"/> so a chain can continue.</returns>
    public static ConsumableItemData SetSlotRedirect(this ConsumableItemData info, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        ConsumableItemManager.FullConsumableItemData full = ConsumableItemManager.allFullItemDatas.Find(x => x.itemData == info);
        if (full != null)
        {
            full.SetSlotRedirect(clickableText, slotMod, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the ConsumableItemData hasn't been added to the API yet.");
        return info;
    }
    #endregion

    #region SlotMod
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of an ability.
    /// </summary>
    /// <param name="slotInfo">The <see cref="SlotModificationManager.Info"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.Info"/> so a chain can continue.</returns>
    public static SlotModificationManager.Info SetAbilityRedirect(this SlotModificationManager.Info slotInfo, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return slotInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification info to the page of a stat icon.
    /// </summary>
    /// <param name="slotInfo">The <see cref="SlotModificationManager.Info"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.Info"/> so a chain can continue.</returns>
    public static SlotModificationManager.Info SetStatIconRedirect(this SlotModificationManager.Info slotInfo, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return slotInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of a boon.
    /// </summary>
    /// <param name="slotInfo">The <see cref="SlotModificationManager.Info"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.Info"/> so a chain can continue.</returns>
    public static SlotModificationManager.Info SetBoonRedirect(this SlotModificationManager.Info slotInfo, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return slotInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of an item.
    /// </summary>
    /// <param name="slotInfo">The <see cref="SlotModificationManager.Info"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.Info"/> so a chain can continue.</returns>
    public static SlotModificationManager.Info SetItemRedirect(this SlotModificationManager.Info slotInfo, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return slotInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of a custom rulebook section.
    /// </summary>
    /// <param name="slotInfo">The <see cref="SlotModificationManager.Info"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.Info"/> so a chain can continue.</returns>
    public static SlotModificationManager.Info SetUniqueRedirect(this SlotModificationManager.Info slotInfo, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return slotInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of another slot modification.
    /// </summary>
    /// <param name="slotInfo">The <see cref="SlotModificationManager.Info"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.Info"/> so a chain can continue.</returns>
    public static SlotModificationManager.Info SetSlotRedirect(this SlotModificationManager.Info slotInfo, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return slotInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of an ability.
    /// </summary>
    /// <param name="info">The <see cref="SlotModificationManager.ModificationType"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.ModificationType"/> so a chain can continue.</returns>
    public static SlotModificationManager.ModificationType SetAbilityRedirect(this SlotModificationManager.ModificationType info, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        SlotModificationManager.Info full = SlotModificationManager.AllSlotModifications.InfoByID(info);
        if (full != null)
        {
            full.SetAbilityRedirect(clickableText, abilityRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the SlotModificationManager.ModificationType hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of a stat icon.
    /// </summary>
    /// <param name="info">The <see cref="SlotModificationManager.ModificationType"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.ModificationType"/> so a chain can continue.</returns>
    public static SlotModificationManager.ModificationType SetStatIconRedirect(this SlotModificationManager.ModificationType info, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        SlotModificationManager.Info full = SlotModificationManager.AllSlotModifications.InfoByID(info);
        if (full != null)
        {
            full.SetStatIconRedirect(clickableText, statIconRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the SlotModificationManager.ModificationType hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of a boon.
    /// </summary>
    /// <param name="info">The <see cref="SlotModificationManager.ModificationType"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.ModificationType"/> so a chain can continue.</returns>
    public static SlotModificationManager.ModificationType SetBoonRedirect(this SlotModificationManager.ModificationType info, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        SlotModificationManager.Info full = SlotModificationManager.AllSlotModifications.InfoByID(info);
        if (full != null)
        {
            full.SetBoonRedirect(clickableText, boonRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the ConsumableItemData hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of an item.
    /// </summary>
    /// <param name="info">The <see cref="SlotModificationManager.ModificationType"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.ModificationType"/> so a chain can continue.</returns>
    public static SlotModificationManager.ModificationType SetItemRedirect(this SlotModificationManager.ModificationType info, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        SlotModificationManager.Info full = SlotModificationManager.AllSlotModifications.InfoByID(info);
        if (full != null)
        {
            full.SetItemRedirect(clickableText, itemNameRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the SlotModificationManager.ModificationType hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of a custom rulebook section.
    /// </summary>
    /// <param name="info">The <see cref="SlotModificationManager.ModificationType"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.ModificationType"/> so a chain can continue.</returns>
    public static SlotModificationManager.ModificationType SetUniqueRedirect(this SlotModificationManager.ModificationType info, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        SlotModificationManager.Info full = SlotModificationManager.AllSlotModifications.InfoByID(info);
        if (full != null)
        {
            full.SetUniqueRedirect(clickableText, uniqueRedirect, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the SlotModificationManager.ModificationType hasn't been added to the API yet.");
        return info;
    }
    /// <summary>
    /// Creates a new hyperlink for the given slot modification to the page of another slot modification.
    /// </summary>
    /// <param name="info">The <see cref="SlotModificationManager.ModificationType"/> to access.</param>
    /// <param name="clickableText">The text in this slot modification's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="SlotModificationManager.ModificationType"/> so a chain can continue.</returns>
    public static SlotModificationManager.ModificationType SetSlotRedirect(this SlotModificationManager.ModificationType info, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        SlotModificationManager.Info full = SlotModificationManager.AllSlotModifications.InfoByID(info);
        if (full != null)
        {
            full.SetSlotRedirect(clickableText, slotMod, redirectColour);
            return info;
        }

        InscryptionAPIPlugin.Logger.LogError($"Can't set rulebook redirect for {clickableText} because the SlotModificationManager.ModificationType hasn't been added to the API yet.");
        return info;
    }
    #endregion

    #region RuleBookPageInfo
    /// <summary>
    /// Creates a new hyperlink for the given page info to the page of an ability.
    /// </summary>
    /// <param name="pageInfo">The <see cref="RuleBookPageInfo"/> to access.</param>
    /// <param name="clickableText">The text in this page info's rulebook description that should be made a hyperlink.</param>
    /// <param name="abilityRedirect">The ID of the ability this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="RuleBookPageInfo"/> so a chain can continue.</returns>
    public static RuleBookPageInfo SetAbilityRedirect(this RuleBookPageInfo pageInfo, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        RuleBookPageInfoExt ext = CustomRedirectPages.Find(x => x.parentPageInfo == pageInfo);
        if (ext == null)
        {
            ext = new(pageInfo, new());
            CustomRedirectPages.Add(ext);
        }
        ext.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return pageInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given page info to the page of a stat icon.
    /// </summary>
    /// <param name="pageInfo">The <see cref="RuleBookPageInfo"/> to access.</param>
    /// <param name="clickableText">The text in this page info's rulebook description that should be made a hyperlink.</param>
    /// <param name="statIconRedirect">The ID of the stat icon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="RuleBookPageInfo"/> so a chain can continue.</returns>
    public static RuleBookPageInfo SetStatIconRedirect(this RuleBookPageInfo pageInfo, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        RuleBookPageInfoExt ext = CustomRedirectPages.Find(x => x.parentPageInfo == pageInfo);
        if (ext == null)
        {
            ext = new(pageInfo, new());
            CustomRedirectPages.Add(ext);
        }
        ext.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return pageInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given page info to the page of a boon.
    /// </summary>
    /// <param name="pageInfo">The <see cref="RuleBookPageInfo"/> to access.</param>
    /// <param name="clickableText">The text in this page info's rulebook description that should be made a hyperlink.</param>
    /// <param name="boonRedirect">The ID of the boon this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="RuleBookPageInfo"/> so a chain can continue.</returns>
    public static RuleBookPageInfo SetBoonRedirect(this RuleBookPageInfo pageInfo, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        RuleBookPageInfoExt ext = CustomRedirectPages.Find(x => x.parentPageInfo == pageInfo);
        if (ext == null)
        {
            ext = new(pageInfo, new());
            CustomRedirectPages.Add(ext);
        }
        ext.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return pageInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given page info to the page of an item.
    /// </summary>
    /// <param name="pageInfo">The <see cref="RuleBookPageInfo"/> to access.</param>
    /// <param name="clickableText">The text in this page info's rulebook description that should be made a hyperlink.</param>
    /// <param name="itemNameRedirect">The name of the item this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="RuleBookPageInfo"/> so a chain can continue.</returns>
    public static RuleBookPageInfo SetItemRedirect(this RuleBookPageInfo pageInfo, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        RuleBookPageInfoExt ext = CustomRedirectPages.Find(x => x.parentPageInfo == pageInfo);
        if (ext == null)
        {
            ext = new(pageInfo, new());
            CustomRedirectPages.Add(ext);
        }
        ext.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return pageInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given page info to the page of another custom rulebook section.
    /// </summary>
    /// <param name="pageInfo">The <see cref="RuleBookPageInfo"/> to access.</param>
    /// <param name="clickableText">The text in this page info's rulebook description that should be made a hyperlink.</param>
    /// <param name="uniqueRedirect">The page ID of the custom page this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="RuleBookPageInfo"/> so a chain can continue.</returns>
    public static RuleBookPageInfo SetUniqueRedirect(this RuleBookPageInfo pageInfo, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        RuleBookPageInfoExt ext = CustomRedirectPages.Find(x => x.parentPageInfo == pageInfo);
        if (ext == null)
        {
            ext = new(pageInfo, new());
            CustomRedirectPages.Add(ext);
        }
        ext.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return pageInfo;
    }
    /// <summary>
    /// Creates a new hyperlink for the given page info to the page of a slot modification.
    /// </summary>
    /// <param name="pageInfo">The <see cref="RuleBookPageInfo"/> to access.</param>
    /// <param name="clickableText">The text in this page info's rulebook description that should be made a hyperlink.</param>
    /// <param name="slotMod">The ID of the slot modification this hyperlink should redirect to.</param>
    /// <param name="redirectColour">The colour that hyperlinked text should be recoloured to indicate it can be clicked.</param>
    /// <returns>The same <see cref="RuleBookPageInfo"/> so a chain can continue.</returns>
    public static RuleBookPageInfo SetSlotRedirect(this RuleBookPageInfo pageInfo, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        RuleBookPageInfoExt ext = CustomRedirectPages.Find(x => x.parentPageInfo == pageInfo);
        if (ext == null)
        {
            ext = new(pageInfo, new());
            CustomRedirectPages.Add(ext);
        }
        ext.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return pageInfo;
    }
    #endregion
}
