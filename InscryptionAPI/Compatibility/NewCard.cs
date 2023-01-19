using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace APIPlugin;

[Obsolete("Use CardManager instead", true)]
public static class NewCard
{
    public static List<CardInfo> cards = new();

    public static Dictionary<int, List<AbilityIdentifier>> abilityIds = new();
    public static Dictionary<int, List<SpecialAbilityIdentifier>> specialAbilityIds = new();
    public static Dictionary<int, EvolveIdentifier> evolveIds = new();
    public static Dictionary<int, IceCubeIdentifier> iceCubeIds = new();
    public static Dictionary<int, TailIdentifier> tailIds = new();

    public static Dictionary<string, Sprite> emissions = new();

    public static void Add(CardInfo card, List<AbilityIdentifier> abilityIdsParam = null,
        List<SpecialAbilityIdentifier> specialAbilitiesIdsParam = null,
        EvolveIdentifier evolveId = null,
        IceCubeIdentifier iceCubeId = null, TailIdentifier tailId = null)
    {
        if (abilityIdsParam != null)
            card.AddAbilities(abilityIdsParam.Select(a => a.realID).ToArray());

        if (specialAbilitiesIdsParam != null)
            card.AssignSpecialAbilities(specialAbilitiesIdsParam);

        if (evolveId != null)
            card.evolveParams = evolveId.Evolution;

        if (iceCubeId != null)
            card.iceCubeParams = iceCubeId.IceCube;

        if (tailId != null)
            card.tailParams = tailId.Tail;

        CardManager.Add(card);
    }

    public static void Add(string name, string displayedName, int baseAttack, int baseHealth,
        List<CardMetaCategory> metaCategories, CardComplexity cardComplexity, CardTemple temple,
        string description = null, bool hideAttackAndHealth = false,
        int bloodCost = 0, int bonesCost = 0, int energyCost = 0,
        List<GemType> gemsCost = null, SpecialStatIcon specialStatIcon = SpecialStatIcon.None,
        List<Tribe> tribes = null, List<Trait> traits = null, List<SpecialTriggeredAbility> specialAbilities = null,
        List<Ability> abilities = null, List<AbilityIdentifier> abilityIdsParam = null,
        List<SpecialAbilityIdentifier> specialAbilitiesIdsParam = null, EvolveParams evolveParams = null,
        string defaultEvolutionName = null, TailParams tailParams = null, IceCubeParams iceCubeParams = null,
        bool flipPortraitForStrafe = false, bool onePerDeck = false,
        List<CardAppearanceBehaviour.Appearance> appearanceBehaviour = null, Texture2D defaultTex = null,
        Texture2D altTex = null, Texture titleGraphic = null, Texture2D pixelTex = null,
        Texture2D emissionTex = null, GameObject animatedPortrait = null, List<Texture> decals = null,
        EvolveIdentifier evolveId = null, IceCubeIdentifier iceCubeId = null, TailIdentifier tailId = null)
    {

        CardInfo info = ScriptableObject.CreateInstance<CardInfo>();
        info.SetOldApiCard(true);

        info.name = name;
        info.displayedName = displayedName;
        info.baseAttack = baseAttack;
        info.baseHealth = baseHealth;
        info.description = description;

        info.SetCost(bloodCost, bonesCost, energyCost, gemsCost);

        info.hideAttackAndHealth = hideAttackAndHealth;
        info.cardComplexity = cardComplexity;
        info.temple = temple;
        info.AddMetaCategories(metaCategories.ToArray());
        info.specialStatIcon = specialStatIcon;

        if (tribes != null)
            info.AddTribes(tribes.ToArray());

        if (traits != null)
            info.AddTraits(traits.ToArray());

        if (specialAbilities != null)
            info.AddSpecialAbilities(specialAbilities.ToArray());

        if (specialAbilitiesIdsParam != null)
            info.AssignSpecialAbilities(specialAbilitiesIdsParam);

        if (abilities != null)
            info.AddAbilities(abilities.ToArray());

        if (abilityIdsParam != null)
            info.AddAbilities(abilityIdsParam.Select(a => a.realID).ToArray());

        info.evolveParams = evolveParams;
        info.tailParams = tailParams;
        info.iceCubeParams = iceCubeParams;
        info.defaultEvolutionName = defaultEvolutionName;

        info.flipPortraitForStrafe = flipPortraitForStrafe;
        info.onePerDeck = onePerDeck;

        if (appearanceBehaviour != null)
            info.AddAppearances(appearanceBehaviour.ToArray());

        if (defaultTex != null)
            info.SetPortrait(defaultTex);

        if (altTex != null)
            info.SetAltPortrait(altTex);

        info.titleGraphic = titleGraphic;

        if (pixelTex != null)
            info.SetPixelPortrait(pixelTex);

        if (emissionTex != null)
            info.SetEmissivePortrait(emissionTex);

        info.animatedPortrait = animatedPortrait;

        if (decals != null)
            info.decals = new List<Texture>(decals);

        if (evolveId != null)
            info.SetEvolve(evolveId.name, evolveId.turnsToEvolve, new List<CardModificationInfo>() { evolveId.mods });

        if (iceCubeId != null)
            info.SetIceCube(iceCubeId.name, new List<CardModificationInfo>() { iceCubeId.mods });

        if (tailId != null)
            info.SetTail(tailId.name, tailId.tailLostTex, mods: new List<CardModificationInfo>() { tailId.mods });

        CardManager.Add(info);
    }

    internal static void AssignSpecialAbilities(this CardInfo info, IEnumerable<SpecialAbilityIdentifier> ids)
    {
        info.AddSpecialAbilities(ids.Where(a => !a.ForStatIcon).Select(a => a.specialTriggerID).ToArray());

        SpecialAbilityIdentifier statId = ids.FirstOrDefault(a => a.ForStatIcon);
        if (statId != null)
            info.specialStatIcon = statId.statIconID;
    }
}