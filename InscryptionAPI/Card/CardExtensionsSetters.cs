using DiskCardGame;
using InscryptionAPI.Helpers;
using Sirenix.Utilities;
using UnityEngine;

namespace InscryptionAPI.Card;

public static partial class CardExtensions
{
    #region Fields
    /// <summary>
    /// Sets a number of basic properties of the card
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="displayedName">Displayed name of the card.</param>
    /// <param name="attack">Attack of the card.</param>
    /// <param name="health">Health of the card.</param>
    /// <param name="description">The description that plays when the card is seen for the first time.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBasic(this CardInfo info, string displayedName, int attack, int health, string description = default)
    {
        info.displayedName = displayedName;
        info.baseAttack = attack;
        info.baseHealth = health;
        info.description = description;
        return info;
    }

    /// <summary>
    /// Sets the base attack and health of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="baseAttack">The base attack for the card.</param>
    /// <param name="baseHealth">The base health for the card.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBaseAttackAndHealth(this CardInfo info, int? baseAttack = 0, int? baseHealth = 0)
    {
        if (baseAttack.HasValue)
            info.baseAttack = baseAttack.Value;

        if (baseHealth.HasValue)
            info.baseHealth = baseHealth.Value;

        return info;
    }

    /// <summary>
    /// Sets the CardTemple for the card. Used to determine what cards are obtainable in each Act.
    /// Act 1 uses Nature cards, Act 2 uses all Temples for each region, Act 3 uses Tech cards.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="temple">The CardTemple to use.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCardTemple(this CardInfo info, CardTemple temple)
    {
        info.temple = temple;
        return info;
    }
    /// <summary>
    /// Sets the CardComplexity for the card. Used mainly in Act 1 to determine whether a certain is obtainable.
    /// Vanilla: unlocked and learned by default
    /// Simple: unlocked by default
    /// Intermediate: unlocked after the second tutorial
    /// Advanced: unlocked after the third tutorial if all its abilities have been learned
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="complexity">The CardComplexity to use.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCardComplexity(this CardInfo info, CardComplexity complexity)
    {
        info.cardComplexity = complexity;
        return info;
    }

    /// <summary>
    /// Sets the displayed name of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="displayedName">The displayed name for the card.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetDisplayedName(this CardInfo info, string displayedName)
    {
        info.displayedName = displayedName.IsNullOrWhitespace() ? "" : displayedName;
        return info;
    }

    /// <summary>
    /// Sets the name of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="name">The name for the card.</param>
    /// <param name="modPrefix">The string that will be prefixed to the card name if it doesn't already exist.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetName(this CardInfo info, string name, string modPrefix = default)
    {
        info.name = !string.IsNullOrEmpty(modPrefix) && !name.StartsWith(modPrefix) ? $"{modPrefix}_{name}" : name;
        return info;
    }

    /// <summary>
    /// Sets the card name and displayed name of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="name">The name for the card.</param>
    /// <param name="displayedName">The displayed name for the card.</param>
    /// <param name="modPrefix">The string that will be prefixed to the card name if it doesn't already exist.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetNames(this CardInfo info, string name, string displayedName, string modPrefix = default(string))
    {
        info.SetDisplayedName(displayedName);
        return info.SetName(name, modPrefix);
    }

    /// <summary>
    /// Sets the CardInfo's special abilities field to any number of special abilities to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="abilities">The abilities to add.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetSpecialAbilities(this CardInfo info, params SpecialTriggeredAbility[] abilities)
    {
        info.specialAbilities = abilities?.ToList();
        return info;
    }

    /// <summary>
    /// Sets the stat icon to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="statIcon">The stat icon to set.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetStatIcon(this CardInfo info, SpecialStatIcon statIcon)
    {
        info.specialStatIcon = statIcon;
        info.AddSpecialAbilities(StatIconManager.AllStatIcons.FirstOrDefault(sii => sii.Id == statIcon).AbilityId);
        return info;
    }

    /// <summary>
    /// Sets any number of traits to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="traits">The traits to add.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTraits(this CardInfo info, params Trait[] traits)
    {
        info.traits = traits?.ToList();
        return info;
    }


    /// <summary>
    /// Set any number of tribes to the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tribes">The tribes to add.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTribes(this CardInfo info, params Tribe[] tribes)
    {
        info.tribes = tribes?.ToList();
        return info;
    }

    /// <summary>
    /// Sets whether the card should be Gemified or not. Can and will un-Gemify cards.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="gemify">Whether the card should be gemified.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGemify(this CardInfo info, bool gemify = true)
    {
        if (gemify && !info.Mods.Exists(x => x.gemify))
            info.Mods.Add(new() { gemify = true });
        else if (!gemify)
            info.Mods.FindAll(x => x.gemify).ForEach(y => y.gemify = false);

        return info;
    }

    #endregion

    #region MetaCategories
    /// <summary>
    /// Sets the card to behave as a "normal" card in Part 1. The CardTemple is Nature and it will appear in choice nodes and trader nodes.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
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
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
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
    /// <param name="info">CardInfo to access.</param>
    /// <param name="temple">The temple that the card will exist under.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGBCPlayable(this CardInfo info, CardTemple temple)
    {
        info.AddMetaCategories(CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable);
        info.temple = temple;
        return info;
    }


    /// <summary>
    /// Sets the card so it shows up for rare card choices and applies the rare background.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
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
    /// <param name="info">Card to access.</param>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo SetTerrain(this CardInfo info)
    {
        info.AddTraits(Trait.Terrain);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);
        return info;
    }
    /// <summary>
    /// Adds the Terrain trait and background to this card, with the option to not use TerrainLayout.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTerrain(this CardInfo info, bool useTerrainLayout)
    {
        info.AddTraits(Trait.Terrain);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground);
        if (useTerrainLayout)
            info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout);
        return info;
    }

    /// <summary>
    /// Adds the Pelt trait and background to this card, and optionally adds the SpawnLice special ability.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetPelt(this CardInfo info, bool spawnLice = true)
    {
        info.AddTraits(Trait.Pelt);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);
        if (spawnLice)
            info.AddSpecialAbilities(SpecialTriggeredAbility.SpawnLice);
        return info;
    }

    #endregion

    #region EvolveIceCubeTail
    /// <summary>
    /// Sets the evolve parameters of the card. These parameters are used to make the Evolve ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="evolveCard">The card that will be generated after the set number of turns.</param>
    /// <param name="numberOfTurns">The number of turns before the card evolves.</param>
    /// <param name="mods">A set of card mods to be applied to the evolved card.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEvolve(this CardInfo info, CardInfo evolveCard, int numberOfTurns, IEnumerable<CardModificationInfo> mods = null)
    {
        info.evolveParams = new()
        {
            evolution = evolveCard,
            turnsToEvolve = numberOfTurns
        };

        if (mods != null && mods.Any())
        {
            info.evolveParams.evolution = CardLoader.Clone(info.evolveParams.evolution); // set the evolution to a clone of the evolveCard info
            (info.evolveParams.evolution.mods ??= new()).AddRange(mods);
        }

        return info;
    }

    /// <summary>
    /// Sets the evolve parameters of the card. These parameters are used to make the Evolve ability function correctly.
    /// This function uses delayed loading to attach the evolution to the card, so if the evolve card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="evolveInto">The name of card that will be generated after the set number of turns.</param>
    /// <param name="numberOfTurns">The number of turns before the card evolves.</param>
    /// <param name="mods">A set of card mods to be applied to the evolved card.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEvolve(this CardInfo info, string evolveInto, int numberOfTurns, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo evolution = CardManager.AllCardsCopy.CardByName(evolveInto);

        if (evolution == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
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
    /// Sets the default evolution name for the card. This is the name used when the card doesn't evolve into another card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="defaultName">The default evolution name to use. Pass in 'null' to use the vanilla default.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetDefaultEvolutionName(this CardInfo info, string defaultName)
    {
        info.defaultEvolutionName = defaultName;
        return info;
    }

    /// <summary>
    /// Sets the ice cube parameters of the card. These parameters are used to make the IceCube ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="iceCube">The card that will be generated when this card dies.</param>
    /// <param name="mods">A set of card mods to be applied to the ice cube contents.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
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
    /// <param name="info">CardInfo to access.</param>
    /// <param name="iceCubeName">The name of the card that will be generated when this card dies.</param>
    /// <param name="mods">A set of card mods to be applied to the ice cube contents.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetIceCube(this CardInfo info, string iceCubeName, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo creatureWithin = CardManager.AllCardsCopy.CardByName(iceCubeName);

        if (creatureWithin == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
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
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="pathToLostTailArt">The path to the .png file containing the lost tail artwork (relative to the Plugins directory).</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, string pathToLostTailArt = null, IEnumerable<CardModificationInfo> mods = null)
    {
        Texture2D lostTailPortrait = pathToLostTailArt == null ? null : TextureHelper.GetImageAsTexture(pathToLostTailArt);
        return info.SetTail(tailName, lostTailPortrait, mods: mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, IEnumerable<CardModificationInfo> mods = null)
    {
        return info.SetTail(tailName, tailLostPortrait: null, mods: mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, CardInfo tail, IEnumerable<CardModificationInfo> mods = null)
    {
        return info.SetTail(tail, null, mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, CardInfo tail, Texture2D tailLostPortrait, FilterMode? filterMode = null, IEnumerable<CardModificationInfo> mods = null)
    {
        var tailLostSprite = !filterMode.HasValue
                                 ? tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
                                 : tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);
        return info.SetTail(tail, tailLostSprite, mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tail">The card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The sprite containing the card portrait.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
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
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The texture containing the card portrait.</param>
    /// <param name="filterMode">The filter mode for the texture, or null if no change.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, Texture2D tailLostPortrait, FilterMode? filterMode = null, IEnumerable<CardModificationInfo> mods = null)
    {
        var tailLostSprite = !filterMode.HasValue
            ? tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait)
            : tailLostPortrait?.ConvertTexture(TextureHelper.SpriteType.CardPortrait, filterMode.Value);
        return info.SetTail(tailName, tailLostSprite, mods);
    }

    /// <summary>
    /// Sets the tail parameters of the card. These parameters are used to make the TailOnHit ability function correctly.
    /// This function uses delayed loading to attach the tail to the card, so if the tail card doesn't exist yet, this function will still work.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="tailName">The name of the card that will be generated as the "tail" when the first hit is dodged.</param>
    /// <param name="tailLostPortrait">The sprite containing the card portrait.</param>
    /// <param name="mods">A set of card mods to be applied to the tail.</param>
    /// <returns>The same CardInfo so a chain can continue..</returns>
    public static CardInfo SetTail(this CardInfo info, string tailName, Sprite tailLostPortrait, IEnumerable<CardModificationInfo> mods = null)
    {
        CardInfo tail = CardManager.AllCardsCopy.CardByName(tailName);

        if (tail == null) // Try delayed loading
        {
            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
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
    /// <param name="info">CardInfo to access.</param>
    /// <param name="bloodCost">The cost in blood (sacrifices).</param>
    /// <param name="bonesCost">The cost in bones.</param>
    /// <param name="energyCost">The cost in energy.</param>
    /// <param name="gemsCost">The cost in gems.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
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
    /// <param name="info">CardInfo to access.</param>
    /// <param name="bloodCost">The cost in blood (sacrifices).</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBloodCost(this CardInfo info, int? bloodCost = 0)
    {
        if (bloodCost.HasValue)
            info.cost = bloodCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the bones cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="bonesCost">The cost in bones.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetBonesCost(this CardInfo info, int? bonesCost = 0)
    {
        if (bonesCost.HasValue)
            info.bonesCost = bonesCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the energy cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="energyCost">The cost in energy.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetEnergyCost(this CardInfo info, int? energyCost = 0)
    {
        if (energyCost.HasValue)
            info.energyCost = energyCost.Value;
        return info;
    }

    /// <summary>
    /// Sets the gems cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="gemsCost">The cost in Mox.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGemsCost(this CardInfo info, List<GemType> gemsCost = null)
    {
        info.gemsCost = gemsCost ?? new();
        return info;
    }
    /// <summary>
    /// Sets the gems cost of the card.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <param name="gemsCost">The cost in Mox.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetGemsCost(this CardInfo info, params GemType[] gemsCost)
    {
        info.gemsCost = gemsCost.ToList();
        return info;
    }
    #endregion

    #region Bools
    /// <summary>
    /// Sets the card's onePerDeck field, which controls whether the player can own multiple copies of it in their deck.
    /// </summary>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo SetOnePerDeck(this CardInfo info, bool onePerDeck = true)
    {
        info.onePerDeck = onePerDeck;
        return info;
    }
    /// <summary>
    /// Sets whether or not the card's Power and Health stats will be displayed.
    /// </summary>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo SetHideStats(this CardInfo info, bool hideStats = true)
    {
        info.hideAttackAndHealth = hideStats;
        return info;
    }
    /// <summary>
    /// Sets whether or not the CardInfo's portrait will be flipped when Strafe and similar abilities change direction.
    /// </summary>
    /// <returns>The same card info so a chain can continue.</returns>
    public static CardInfo SetStrafeFlipsPortrait(this CardInfo info, bool flipPortrait = true)
    {
        info.flipPortraitForStrafe = flipPortrait;
        return info;
    }
    #endregion

    /// <summary>
    /// Indicates if this is a base game card or not.
    /// </summary>
    /// <param name="info">CardInfo to access.</param>
    /// <returns>True of this card came from the base game; false otherwise.</returns>
    public static bool IsBaseGameCard(this CardInfo info) => info.GetExtendedPropertyAsBool("BaseGameCard") ?? false;

    // Sets an indicator of whether this is a base game card or not.
    internal static CardInfo SetBaseGameCard(this CardInfo info, bool isBaseGameCard = true) => info.SetExtendedProperty("BaseGameCard", isBaseGameCard);

    // Sets an indicator of whether this CardInfo was made using the 1.0 API.
    internal static CardInfo SetOldApiCard(this CardInfo info, bool isOldApiCard = true) => info.SetExtendedProperty("AddedByOldApi", isOldApiCard);
    internal static bool IsOldApiCard(this CardInfo info) => info.GetExtendedPropertyAsBool("AddedByOldApi") ?? false;

    /// <summary>
    /// Sets the custom unlock check for the card. Some parts require a card is unlocked before it can be obtained or used.
    /// </summary>
    /// <param name="c">The card.</param>
    /// <param name="check">The custom unlock check, a func that needs to return true for the card to be unlocked. The bool argument is true when the game is in Kaycee's Mod mode and the int argument is the current Kaycee's Mod challenge level.</param>
    /// <returns>The same CardInfo so a chain can continue.</returns>
    public static CardInfo SetCustomUnlockCheck(this CardInfo c, Func<bool, int, bool> check)
    {
        if (check == null && CardManager.CustomCardUnlocks.ContainsKey(c.name))
            CardManager.CustomCardUnlocks.Remove(c.name);
        else
        {
            if (CardManager.CustomCardUnlocks.ContainsKey(c.name))
                CardManager.CustomCardUnlocks[c.name] = check;
            else
                CardManager.CustomCardUnlocks.Add(c.name, check);
        }
        return c;
    }
}