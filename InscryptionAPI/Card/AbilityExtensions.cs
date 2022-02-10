using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Card;

public static class AbilityExtensions
{
    public static AbilityInfo AbilityByID(this IEnumerable<AbilityInfo> abilities, Ability id) => abilities.FirstOrDefault(x => x.ability == id);

    public static AbilityManager.FullAbility AbilityByID(this IEnumerable<AbilityManager.FullAbility> abilities, Ability id) => abilities.FirstOrDefault(x => x.Id == id);

    public static AbilityInfo SetIcon(this AbilityInfo info, Texture2D icon)
    {
        AbilityManager.FullAbility ability = AbilityManager.AllAbilities.FirstOrDefault(fab => fab.Id == info.ability);
        if (ability == default(AbilityManager.FullAbility))
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via AbilityManager.Add");

        ability.SetIcon(icon);
        return info;
    }

    public static AbilityInfo SetCustomFlippedTexture(this AbilityInfo info, Texture2D icon)
    {
        AbilityManager.FullAbility ability = AbilityManager.AllAbilities.FirstOrDefault(fab => fab.Id == info.ability);
        if (ability == default(AbilityManager.FullAbility))
            throw new InvalidOperationException("Cannot set custom texture directly on AbilityInfo unless it has been added via AbilityManager.Add");

        ability.SetCustomFlippedTexture(icon);
        return info;        
    }

    public static void SetIcon(this AbilityManager.FullAbility info, Texture2D icon)
    {
        info.Texture = icon;
    }

    public static void SetCustomFlippedTexture(this AbilityManager.FullAbility info, Texture2D icon)
    {
        info.CustomFlippedTexture = icon;
        info.Info.customFlippedIcon = true;
    }

    public static StatIconInfo SetIcon(this StatIconInfo info, Texture2D icon, FilterMode? filterMode = null)
    {
        info.iconGraphic = icon;
        if (filterMode.HasValue)
            info.iconGraphic.filterMode = filterMode.Value;
        return info;
    }
    
    public static StatIconInfo SetIcon(this StatIconInfo info, string pathToArt, FilterMode? filterMode = null)
    {
        info.iconGraphic = TextureHelper.GetImageAsTexture(pathToArt);
        if (filterMode.HasValue)
            info.iconGraphic.filterMode = filterMode.Value;
        return info;
    }
    
    public static StatIconInfo SetPixelIcon(this StatIconInfo info, Texture2D icon, FilterMode? filterMode = null)
    {
        if (!filterMode.HasValue)
            info.pixelIconGraphic = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelStatIcon);
        else
            info.pixelIconGraphic = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelStatIcon, filterMode.Value);
        return info;
    }

    public static AbilityInfo SetPixelAbilityIcon(this AbilityInfo info, Texture2D icon, FilterMode? filterMode = null)
    {
        if (!filterMode.HasValue)
            info.pixelIcon = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelAbilityIcon);
        else
            info.pixelIcon = TextureHelper.ConvertTexture(icon, TextureHelper.SpriteType.PixelAbilityIcon, filterMode.Value);
        return info;
    }

    public static AbilityInfo AddMetaCategories(this AbilityInfo info, params AbilityMetaCategory[] categories)
    {
        info.metaCategories = info.metaCategories ?? new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

     public static StatIconInfo AddMetaCategories(this StatIconInfo info, params AbilityMetaCategory[] categories)
    {
        info.metaCategories = info.metaCategories ?? new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    public static AbilityInfo SetDefaultPart1Ability(this AbilityInfo info)
    {
        info.AddMetaCategories(AbilityMetaCategory.Part1Modular, AbilityMetaCategory.Part1Rulebook);
        return info;
    }

    public static StatIconInfo SetDefaultPart1Ability(this StatIconInfo info)
    {
        info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
        return info;
    }

    public static AbilityInfo SetDefaultPart3Ability(this AbilityInfo info)
    {
        info.AddMetaCategories(AbilityMetaCategory.Part3Modular, AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3BuildACard);
        return info;
    }

    public static StatIconInfo SetDefaultPart3Ability(this StatIconInfo info)
    {
        info.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
        return info;
    }
}
