using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionAPI.Card;

public static class CardExtensions
{
    public static CardInfo CardByName(this IEnumerable<CardInfo> cards, string name) => cards.FirstOrDefault(x => x.name == name);

    private static Sprite GetPortrait(Texture2D portrait, TextureHelper.SpriteType spriteType, FilterMode? filterMode)
    {
        if (!filterMode.HasValue)
            return TextureHelper.ConvertTexture(portrait, spriteType);
        else
            return TextureHelper.ConvertTexture(portrait, spriteType, filterMode.Value);
    }

    public static CardInfo SetPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        info.portraitTex = GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode);

        if (!string.IsNullOrEmpty(info.name))
            info.portraitTex.name = info.name + "_portrait";

        return info;
    }

    public static CardInfo SetEmissivePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        if (info.portraitTex == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting normal portrait");


        info.portraitTex.RegisterEmissionForSprite(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));

        return info;
    }

    public static CardInfo SetEmissivePortrait(this CardInfo info, string pathToArt)
    {
        return info.SetEmissivePortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, Texture2D emission, FilterMode? filterMode = null)
    {
        info.SetPortrait(portrait, filterMode);
        info.SetEmissivePortrait(emission, filterMode);
        return info;
    }

    public static CardInfo SetPortrait(this CardInfo info, string pathToArt, string pathToEmission)
    {
        info.SetPortrait(pathToArt);
        info.SetEmissivePortrait(pathToEmission);
        return info;
    }

    public static CardInfo SetAltPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetAltPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    public static CardInfo SetAltPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        info.alternatePortrait = GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode);
        if (!string.IsNullOrEmpty(info.name))
            info.alternatePortrait.name = info.name + "_altportrait";

        TextureHelper.TryReuseEmission(info, info.alternatePortrait);

        return info;
    }

    public static CardInfo SetPixelPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    public static CardInfo SetPixelPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        if (!filterMode.HasValue)
            info.pixelPortrait = TextureHelper.ConvertTexture(portrait, TextureHelper.SpriteType.PixelPortrait);
        else
            info.pixelPortrait = TextureHelper.ConvertTexture(portrait, TextureHelper.SpriteType.PixelPortrait, filterMode.Value);

        if (!string.IsNullOrEmpty(info.name))
            info.pixelPortrait.name = info.name + "_pixelportrait";

        return info;
    }

    public static TailParams SetLostTailPortrait(this TailParams info, string pathToArt, CardInfo owner)
    {
        return info.SetLostTailPortrait(TextureHelper.GetImageAsTexture(pathToArt), owner);
    }

    public static TailParams SetLostTailPortrait(this TailParams info, Texture2D portrait, CardInfo owner, FilterMode? filterMode = null)
    {
        if (!filterMode.HasValue)
            info.tailLostPortrait = TextureHelper.ConvertTexture(portrait, TextureHelper.SpriteType.CardPortrait);
        else
            info.tailLostPortrait = TextureHelper.ConvertTexture(portrait, TextureHelper.SpriteType.CardPortrait, filterMode.Value);

        if (!string.IsNullOrEmpty(owner.name))
            info.tailLostPortrait.name = owner.name + "_pixelportrait";

        TextureHelper.TryReuseEmission(owner, info.tailLostPortrait);

        return info;
    }

    /// <summary>
    /// Sets the card so it shows up for normal card choices in Act 1.
    /// </summary>
    public static CardInfo SetDefaultPart1Card(this CardInfo info)
    {
        if (!info.metaCategories.Contains(CardMetaCategory.Rare))
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer);
            info.cardComplexity = CardComplexity.Simple;
        }
        info.temple = CardTemple.Nature;
        return info;
    }

    /// <summary>
    /// Sets the card so it shows up for normal card choices in Act 3.
    /// </summary>
    public static CardInfo SetDefaultPart3Card(this CardInfo info)
    {
        if (!info.metaCategories.Contains(CardMetaCategory.Rare))
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode, CardMetaCategory.Part3Random);
            info.cardComplexity = CardComplexity.Simple;
        }
        info.temple = CardTemple.Tech;
        return info;
    }

    /// <summary>
    /// Sets the card so it shows up for rare card choices and applies the rare background.
    /// </summary>
    public static CardInfo SetRare(this CardInfo info)
    {
        info.AddMetaCategories(CardMetaCategory.Rare);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.RareCardColors, CardAppearanceBehaviour.Appearance.RareCardBackground);

        info.metaCategories.Remove(CardMetaCategory.ChoiceNode);
        info.metaCategories.Remove(CardMetaCategory.TraderOffer);
        info.metaCategories.Remove(CardMetaCategory.Part3Random);

        info.cardComplexity = CardComplexity.Intermediate;

        return info;
    }

    /// <summary>
    /// Adds the terrain trait and background to this card.
    /// </summary>
    public static CardInfo SetTerrain(this CardInfo info)
    {
        info.AddTraits(Trait.Terrain);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);
        return info;
    }

    public static CardInfo SetBasic(this CardInfo info, string displayedName, int attack, int health, string description = default(string))
    {
        info.displayedName = displayedName;
        info.baseAttack = attack;
        info.baseHealth = health;
        info.description = description;    
        return info;
    }

    public static CardInfo SetTail(this CardInfo info, string tailName)
    {
        info.tailParams = new();
        info.tailParams.tail = CardManager.AllCardsCopy.CardByName(tailName);
        info.tailParams.tailLostPortrait = info.portraitTex;
        return info;
    }

    public static CardInfo SetTail(this CardInfo info, string tailName, string pathToLostTailArt)
    {
        info.tailParams = new();
        info.tailParams.tail = CardManager.AllCardsCopy.CardByName(tailName);
        info.tailParams.SetLostTailPortrait(pathToLostTailArt, info);
        return info;
    }

    public static CardInfo SetTail(this CardInfo info, string tailName, Texture2D tailLostPortrait, FilterMode? filterMode = null)
    {
        info.tailParams = new();
        info.tailParams.tail = CardManager.AllCardsCopy.CardByName(tailName);
        info.tailParams.SetLostTailPortrait(tailLostPortrait, info, filterMode);
        return info;
    }

    public static CardInfo SetIceCube(this CardInfo info, string iceCubeName)
    {
        info.iceCubeParams = new();
        info.iceCubeParams.creatureWithin = CardManager.AllCardsCopy.CardByName(iceCubeName);
        return info;
    }

    public static CardInfo SetEvolve(this CardInfo info, string evolveInto, int numberOfTurns)
    {
        info.evolveParams = new();
        info.evolveParams.turnsToEvolve = numberOfTurns;
        info.evolveParams.evolution = CardManager.AllCardsCopy.CardByName(evolveInto);
        return info;
    }

    public static CardInfo SetCost(this CardInfo info, int bloodCost = 0, int bonesCost = 0, int energyCost = 0, List<GemType> gemsCost = null)
    {
        info.cost = bloodCost;
        info.bonesCost = bonesCost;
        info.energyCost = energyCost;
        info.gemsCost = gemsCost ?? new();
        return info;
    }

    public static CardInfo AddAbilities(this CardInfo info, params Ability[] abilities)
    {
        info.abilities = info.abilities ?? new();
        info.abilities.AddRange(abilities);
        return info;
    }

    public static CardInfo AddAppearances(this CardInfo info, params CardAppearanceBehaviour.Appearance[] appearances)
    {
        info.appearanceBehaviour = info.appearanceBehaviour ?? new();
        foreach (var app in appearances)
            if (!info.appearanceBehaviour.Contains(app))
                info.appearanceBehaviour.Add(app);
        return info;
    }

    public static CardInfo AddMetaCategories(this CardInfo info, params CardMetaCategory[] categories)
    {
        info.metaCategories = info.metaCategories ?? new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    public static CardInfo AddTraits(this CardInfo info, params Trait[] traits)
    {
        info.traits = info.traits ?? new();
        foreach (var app in traits)
            if (!info.traits.Contains(app))
                info.traits.Add(app);
        return info;
    }

    public static CardInfo AddTribes(this CardInfo info, params Tribe[] tribes)
    {
        info.tribes = info.tribes ?? new();
        foreach (var app in tribes)
            if (!info.tribes.Contains(app))
                info.tribes.Add(app);
        return info;
    }

    public static CardInfo AddSpecialAbilities(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        info.specialAbilities = info.specialAbilities ?? new();
        foreach (var app in abilities)
            if (!info.specialAbilities.Contains(app))
                info.specialAbilities.Add(app);
        return info;
    }

    /// <summary>
    /// Adds this card to Act 2 packs and collection.
    /// </summary>
    public static CardInfo SetGBCPlayable(this CardInfo info, CardTemple temple)
    {
        info.AddMetaCategories(CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable);
        info.temple = temple;
        return info;
    }
}
