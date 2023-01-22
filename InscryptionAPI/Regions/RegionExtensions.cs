using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine;
using VolumetricFogAndMist;

namespace InscryptionAPI.Regions;

public static class RegionExtensions
{
    public static RegionData RegionByName(this IEnumerable<RegionData> regions, string name) => regions.FirstOrDefault(x => x.name == name);

    public static RegionData SetName(this RegionData region, string name)
    {
        region.name = name;
        return region;
    }

    /// <summary>
    /// Adds terrain cards to this region.<br/>
    /// Terrain cards require the <c>Terrain</c> trait to appear.<br/>
    /// Every region must have at least one valid terrain card.
    /// </summary>
    /// <param name="cards">The terrain cards to add.</param>
    public static RegionData AddTerrainCards(this RegionData region, params string[] cards)
    {
        region.terrainCards = region.terrainCards ?? new();
        foreach (string card in cards)
        {
            CardInfo cardInfo = CardManager.AllCardsCopy.CardByName(card);
            if (!region.terrainCards.Contains(cardInfo))
            {
                region.terrainCards.Add(cardInfo);
            }
        }
        return region;
    }

    /// <summary>
    /// Adds likely cards to this region.<br/>
    /// Likely cards require the <c>ChoiceNode</c> metacategory to appear.<br/>
    /// One of three normal card choices is guaranteed to contain a card from the region's likely cards.
    /// </summary>
    /// <param name="cards">The likely cards to add.</param>
    public static RegionData AddLikelyCards(this RegionData region, params string[] cards)
    {
        region.likelyCards = region.likelyCards ?? new();
        foreach (string card in cards)
        {
            CardInfo cardInfo = CardManager.AllCardsCopy.CardByName(card);
            if (!region.likelyCards.Contains(cardInfo))
            {
                region.likelyCards.Add(cardInfo);
            }
        }
        return region;
    }

    /// <summary>
    /// Adds consumables to this region.<br/>
    /// This only applies to consumables that are <c>regionSpecific</c>. Adding non-region-specific consumables will increase the probability of the consumable appearing.
    /// </summary>
    /// <param name="consumables"></param>
    public static RegionData AddConsumableItems(this RegionData region, params string[] consumables)
    {
        region.consumableItems = region.consumableItems ?? new();
        foreach (string consumable in consumables)
        {
            region.consumableItems.Add(ItemsUtil.GetConsumableByName(consumable));
        }
        return region;
    }

    public static RegionData AddConsumableItems(this RegionData region, params ConsumableItemData[] consumables)
    {
        region.consumableItems = region.consumableItems ?? new();
        foreach (ConsumableItemData consumable in consumables)
        {
            region.consumableItems.Add(consumable);
        }
        return region;
    }

    /// <summary>
    /// Adds dominant tribes to this region.<br/>
    /// One of three normal card choices is guaranteed to contain a card from the region's dominant tribes.<br/>
    /// Every region must have at least one dominant tribe.
    /// </summary>
    /// <param name="tribes">The tribes to add.</param>
    public static RegionData AddDominantTribes(this RegionData region, params Tribe[] tribes)
    {
        region.dominantTribes = region.dominantTribes ?? new();
        foreach (Tribe tribe in tribes)
        {
            region.dominantTribes.Add(tribe);
        }
        return region;
    }

    public static RegionData SetBoardColor(this RegionData region, Color color)
    {
        region.boardLightColor = color;
        return region;
    }

    public static RegionData SetCardsColor(this RegionData region, Color color)
    {
        region.cardsLightColor = color;
        return region;
    }

    /// <summary>
    /// Creates a new encounter for this region and returns the builder.<br/>
    /// Every region with battles needs at least one encounter.
    /// </summary>
    /// <param name="name">The name for the encounter.</param>
    public static EncounterBuilderBlueprintData CreateEncounter(this RegionData region, string name = null)
    {
        EncounterBuilderBlueprintData blueprint = ScriptableObject.CreateInstance<EncounterBuilderBlueprintData>();
        blueprint.SetBasic(name, region);
        return blueprint;
    }

    /// <summary>
    /// Adds normal (card battle node) encounters to this region.<br/>
    /// Every region with battles needs at least one encounter.
    /// </summary>
    /// <param name="encounters">The encounters to add.</param>
    public static RegionData AddEncounters(this RegionData region, params EncounterBlueprintData[] encounters)
    {
        region.encounters = region.encounters ?? new();
        foreach (EncounterBlueprintData encounterData in encounters)
        {
            bool assigned = false;
            for (int i = 0; i < region.encounters.Count; i++)
            {
                if (region.encounters[i].name.Equals(encounterData))
                {
                    region.encounters[i] = encounterData;
                    assigned = true;
                    break;
                }
            }

            if (!assigned)
                region.encounters.Add(encounterData);
        }
        return region;
    }

    /// <summary>
    /// Sets the boss prep encounter condition for this region. If this condition is not met, the 'boss prep encounter' will not appear.
    /// </summary>
    /// <param name="condition">The condition that needs to be fulfilled</param>
    public static RegionData SetBossPrepCondition(this RegionData region, StoryEventCondition condition)
    {
        region.bossPrepCondition = condition;
        return region;
    }

    /// <summary>
    /// Sets the boss prep encounter for this region. The boss prep encounter is the final battle node in the region, before the boss.
    /// </summary>
    /// <param name="encounter">The encounter to set.</param>
    public static RegionData SetBossPrepEncounter(this RegionData region, EncounterBlueprintData encounter)
    {
        region.bossPrepEncounter = encounter;
        return region;
    }

    public static RegionData AddBosses(this RegionData region, params Opponent.Type[] bosses)
    {
        region.bosses = region.bosses ?? new();
        foreach (Opponent.Type bossType in bosses)
        {
            if (!region.bosses.Contains(bossType))
            {
                region.bosses.Add(bossType);
            }
        }
        return region;
    }

    public static RegionData SetDustParticlesEnabled(this RegionData region, bool enabled)
    {
        region.dustParticlesDisabled = !enabled;
        return region;
    }

    public static RegionData SetFogEnabled(this RegionData region, bool enabled)
    {
        region.fogEnabled = enabled;
        return region;
    }

    public static RegionData SetFogAlpha(this RegionData region, float alpha)
    {
        region.fogAlpha = alpha;
        return region;
    }

    public static RegionData SetMapEmission(this RegionData region, Texture2D texture)
    {
        region.mapEmission = texture;
        return region;
    }

    public static RegionData SetMapEmissionColor(this RegionData region, Color color)
    {
        region.mapEmissionColor = color;
        return region;
    }

    public static RegionData SetSilenceCabinAmbience(this RegionData region, bool enabled)
    {
        region.silenceCabinAmbience = enabled;
        return region;
    }

    /// <summary>
    /// Sets the music loop ID for this region.
    /// </summary>
    /// <param name="id">The music ID to use.</param>
    public static RegionData SetAmbientLoopId(this RegionData region, string id)
    {
        region.ambientLoopId = id;
        return region;
    }

    public static RegionData AddFillerScenery(this RegionData region, params FillerSceneryEntry[] fillerScenery)
    {
        region.fillerScenery = region.fillerScenery ?? new();
        foreach (FillerSceneryEntry entry in fillerScenery)
        {
            region.fillerScenery.Add(entry);
        }
        return region;
    }

    public static RegionData AddScarceScenery(this RegionData region, params ScarceSceneryEntry[] scarceScenery)
    {
        region.scarceScenery = region.scarceScenery ?? new();
        foreach (ScarceSceneryEntry entry in scarceScenery)
        {
            region.scarceScenery.Add(entry);
        }
        return region;
    }

    public static RegionData SetFogProfile(this RegionData region, VolumetricFogProfile fogProfile)
    {
        region.fogProfile = fogProfile;
        return region;
    }

    public static RegionData SetMapAlbedo(this RegionData region, Texture mapAlbedo)
    {
        region.mapAlbedo = mapAlbedo;
        return region;
    }

    public static RegionData SetMapParticlesPrefabs(this RegionData region, params GameObject[] particles)
    {
        region.mapParticlesPrefabs = region.mapParticlesPrefabs ?? new();
        foreach (GameObject particle in particles)
        {
            region.mapParticlesPrefabs.Add(particle);
        }
        return region;
    }

    public static RegionData Build(this RegionData region, bool ignoreTerrainWarning = false, bool ignoreTribesWarning = false,
                                   bool ignoreEncountersWarning = false, bool ignoreBossesWarning = false)
    {

        if (!ignoreTerrainWarning && (region.terrainCards == null || region.terrainCards.Count == 0))
            InscryptionAPIPlugin.Logger.LogWarning($"Region {region.name} does not have any terrain cards!");
        if (!ignoreTribesWarning && (region.dominantTribes == null || region.dominantTribes.Count == 0))
            InscryptionAPIPlugin.Logger.LogWarning($"Region {region.name} does not have any dominant tribes!");
        if (!ignoreEncountersWarning && (region.encounters == null || region.encounters.Count == 0))
            InscryptionAPIPlugin.Logger.LogWarning($"Region {region.name} does not have any encounters!");
        if (!ignoreBossesWarning && (region.bosses == null || region.bosses.Count == 0))
            InscryptionAPIPlugin.Logger.LogWarning($"Region {region.name} does not have any bosses!");

        return region;
    }
}
