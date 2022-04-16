using System.Collections;
using DiskCardGame;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;
using UnityEngine;

namespace InscryptionAPI.Card;

public static class CardExtensions
{
    /// <summary>
    /// Gets the first card matching the given name, or null if it does not exist
    /// </summary>
    /// <param name="cards">An enumeration of Inscryption cards</param>
    /// <param name="name">The name to search for (case sensitive).</param>
    /// <returns>The first matching card, or null if no match</returns>
    public static CardInfo CardByName(this IEnumerable<CardInfo> cards, string name) => cards.FirstOrDefault(c => c.name.Equals(name));

    private static Sprite GetPortrait(Texture2D portrait, TextureHelper.SpriteType spriteType, FilterMode? filterMode = null)
    {
        return !filterMode.HasValue
            ? portrait.ConvertTexture(spriteType)
            : portrait.ConvertTexture(spriteType, filterMode.Value);
    }


    #region Adders

    /// <summary>
    /// Adds any number of abilities to the the card. Abilities can be added multiple times.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="abilities">The abilities to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddAbilities(this CardInfo info, params Ability[] abilities)
    {
        info.abilities ??= new();
        info.abilities.AddRange(abilities);
        return info;
    }

    /// <summary>
    /// Adds any number of appearance behaviors to the the card. Duplicate appearance behaviors are ignored.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="appearances">The appearances to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddAppearances(this CardInfo info, params CardAppearanceBehaviour.Appearance[] appearances)
    {
        info.appearanceBehaviour ??= new();
        foreach (var app in appearances)
            if (!info.appearanceBehaviour.Contains(app))
                info.appearanceBehaviour.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of decals to the the card. Duplicate decals are ignored.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="decals">The decals to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddDecal(this CardInfo info, params Texture[] decals)
    {
        info.decals ??= new();
        foreach (var dec in decals)
            if (!info.decals.Contains(dec))
                info.decals.Add(dec);

        return info;
    }

    /// <summary>
    /// Adds any number of decals to the the card. Duplicate decals are ignored.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="decals">The paths to the .png files containing the decals (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddDecal(this CardInfo info, params string[] decals)
    {
        return decals == null
            ? info
            : info.AddDecal(decals.Select(d => TextureHelper.GetImageAsTexture(d)).ToArray());

    }

    /// <summary>
    /// Adds any number of metacategories to the the card. Duplicate metacategories are ignored.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="categories">The categories to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddMetaCategories(this CardInfo info, params CardMetaCategory[] categories)
    {
        info.metaCategories ??= new();
        foreach (var app in categories)
            if (!info.metaCategories.Contains(app))
                info.metaCategories.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of special abilities to the the card. Duplicate special abilities are ignored.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="abilities">The abilities to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddSpecialAbilities(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        info.specialAbilities ??= new();
        foreach (var app in abilities)
            if (!info.specialAbilities.Contains(app))
                info.specialAbilities.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of traits to the the card. Duplicate traits are ignored.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="traits">The traits to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddTraits(this CardInfo info, params Trait[] traits)
    {
        info.traits ??= new();
        foreach (var app in traits)
            if (!info.traits.Contains(app))
                info.traits.Add(app);
        return info;
    }

    /// <summary>
    /// Adds any number of tribes to the the card. Duplicate tribes are ignored.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tribes">The tribes to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo AddTribes(this CardInfo info, params Tribe[] tribes)
    {
        info.tribes ??= new();
        foreach (var app in tribes)
            if (!info.tribes.Contains(app))
                info.tribes.Add(app);
        return info;
    }

    #endregion

    #region Setters

    /// <summary>
    /// Sets a number of basic properties of the card
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="displayedName">Displayed name of the card</param>
    /// <param name="attack">Attack of the card</param>
    /// <param name="health">Health of the card</param>
    /// <param name="description">The description that plays when the card is seen for the first time.</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetBasic(this CardInfo info, string displayedName, int attack, int health, string description = default(string))
    {
        info.displayedName = displayedName;
        info.baseAttack = attack;
        info.baseHealth = health;
        info.description = description;
        return info;
    }

    /// <summary>
    /// Sets an indicator of whether this is a base game card or not.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="isBaseGameCard">Whether this is a base game card or not</param>
    /// <returns>The same card info so a chain can continue</returns>
    internal static CardInfo SetBaseGameCard(this CardInfo info, bool isBaseGameCard = true)
    {
        info.SetExtendedProperty("BaseGameCard", isBaseGameCard);
        return info;
    }

    /// <summary>
    /// Indicates if this is a base game card or not
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>True of this card came from the base game; false otherwise</returns>
    public static bool IsBaseGameCard(this CardInfo info)
    {
        bool? isBGC = info.GetExtendedPropertyAsBool("BaseGameCard");
        return isBGC.HasValue && isBGC.Value;
    }

    #region Fields

    /// <summary>
    /// Sets the base attack and health of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="baseAttack">The base attack for the card</param>
    /// <param name="baseHealth">The base health for the card</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetBaseAttackAndHealth(this CardInfo info, int? baseAttack = 0, int? baseHealth = 0)
    {
        if (baseAttack.HasValue)
            info.baseAttack = baseAttack.Value;

        if (baseHealth.HasValue)
            info.baseHealth = baseHealth.Value;

        return info;
    }

    /// <summary>
    /// Sets the displayed name of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="displayedName">The displayed name for the card</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetDisplayedName(this CardInfo info, string displayedName)
    {
        info.displayedName = displayedName.IsNullOrWhitespace() ? "" : displayedName;
        return info;
    }

    /// <summary>
    /// Sets the name of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="name">The name for the card</param>
    /// <param name="modPrefix">The string that will be prefixed to the card name if it doesn't already exist.</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetName(this CardInfo info, string name, string modPrefix = default(string))
    {
        info.name = !string.IsNullOrEmpty(modPrefix) && !name.StartsWith(modPrefix) ? $"{modPrefix}_{name}" : name;
        return info;
    }

    /// <summary>
    /// Sets the card name and displayed name of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="name">The name for the card</param>
    /// <param name="displayedName">The displayed name for the card</param>
    /// <param name="modPrefix">The string that will be prefixed to the card name if it doesn't already exist.</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetNames(this CardInfo info, string name, string displayedName, string modPrefix = default(string))
    {
        info.SetDisplayedName(displayedName);
        return info.SetName(name, modPrefix);
    }

    /// <summary>
    /// Sets any number of special abilities to the the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="abilities">The abilities to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetSpecialAbilities(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        info.specialAbilities = abilities?.ToList();
        return info;
    }

    /// <summary>
    /// Sets the stat icon to the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="statIcon">The stat icon to set</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetStatIcon(this CardInfo info, SpecialStatIcon statIcon)
    {
        info.specialStatIcon = statIcon;
        info.AddSpecialAbilities(StatIconManager.AllStatIcons.FirstOrDefault(sii => sii.Id == statIcon).AbilityId);
        return info;
    }

    /// <summary>
    /// Sets any number of traits to the the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="traits">The traits to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetTraits(this CardInfo info, params Trait[] traits)
    {
        info.traits = traits?.ToList();
        return info;
    }


    /// <summary>
    /// Set any number of tribes to the the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tribes">The tribes to add</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetTribes(this CardInfo info, params Tribe[] tribes)
    {
        info.tribes = tribes?.ToList();
        return info;
    }

    #region MetaCategories

    /// <summary>
    /// Sets the card to behave as a "normal" card in Part 1. The CardTemple is Nature and it will appear in choice nodes and trader nodes.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// Sets the card to behave as a "normal" card in Part 3. The CardTemple is Tech and it will appear in choice nodes and as a potential random card from GiftBot.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// Makes the card fully playable in GBC mode and able to appear in card packs.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="temple">The temple that the card will exist under</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetGBCPlayable(this CardInfo info, CardTemple temple)
    {
        info.AddMetaCategories(CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable);
        info.temple = temple;
        return info;
    }


    /// <summary>
    /// Sets the card so it shows up for rare card choices and applies the rare background.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetRare(this CardInfo info)
    {
        info.AddMetaCategories(CardMetaCategory.Rare);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.RareCardBackground);

        info.metaCategories.Remove(CardMetaCategory.ChoiceNode);
        info.metaCategories.Remove(CardMetaCategory.TraderOffer);
        info.metaCategories.Remove(CardMetaCategory.Part3Random);

        info.cardComplexity = CardComplexity.Intermediate;

        return info;
    }

    /// <summary>
    /// Adds the terrain trait and background to this card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetTerrain(this CardInfo info)
    {
        info.AddTraits(Trait.Terrain);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);
        return info;
    }

    #endregion

    #region EvolveIceCubeTail

    /// <summary>
    /// Sets the evolve parameters of the card. These parameters are used to make the Evolve ability function correctly.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="evolveCard">The card that will be generated after the set number of turns.</param>
    /// <param name="numberOfTurns">The number of turns before the card evolves</param>
    /// <param name="mods">A set of card mods to be applied to the evolved card</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEvolve(this CardInfo info, CardInfo evolveCard, int numberOfTurns, IEnumerable<CardModificationInfo> mods = null)
    {
        info.evolveParams = new()
        {
            evolution = evolveCard
        };

        if (mods != null && mods.Any())
        {
            info.evolveParams.evolution = CardLoader.Clone(info.evolveParams.evolution);
            (info.evolveParams.evolution.mods ??= new()).AddRange(mods);
        }

        info.evolveParams.turnsToEvolve = numberOfTurns;

        return info;
    }

    /// <summary>
    /// Sets the evolve parameters of the card. These parameters are used to make the Evolve ability function correctly.
    /// This function uses delayed loading to attach the evolution to the card, so if the evolve card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="evolveInto">The name of card that will be generated after the set number of turns.</param>
    /// <param name="numberOfTurns">The number of turns before the card evolves</param>
    /// <param name="mods">A set of card mods to be applied to the evolved card</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEvolve(this CardInfo info, string evolveInto, int numberOfTurns, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo evolution = CardManager.AllCardsCopy.CardByName(evolveInto);

        if (evolution == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                CardInfo target = cards.CardByName(info.name);
                CardInfo evolveIntoCard = cards.CardByName(evolveInto);

                if (target != null && evolveIntoCard == null && target.IsOldApiCard()) // Maybe this is due to poor naming conventions allowed by the old API.
                    evolveIntoCard = cards.CardByName($"{target.GetModPrefix()}_{evolveInto}");

                if (target != null && evolveIntoCard != null)
                    target.SetEvolve(evolveIntoCard, numberOfTurns, mods);

                return cards;
            };
        }
        else
        {
            info.SetEvolve(evolution, numberOfTurns, mods);
        }
        return info;
    }

    /// <summary>
    /// Sets the ice cube parameters of the card. These parameters are used to make the IceCube ability function correctly.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="iceCube">The card that will be generated when this card dies.</param>
    /// <param name="mods">A set of card mods to be applied to the ice cube contents</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetIceCube(this CardInfo info, CardInfo iceCube, IEnumerable<CardModificationInfo> mods = null)
    {
        info.iceCubeParams = new()
        {
            creatureWithin = iceCube
        };

        if (mods != null && mods.Any())
        {
            info.iceCubeParams.creatureWithin = CardLoader.Clone(info.iceCubeParams.creatureWithin);
            (info.iceCubeParams.creatureWithin.mods ??= new()).AddRange(mods);
        }

        return info;
    }

    /// <summary>
    /// Sets the ice cube parameters of the card. These parameters are used to make the IceCube ability function correctly.
    /// This function uses delayed loading to attach the ice cube to the card, so if the ice cube card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="iceCubeName">The name of the card that will be generated when this card dies.</param>
    /// <param name="mods">A set of card mods to be applied to the ice cube contents</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetIceCube(this CardInfo info, string iceCubeName, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo creatureWithin = CardManager.AllCardsCopy.CardByName(iceCubeName);

        if (creatureWithin == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                CardInfo target = cards.CardByName(info.name);
                CardInfo creatureWithinCard = cards.CardByName(iceCubeName);

                if (target != null && creatureWithinCard == null && target.IsOldApiCard()) // Maybe this is due to poor naming conventions allowed by the old API.
                    creatureWithinCard = cards.CardByName($"{target.GetModPrefix()}_{iceCubeName}");

                if (target != null && creatureWithinCard != null)
                    target.SetIceCube(creatureWithinCard, mods);

                return cards;
            };
        }
        else
        {
            info.SetIceCube(creatureWithin, mods);
        }

        return info;
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="pathToLostTailArt">The path to the .png file containing the lost tail artwork (relative to the Plugins directory)</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, string pathToLostTailArt = null, IEnumerable<CardModificationInfo> mods = null)
    {
        Texture2D lostTailPortrait = pathToLostTailArt == null ? null : TextureHelper.GetImageAsTexture(pathToLostTailArt);
        return info.SetTail(tailName, lostTailPortrait, mods: mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetTail(this CardInfo info, CardInfo tail, Texture2D tailLostPortrait, FilterMode? filterMode = null, IEnumerable<CardModificationInfo> mods = null)
    {
        var tailLostSprite = !filterMode.HasValue
                                 ? tailLostPortrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
                                 : tailLostPortrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);
        return info.SetTail(tail, tailLostSprite, mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The sprite containing the card portrait</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetTail(this CardInfo info, CardInfo tail, Sprite tailLostPortrait, IEnumerable<CardModificationInfo> mods = null)
    {
        info.tailParams = new()
        {
            tail = tail
        };

        if (mods != null && mods.Any())
        {
            info.tailParams.tail = CardLoader.Clone(info.tailParams.tail);
            (info.tailParams.tail.mods ??= new()).AddRange(mods);
        }

        if (tailLostPortrait != null)
            info.tailParams.SetLostTailPortrait(tailLostPortrait, info);

        return info;
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <param name="mods">A set of card mods to be applied to the tail</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, Texture2D tailLostPortrait, FilterMode? filterMode = null, IEnumerable<CardModificationInfo> mods = null)
    {
        var tailLostSprite = !filterMode.HasValue
            ? tailLostPortrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
            : tailLostPortrait.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);
        return info.SetTail(tailName, tailLostSprite, mods);
    }
    
    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The sprite containing the card portrait.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, Sprite tailLostPortrait, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo tail = CardManager.AllCardsCopy.CardByName(tailName);

        if (tail == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                CardInfo target = cards.CardByName(info.name);
                CardInfo tailCard = cards.CardByName(tailName);

                if (target != null && tailCard == null && target.IsOldApiCard()) // Maybe this is due to poor naming conventions allowed by the old API.
                    tailCard = cards.CardByName($"{target.GetModPrefix()}_{tailName}");

                if (target != null && tailCard != null)
                    target.SetTail(tailCard, tailLostPortrait, mods);


                return cards;
            };
        }
        else
        {
            info.SetTail(tail, tailLostPortrait, mods);
        }

        return info;
    }

    #endregion

    #region CardCosts

    /// <summary>
    /// Sets the cost of the card. Any and all costs can be set this way.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="bloodCost">The cost in blood (sacrifices)</param>
    /// <param name="bonesCost">The cost in bones</param>
    /// <param name="energyCost">The cost in energy</param>
    /// <param name="gemsCost">The cost in gems</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetCost(this CardInfo info, int? bloodCost = 0, int? bonesCost = 0, int? energyCost = 0, List<GemType> gemsCost = null)
    {
        info.SetBloodCost(bloodCost);
        info.SetBonesCost(bonesCost);
        info.SetEnergyCost(energyCost);
        return info.SetGemsCost(gemsCost);
    }

    /// <summary>
    /// Sets the blood cost of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="bloodCost">The cost in blood (sacrifices)</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetBloodCost(this CardInfo info, int? bloodCost = 0)
    {
        if (bloodCost.HasValue)
            info.cost = bloodCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the bones cost of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="bonesCost">The cost in bones</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetBonesCost(this CardInfo info, int? bonesCost = 0)
    {
        if (bonesCost.HasValue)
            info.bonesCost = bonesCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the energy cost of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="energyCost">The cost in energy</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEnergyCost(this CardInfo info, int? energyCost = 0)
    {
        if (energyCost.HasValue)
            info.energyCost = energyCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the gems cost of the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="gemsCost">The cost in gems</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetGemsCost(this CardInfo info, List<GemType> gemsCost = null)
    {
        info.gemsCost = gemsCost ?? new();
        return info;
    }

    #endregion

    #endregion

    #region Portraits

    #region Main

    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="emission">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns></returns>
    public static CardInfo SetPortrait(this CardInfo info, Texture2D portrait, Texture2D emission, FilterMode? filterMode = null)
    {
        info.SetPortrait(portrait, filterMode);
        info.SetEmissivePortrait(emission, filterMode);
        return info;
    }

    /// <summary>
    /// Sets the default card portrait for the card
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="sprite">The sprite containing the card portrait</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// <param name="info">Card to access</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <param name="pathToEmission">The path to the .png file containing the emission artwork (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, string pathToArt, string pathToEmission)
    {
        info.SetPortrait(pathToArt);
        info.SetEmissivePortrait(pathToEmission);
        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="emission">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetPortraitAndEmission(this CardInfo info, Texture2D portrait, Texture2D emission, FilterMode? filterMode = null)
    {
        info.SetPortrait(portrait, filterMode);
        info.SetEmissivePortrait(emission, filterMode);
        return info;
    }

    /// <summary>
    /// Sets the cards portrait and emission at the same time.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <param name="emission">The sprite containing the emission</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// <param name="info">Card to access</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetAltPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetAltPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetAltPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the card's alternate portrait. This portrait is only used when asked for by an ability or an appearance behavior.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissivePortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the emissive portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="sprite">The sprite containing the emission</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEmissivePortrait(this CardInfo info, Sprite sprite)
    {
        if (info.portraitTex == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting normal portrait");

        info.portraitTex.RegisterEmissionForSprite(sprite);

        return info;
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="pathToArt">The path to the .png file containing the artwork (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetEmissiveAltPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The texture containing the emission</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetEmissiveAltPortrait(GetPortrait(portrait, TextureHelper.SpriteType.CardPortrait, filterMode));
    }

    /// <summary>
    /// Sets the emissive alternate portrait for the card. This can only be done after the default portrait has been set (SetPortrait)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The sprite containing the emission</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetEmissiveAltPortrait(this CardInfo info, Sprite portrait)
    {
        if (info.alternatePortrait == null)
            throw new InvalidOperationException($"Cannot set emissive portrait before setting normal portrait");

        info.alternatePortrait.RegisterEmissionForSprite(portrait);

        return info;
    }

    #endregion

    #region Pixel

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, string pathToArt)
    {
        return info.SetPixelPortrait(TextureHelper.GetImageAsTexture(pathToArt));
    }

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, Texture2D portrait, FilterMode? filterMode = null)
    {
        return info.SetPixelPortrait(
            !filterMode.HasValue
                ? portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait)
                : portrait.ConvertTexture(TextureHelper.SpriteType.PixelPortrait, filterMode.Value)
        );
    }

    /// <summary>
    /// Sets the card's pixel portrait. This portrait is used when the card is displayed in GBC mode (Act 2).
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetPixelPortrait(this CardInfo info, Sprite portrait)
    {
        info.pixelPortrait = portrait;

        if (!string.IsNullOrEmpty(info.name))
            info.pixelPortrait.name = info.name + "_pixelportrait";

        return info;
    }

    #endregion

    #region LostTail

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// <param name="info">Card to access</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <returns>The same card info so a chain can continue</returns>
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
    /// <param name="info">Tail to access</param>
    /// <param name="pathToArt">The path to the .png file containing the portrait artwork (relative to the Plugins directory)</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, string pathToArt, CardInfo owner)
    {
        owner.tailParams = info;
        return info.SetLostTailPortrait(TextureHelper.GetImageAsTexture(pathToArt), owner);
    }

    /// <summary>
    /// Sets the card's lost tail portrait. This portrait is used when the card has the TailOnHit ability and has dodged a hit.
    /// </summary>
    /// <param name="info">Tail to access</param>
    /// <param name="portrait">The texture containing the card portrait</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue</returns>
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
    /// <param name="info">Tail to access</param>
    /// <param name="portrait">The sprite containing the card portrait</param>
    /// <param name="owner">The card that the tail parameters belongs to.</param>
    /// <returns>The same TailParams so a chain can continue</returns>
    public static TailParams SetLostTailPortrait(this TailParams info, Sprite portrait, CardInfo owner)
    {
        info.tailLostPortrait = portrait;

        if (!string.IsNullOrEmpty(owner.name))
            info.tailLostPortrait.name = owner.name + "_tailportrait";

        TextureHelper.TryReuseEmission(owner, info.tailLostPortrait);

        return info;
    }

    #endregion

    #endregion

    #endregion

    #region Helpers
    

    /// <summary>
    /// Create a basic EncounterBlueprintData.CardBlueprint based off the CardInfo object.
    /// </summary>
    /// <param name="cardInfo">CardInfo to create the blueprint with.</param>
    /// <returns>The CardBlueprint object that can be used when creating EncounterData.</returns>
    public static EncounterBlueprintData.CardBlueprint CreateBlueprint(this CardInfo cardInfo)
    {
        return new EncounterBlueprintData.CardBlueprint
        {
            card = cardInfo
        };
    }

    /// <summary>
    /// Check the CardInfo not having a specific Ability.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="ability">The ability to check for.</param>
    /// <returns>true if the ability does not exist.</returns>
    public static bool LacksAbility(this CardInfo cardInfo, Ability ability)
    {
        return !cardInfo.HasAbility(ability);
    }

    /// <summary>
    /// Check the CardInfo having a specific SpecialTriggeredAbility.
    ///
    /// A condensed version of `cardInfo.SpecialAbilities.Contains(ability)`.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="ability">The specialTriggeredAbility to check for.</param>
    /// <returns>true if the specialTriggeredAbility does exist.</returns>
    public static bool HasSpecialAbility(this CardInfo cardInfo, SpecialTriggeredAbility ability)
    {
        return cardInfo.SpecialAbilities.Contains(ability);
    }
    
    /// <summary>
    /// Check the CardInfo not having a specific Ability.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="ability">The specialTriggeredAbility to check for.</param>
    /// <returns>true if the specialTriggeredAbility does not exist.</returns>
    public static bool LacksSpecialAbility(this CardInfo cardInfo, SpecialTriggeredAbility ability)
    {
        return !cardInfo.HasSpecialAbility(ability);
    }
    
    /// <summary>
    /// Check the CardInfo not having a specific Trait.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="trait">The specialTriggeredAbility to check for.</param>
    /// <returns>true if the card is does not have the specified trait.</returns>
    public static bool LacksTrait(this CardInfo cardInfo, Trait trait)
    {
        return !cardInfo.HasTrait(trait);
    }
    
    /// <summary>
    /// Check the CardInfo not being of a specific Tribe.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <param name="tribe">The tribe to check for.</param>
    /// <returns>true if the card is not of the specified tribe.</returns>
    public static bool IsNotOfTribe(this CardInfo cardInfo, Tribe tribe)
    {
        return !cardInfo.IsOfTribe(tribe);
    }

    /// <summary>
    /// Spawn the CardInfo object to the player's hand.
    /// </summary>
    /// <param name="cardInfo">CardInfo to access.</param>
    /// <returns>The enumeration of the card being placed in the player's hand.</returns>
    public static IEnumerator SpawnInHand(this CardInfo cardInfo)
    {
        yield return CardSpawner.Instance.SpawnCardToHand(cardInfo);
    }

    #region PlayableCard

    
    /// <summary>
    /// Checks if the card has a specific Trait.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="trait">The trait to check for.</param>
    /// <returns>true if the card has the specified trait.</returns>
    public static bool HasTrait(this PlayableCard playableCard, Trait trait)
    {
        return playableCard.Info.HasTrait(trait);
    }
    
    /// <summary>
    /// Checks if the card has a specific Trait.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="trait">The trait to check for.</param>
    /// <returns>true if the card does not have the specified trait.</returns>
    public static bool LacksTrait(this PlayableCard playableCard, Trait trait)
    {
        return playableCard.Info.LacksTrait(trait);
    }
    
    /// <summary>
    /// Checks if the PlayableCard is of a specified Tribe.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="tribe">The tribe to check for.</param>
    /// <returns>true if the card is of the specified tribe.</returns>
    public static bool IsOfTribe(this PlayableCard playableCard, Tribe tribe)
    {
        return playableCard.Info.IsOfTribe(tribe);
    }
    
    /// <summary>
    /// Checks if the PlayableCard is not of a specified Tribe.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <param name="tribe">The tribe to check for.</param>
    /// <returns>true if the card is not of the specified tribe.</returns>
    public static bool IsNotOfTribe(this PlayableCard playableCard, Tribe tribe)
    {
        return playableCard.Info.IsNotOfTribe(tribe);
    }
    
    /// <summary>
    /// Checks if the card is not dead.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <returns>true if the card is not dead.</returns>
    public static bool NotDead(this PlayableCard playableCard)
    {
        return playableCard && !playableCard.Dead;
    }
    
    /// <summary>
    /// Checks if the card is not the opponent's card.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access.</param>
    /// <returns>true if card is not the opponent's card.</returns>
    public static bool IsPlayerCard(this PlayableCard playableCard)
    {
        return !playableCard.OpponentCard;
    } 

    /// <summary>
    /// Check the PlayableCard not having a specific Ability.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="ability">The ability to check for</param>
    /// <returns>true if the ability does not exist</returns>
    public static bool LacksAbility(this PlayableCard playableCard, Ability ability)
    {
        return !playableCard.HasAbility(ability);
    }

    /// <summary>
    /// Check the PlayableCard not having a specific SpecialTriggeredAbility.
    ///
    /// A condensed version of `playableCard.Info.SpecialAbilities.Contains(ability)`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="ability">The specialTriggeredAbility to check for</param>
    /// <returns>true if the specialTriggeredAbility does not exist</returns>
    public static bool LacksSpecialAbility(this PlayableCard playableCard, SpecialTriggeredAbility ability)
    {
        return !playableCard.HasSpecialAbility(ability);
    }

    /// <summary>
    /// Check the PlayableCard having a specific SpecialTriggeredAbility.
    ///
    /// A condensed version of `playableCard.Info.SpecialAbilities.Contains(ability)`.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <param name="ability">The specialTriggeredAbility to check for</param>
    /// <returns>true if the specialTriggeredAbility does exist</returns>
    public static bool HasSpecialAbility(this PlayableCard playableCard, SpecialTriggeredAbility ability)
    {
        return playableCard.Info.HasSpecialAbility(ability);
    }

    /// <summary>
    /// Check if the PlayableCard has a card opposing it in the opposite slot.
    ///
    /// Also acts as a null check if this PlayableCard is in a slot.
    /// </summary>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <returns>true if a card exists in the opposing slot</returns>
    public static bool HasOpposingCard(this PlayableCard playableCard)
    {
        return playableCard.Slot && playableCard.Slot.opposingSlot.Card;
    }
    
    /// <summary>
    /// Retrieve the CardSlot object that is opposing this PlayableCard.
    /// </summary>
    /// <remarks>It is on the implementer to check if the returned value is not null</remarks>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <returns>The card slot opposite of this playableCard, otherwise return null.</returns>
    public static CardSlot OpposingSlot(this PlayableCard playableCard)
    {
        return playableCard.Slot ? playableCard.Slot.opposingSlot : null;
    }
    
    /// <summary>
    /// Retrieve the PlayableCard that is opposing this PlayableCard in the opposite slot.
    /// </summary>
    /// <remarks>It is on the implementer to check if the returned value is not null</remarks>
    /// <param name="playableCard">PlayableCard to access</param>
    /// <returns>The card in the opposing slot, otherwise return null.</returns>
    public static PlayableCard OpposingCard(this PlayableCard playableCard)
    {
        return playableCard.OpposingSlot()?.Card;
    }
    

    #endregion

    #endregion

    #region ExtendedProperties

    /// <summary>
    /// Adds a custom property value to the card.
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="propertyName">The name of the property to set</param>
    /// <param name="value">The value of the property</param>
    /// <returns>The same card info so a chain can continue</returns>
    public static CardInfo SetExtendedProperty(this CardInfo info, string propertyName, object value)
    {
        info.GetCardExtensionTable()[propertyName] = value?.ToString();
        return info;
    }

    /// <summary>
    /// Gets a custom property value from the card
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="propertyName">The name of the property to get the value of</param>
    /// <returns></returns>
    public static string GetExtendedProperty(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var ret);
        return ret;
    }

    /// <summary>
    /// Gets a custom property as an int (can by null)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as an int or null if it didn't exist or couldn't be parsed as int</returns>
    public static int? GetExtendedPropertyAsInt(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return int.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a float (can by null)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as a float or null if it didn't exist or couldn't be parsed as float</returns>
    public static float? GetExtendedPropertyAsFloat(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return float.TryParse(str, out var ret) ? ret : null;
    }

    /// <summary>
    /// Gets a custom property as a boolean (can be null)
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="propertyName">Property name to get value of</param>
    /// <returns>Returns the value of the property as a boolean or null if it didn't exist or couldn't be parsed as boolean</returns>
    public static bool? GetExtendedPropertyAsBool(this CardInfo info, string propertyName)
    {
        info.GetCardExtensionTable().TryGetValue(propertyName, out var str);
        return bool.TryParse(str, out var ret) ? ret : null;
    }

    #endregion

    #region ModPrefixesAndTags

    /// <summary>
    /// Sets the mod guid that was derived from the call stack
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="modGuid">Mod Guid</param>
    /// <returns>The same card info so a chain can continue</returns>
    internal static CardInfo SetModTag(this CardInfo info, string modGuid)
    {
        info.SetExtendedProperty("CallStackModGUID", modGuid);
        return info;
    }

    /// <summary>
    /// Gets the GUID of the mod that created this card
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>The ID of the mod that created this card, or null if it wasn't found</returns>
    public static string GetModTag(this CardInfo info)
    {
        return info.GetExtendedProperty("CallStackModGUID");
    }

    /// <summary>
    /// Sets the mod prefix for the card
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <param name="modPrefix">Mod prefix</param>
    /// <returns>The same card info so a chain can continue</returns>
    internal static CardInfo SetModPrefix(this CardInfo info, string modPrefix)
    {
        info.SetExtendedProperty("ModPrefix", modPrefix);
        return info;
    }

    /// <summary>
    /// Gets the card name prefix for this card
    /// </summary>
    /// <param name="info">Card to access</param>
    /// <returns>The mod prefix for this card, or null if it wasn't found</returns>
    public static string GetModPrefix(this CardInfo info)
    {
        return info.GetExtendedProperty("ModPrefix");
    }

    #endregion

    internal static CardInfo SetOldApiCard(this CardInfo info, bool isOldApiCard = true)
    {
        info.SetExtendedProperty("AddedByOldApi", isOldApiCard);
        return info;
    }

    internal static bool IsOldApiCard(this CardInfo info)
    {
        bool? isOAPI = info.GetExtendedPropertyAsBool("AddedByOldApi");
        return isOAPI.HasValue && isOAPI.Value;
    }
}