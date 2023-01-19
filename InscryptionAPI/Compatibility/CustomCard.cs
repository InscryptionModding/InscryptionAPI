using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace APIPlugin;

[Obsolete("Use CardManager instead", true)]
public class CustomCard
{
    public static List<CustomCard> cards = new();

    public static Dictionary<int, List<AbilityIdentifier>> abilityIds = new();
    public static Dictionary<int, List<SpecialAbilityIdentifier>> specialAbilityIds = new();
    public static Dictionary<int, EvolveIdentifier> evolveIds = new();
    public static Dictionary<int, IceCubeIdentifier> iceCubeIds = new();
    public static Dictionary<int, TailIdentifier> tailIds = new();
    public static Dictionary<string, Sprite> emissions = new();

    public string name;
    public List<CardMetaCategory> metaCategories;
    public CardComplexity? cardComplexity;
    public CardTemple? temple;
    public string displayedName;
    public int? baseAttack;
    public int? baseHealth;
    public string description;
    public bool? hideAttackAndHealth;
    public int? cost;
    public int? bonesCost;
    public int? energyCost;
    public List<GemType> gemsCost;
    public SpecialStatIcon? specialStatIcon;
    public List<Tribe> tribes;
    public List<Trait> traits;
    public List<SpecialTriggeredAbility> specialAbilities;
    public List<Ability> abilities;
    public EvolveParams evolveParams;
    public string defaultEvolutionName;
    public TailParams tailParams;
    public IceCubeParams iceCubeParams;
    public bool? flipPortraitForStrafe;
    public bool? onePerDeck;
    public List<CardAppearanceBehaviour.Appearance> appearanceBehaviour;
    [IgnoreMapping] public Texture2D tex;
    [IgnoreMapping] public Texture2D altTex;
    public Texture titleGraphic;
    [IgnoreMapping] public Texture2D pixelTex;
    [IgnoreMapping] public Texture2D emissionTex;
    public GameObject animatedPortrait;
    public List<Texture> decals;

    [IgnoreMapping] private List<AbilityIdentifier> abilityIdParam;
    [IgnoreMapping] private List<SpecialAbilityIdentifier> specialAbilityIdParam;
    [IgnoreMapping] private EvolveIdentifier evolveId;
    [IgnoreMapping] private IceCubeIdentifier iceCubeId;
    [IgnoreMapping] private TailIdentifier tailId;

    public CustomCard(
        string name,
        List<AbilityIdentifier> abilityIdParam = null,
        List<SpecialAbilityIdentifier> specialAbilityIdParam = null,
        EvolveIdentifier evolveId = null,
        IceCubeIdentifier iceCubeId = null,
        TailIdentifier tailId = null)
    {
        this.name = name;
        this.abilityIdParam = abilityIdParam;
        this.specialAbilityIdParam = specialAbilityIdParam;
        this.evolveId = evolveId;
        this.iceCubeId = iceCubeId;
        this.tailId = tailId;

        CardManager.ModifyCardList += delegate (List<CardInfo> cards)
        {
            CardInfo targetCard = cards.CardByName(this.name);

            if (targetCard != null)
                this.AdjustCard(targetCard, cards);

            return cards;
        };
    }

    public CardInfo AdjustCard(CardInfo card, List<CardInfo> cardsToSearch)
    {
        TypeMapper<CustomCard, CardInfo>.Convert(this, card);

        if (this.tex is not null)
            card.SetPortrait(this.tex);

        if (this.altTex is not null)
            card.SetAltPortrait(this.altTex);

        if (this.pixelTex is not null)
            card.SetPixelPortrait(this.pixelTex);

        if (this.decals is not null)
            card.AddDecal(this.decals.ToArray());

        if (this.abilityIdParam != null)
            card.AddAbilities(this.abilityIdParam.Select(ai => ai.realID).ToArray());

        if (this.specialAbilityIdParam != null)
            card.AssignSpecialAbilities(this.specialAbilityIdParam);

        if (this.evolveId != null)
        {
            // We're already in the mapper. I don't want to add another event handler
            CardInfo evolveTarget = cardsToSearch.CardByName(this.evolveId.name);
            if (evolveTarget != null)
                card.SetEvolve(evolveTarget, this.evolveId.turnsToEvolve, new List<CardModificationInfo>() { this.evolveId.mods });
        }

        if (this.tailId != null)
        {
            // We're already in the mapper. I don't want to add another event handler
            CardInfo tail = cardsToSearch.CardByName(this.tailId.name);
            if (tail != null)
                card.SetTail(tail, this.tailId.tailLostTex, mods: new List<CardModificationInfo>() { this.tailId.mods });
        }

        if (this.iceCubeId != null)
        {
            // We're already in the mapper. I don't want to add another event handler
            CardInfo iceCubeTarget = cardsToSearch.CardByName(this.iceCubeId.name);
            if (iceCubeTarget != null)
                card.SetIceCube(iceCubeTarget, new List<CardModificationInfo>() { this.iceCubeId.mods });
        }

        return card;
    }
}