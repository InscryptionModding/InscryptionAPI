using APIPlugin;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine;
using VolumetricFogAndMist;

namespace InscryptionAPI.Regions;

/// <summary>
/// Utility class that adds extensions to aid in creating and modifying <see cref="RegionData"/>.
/// </summary>
public static class RegionExtensions
{
    /// <summary>
    /// If the created region will be added to the pool of regions that can appear in a run.
    /// </summary>
    /// <param name="data">The <see cref="RegionData"/> for the region we are modifying.</param>
    /// <param name="canAppear">If the created region will be added to the pool of regions that can appear in a run.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetCanAppearRandomly(this RegionData data, bool canAppear) {
        Part1RegionData region = RegionManager.NewRegions.FirstOrDefault(x => x.Region == data);
        if (region != null) {
            region.CanAppearRandomly = canAppear;
        }
        else {
            InscryptionAPIPlugin.Logger.LogWarning($"Could not find custom region for RegionData [{data.name}]!");
        }
        return data;
    }

    /// <summary>
    /// Retrieves the <see cref="RegionData"/> with the given name.
    /// </summary>
    /// <param name="regions">Collection of <see cref="RegionData"/> objects to search for.</param>
    /// <param name="name">Region name to look for.</param>
    /// <returns>The <see cref="RegionData"/> with the given name, or null if one couldn't be found.</returns>
    public static RegionData RegionByName(this IEnumerable<RegionData> regions, string name) => regions.FirstOrDefault(x => x.name == name);

    /// <summary>
    /// Sets the given <see cref="RegionData"/>'s internal name.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="name">The name of the region.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetName(this RegionData region, string name)
    {
        region.name = name;
        return region;
    }

    /// <summary>
    /// Adds terrain cards to this region.<br/>
    /// Terrain cards require the <see cref="Terrain"/> trait to appear.<br/>
    /// Every region must have at least one valid terrain card.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="cards">The terrain cards to add.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
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
    /// Adds cards that are more likely to appear in this region's choice nodes.<br/>
    /// Likely cards still require the <see cref="CardMetaCategory.ChoiceNode"/> metacategory.<br/>
    /// One of three normal card choices is guaranteed to contain a card from the region's likely cards.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="cards">The likely cards to add.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
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
    /// This only applies to consumables that are <see cref="ConsumableItemData.regionSpecific"/>.
    /// Adding non-region-specific consumables will increase the probability of the consumable appearing.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="consumables">Names of items to appear in this region.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData AddConsumableItems(this RegionData region, params string[] consumables)
    {
        region.consumableItems = region.consumableItems ?? new();
        foreach (string consumable in consumables)
        {
            region.consumableItems.Add(ItemsUtil.GetConsumableByName(consumable));
        }
        return region;
    }

    /// <summary>
    /// Adds consumables to this region.<br/>
    /// This only applies to consumables that are <see cref="ConsumableItemData.regionSpecific"/>.
    /// Adding non-region-specific consumables will increase the probability of the consumable appearing.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="consumables">Names of items to appear in this region.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
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
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="tribes">The tribes to add.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData AddDominantTribes(this RegionData region, params Tribe[] tribes)
    {
        region.dominantTribes = region.dominantTribes ?? new();
        foreach (Tribe tribe in tribes)
        {
            region.dominantTribes.Add(tribe);
        }
        return region;
    }

    /// <summary>
    /// Sets this region's board lighting colour.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="color">The colour to set the board lighting to.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetBoardColor(this RegionData region, Color color)
    {
        region.boardLightColor = color;
        return region;
    }

    /// <summary>
    /// Sets this region's card lighting colour.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="color">The colour to set the card lighting to.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetCardsColor(this RegionData region, Color color)
    {
        region.cardsLightColor = color;
        return region;
    }

    /// <summary>
    /// Creates a new encounter for this region and returns the builder.<br/>
    /// Every region with battles needs at least one encounter.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="name">The name for the encounter.</param>
    /// <returns>The created encounter data.</returns>
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
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="encounters">The encounters to add.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
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
    /// Sets the boss prep encounter condition for this region.
    /// If this condition is not met, the 'boss prep encounter' will not appear.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="condition">The condition that needs to be fulfilled.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetBossPrepCondition(this RegionData region, StoryEventCondition condition)
    {
        region.bossPrepCondition = condition;
        return region;
    }

    /// <summary>
    /// Sets the boss prep encounter for this region.
    /// The boss prep encounter is the final battle node in the region, before the boss.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="encounter">The encounter to set.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetBossPrepEncounter(this RegionData region, EncounterBlueprintData encounter)
    {
        region.bossPrepEncounter = encounter;
        return region;
    }

    /// <summary>
    /// Adds bosses that can appear in this region.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="bosses">The IDs for all bosses that can appear in this region.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
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

    /// <summary>
    /// Sets whether or not dust particles should appear on the map while in this region.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="enabled">If dust particles should appear while in this region.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetDustParticlesEnabled(this RegionData region, bool enabled)
    {
        region.dustParticlesDisabled = !enabled;
        return region;
    }

    /// <summary>
    /// Sets whether or not fog should appear on the map while in this region.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="enabled">If fog should appear while in this region.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetFogEnabled(this RegionData region, bool enabled)
    {
        region.fogEnabled = enabled;
        return region;
    }

    /// <summary>
    /// Sets the fog's alpha value.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="alpha">How opaque/transparent the fog should be. Value ranges from [0.0, 1.0]</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetFogAlpha(this RegionData region, float alpha)
    {
        region.fogAlpha = alpha;
        return region;
    }

    /// <summary>
    /// Sets the emission texture for this region's map object.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="texture">The <see cref="Texture"/> to use for the map emission.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetMapEmission(this RegionData region, Texture texture) {
        region.mapEmission = texture;
        return region;
    }

    /// <summary>
    /// Sets the emission texture for this region's map object.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="texture">The <see cref="Texture2D"/> to use for the map emission.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetMapEmission(this RegionData region, Texture2D texture)
    {
        region.mapEmission = texture;
        return region;
    }

    /// <summary>
    /// Sets the map emission's colour.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="color">The colour to use for the map emission.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetMapEmissionColor(this RegionData region, Color color)
    {
        region.mapEmissionColor = color;
        return region;
    }

    /// <summary>
    /// Sets if this region should have no ambience.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="enabled">If the ambient music should be replaced with silence.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetSilenceCabinAmbience(this RegionData region, bool enabled)
    {
        region.silenceCabinAmbience = enabled;
        return region;
    }

    /// <summary>
    /// Sets the music loop ID for this region.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="id">The music ID to use.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetAmbientLoopId(this RegionData region, string id)
    {
        region.ambientLoopId = id;
        return region;
    }

    /// <summary>
    /// Sets the data used for this region's map's filler scenery objects.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="fillerScenery">The data for determining what objects are used to fill the map's filler scenery.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData AddFillerScenery(this RegionData region, params FillerSceneryEntry[] fillerScenery)
    {
        region.fillerScenery = region.fillerScenery ?? new();
        foreach (FillerSceneryEntry entry in fillerScenery)
        {
            region.fillerScenery.Add(entry);
        }
        return region;
    }

    /// <summary>
    /// Sets the data used for this region's map's scarce scenery objects.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="scarceScenery">The data for determining what objects are used to fill the map's scarce scenery.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData AddScarceScenery(this RegionData region, params ScarceSceneryEntry[] scarceScenery)
    {
        region.scarceScenery = region.scarceScenery ?? new();
        foreach (ScarceSceneryEntry entry in scarceScenery)
        {
            region.scarceScenery.Add(entry);
        }
        return region;
    }

    /// <summary>
    /// Sets the data used for how map fog should appear in this region.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="fogProfile">The description object that controls how the fog appears.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetFogProfile(this RegionData region, VolumetricFogProfile fogProfile)
    {
        region.fogProfile = fogProfile;
        return region;
    }

    /// <summary>
    /// Sets the albedo texture for this region's map.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="mapAlbedo">The texture used for the map's albedo..</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetMapAlbedo(this RegionData region, Texture mapAlbedo)
    {
        region.mapAlbedo = mapAlbedo;
        return region;
    }

    /// <summary>
    /// Sets the prefab objects for this particles on this region's map.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="particles">The objects used for the particles on the map.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData SetMapParticlesPrefabs(this RegionData region, params GameObject[] particles)
    {
        region.mapParticlesPrefabs = region.mapParticlesPrefabs ?? new();
        foreach (GameObject particle in particles)
        {
            region.mapParticlesPrefabs.Add(particle);
        }
        return region;
    }

    /// <summary>
    /// Validates the region and outputs console warnings if it's missing any important data.
    /// </summary>
    /// <param name="region">The <see cref="RegionData"/> to access.</param>
    /// <param name="ignoreTerrainWarning">Whether or not the API should output a console warning when this region has no terrain cards.</param>
    /// <param name="ignoreTribesWarning">Whether or not the API should output a console warning when this region has no dominant tribes.</param>
    /// <param name="ignoreEncountersWarning">Whether or not the API should output a console warning when this region has no encounters.</param>
    /// <param name="ignoreBossesWarning">Whether or not the API should output a console warning when this region has no bosses.</param>
    /// <returns>The same <see cref="RegionData"/> so a chain can continue.</returns>
    public static RegionData Build(this RegionData region,
        bool ignoreTerrainWarning = false, bool ignoreTribesWarning = false,
        bool ignoreEncountersWarning = false, bool ignoreBossesWarning = false) {
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
