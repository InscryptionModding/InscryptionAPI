using DiskCardGame;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using InscryptionAPI.Items;
using InscryptionAPI.Slots;
using UnityEngine;
using static InscryptionAPI.RuleBook.RuleBookManager;

namespace InscryptionAPI.RuleBook;

public static class RuleBookRedirectExtensions
{
    #region AbilityInfo
    public static AbilityManager.FullAbility SetAbilityRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullAbility;
    }
    public static AbilityManager.FullAbility SetStatIconRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullAbility;
    }
    public static AbilityManager.FullAbility SetBoonRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullAbility;
    }
    public static AbilityManager.FullAbility SetItemRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullAbility;
    }
    public static AbilityManager.FullAbility SetUniqueRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullAbility;
    }
    public static AbilityManager.FullAbility SetSlotRedirect(this AbilityManager.FullAbility fullAbility, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullAbility.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullAbility;
    }

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
    public static StatIconManager.FullStatIcon SetAbilityRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullIcon;
    }
    public static StatIconManager.FullStatIcon SetStatIconRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullIcon;
    }
    public static StatIconManager.FullStatIcon SetBoonRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullIcon;
    }
    public static StatIconManager.FullStatIcon SetItemRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullIcon;
    }
    public static StatIconManager.FullStatIcon SetUniqueRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullIcon;
    }
    public static StatIconManager.FullStatIcon SetSlotRedirect(this StatIconManager.FullStatIcon fullIcon, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullIcon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullIcon;
    }

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
    public static BoonManager.FullBoon SetAbilityRedirect(this BoonManager.FullBoon fullBoon, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullBoon;
    }
    public static BoonManager.FullBoon SetStatIconRedirect(this BoonManager.FullBoon fullBoon, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullBoon;
    }
    public static BoonManager.FullBoon SetBoonRedirect(this BoonManager.FullBoon fullBoon, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullBoon;
    }
    public static BoonManager.FullBoon SetItemRedirect(this BoonManager.FullBoon fullBoon, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullBoon;
    }
    public static BoonManager.FullBoon SetUniqueRedirect(this BoonManager.FullBoon fullBoon, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullBoon;
    }
    public static BoonManager.FullBoon SetSlotRedirect(this BoonManager.FullBoon fullBoon, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullBoon.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullBoon;
    }

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
    public static ConsumableItemManager.FullConsumableItemData SetAbilityRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return fullItem;
    }
    public static ConsumableItemManager.FullConsumableItemData SetStatIconRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return fullItem;
    }
    public static ConsumableItemManager.FullConsumableItemData SetBoonRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return fullItem;
    }
    public static ConsumableItemManager.FullConsumableItemData SetItemRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return fullItem;
    }
    public static ConsumableItemManager.FullConsumableItemData SetUniqueRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return fullItem;
    }
    public static ConsumableItemManager.FullConsumableItemData SetSlotRedirect(this ConsumableItemManager.FullConsumableItemData fullItem, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        fullItem.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return fullItem;
    }

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
    public static SlotModificationManager.Info SetAbilityRedirect(this SlotModificationManager.Info slotInfo, string clickableText, Ability abilityRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Abilities, redirectColour, abilityRedirect.ToString());
        return slotInfo;
    }
    public static SlotModificationManager.Info SetStatIconRedirect(this SlotModificationManager.Info slotInfo, string clickableText, SpecialStatIcon statIconRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.StatIcons, redirectColour, statIconRedirect.ToString());
        return slotInfo;
    }
    public static SlotModificationManager.Info SetBoonRedirect(this SlotModificationManager.Info slotInfo, string clickableText, BoonData.Type boonRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Boons, redirectColour, boonRedirect.ToString());
        return slotInfo;
    }
    public static SlotModificationManager.Info SetItemRedirect(this SlotModificationManager.Info slotInfo, string clickableText, string itemNameRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, itemNameRedirect);
        return slotInfo;
    }
    public static SlotModificationManager.Info SetUniqueRedirect(this SlotModificationManager.Info slotInfo, string clickableText, string uniqueRedirect, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Unique, redirectColour, uniqueRedirect);
        return slotInfo;
    }
    public static SlotModificationManager.Info SetSlotRedirect(this SlotModificationManager.Info slotInfo, string clickableText, SlotModificationManager.ModificationType slotMod, Color redirectColour)
    {
        slotInfo.RulebookDescriptionRedirects[clickableText] = new(PageRangeType.Items, redirectColour, SlotModificationManager.SLOT_PAGEID + slotMod.ToString());
        return slotInfo;
    }

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
