using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;
using static DiskCardGame.EncounterBlueprintData;

namespace APIPlugin
{
	public class NewEncounter
	{
		public static List<NewEncounter> encounters = new List<NewEncounter>();
		public string name, regionName;
		public bool regular;
		public bool bossPrep;
		public EncounterBlueprintData encounterBlueprintData;

		public NewEncounter(string name, EncounterBlueprintData encounterBlueprintData, string regionName, bool regular, bool bossPrep)
		{
			if (bossPrep && NewEncounter.encounters.Exists(x => (x.regionName == regionName && x.bossPrep)))
			{
				NewEncounter encounter = NewEncounter.encounters.Find(x => (x.regionName == regionName && x.bossPrep));
				Plugin.Log.LogError($"Region {regionName} already has a custom encounter: {encounter.name}. It will be overridden by {name}!");
				NewEncounter.encounters.Remove(encounter);
			}
			this.name = name;
			this.encounterBlueprintData = encounterBlueprintData;
			this.regionName = regionName;
			this.bossPrep = bossPrep;
			this.regular = regular;
			NewEncounter.encounters.Add(this);
			Plugin.Log.LogInfo($"Added custom encounter {name} to region {regionName}!");
		}

		public static void Add(
			string name, string regionName, List<TurnModBlueprint> turnMods = null, List<Tribe> dominantTribes = null,
			List<Ability> redundantAbilities = null, List<CardInfo> unlockedCardPrerequisites = null, bool regionSpecific = true,
			int minDifficulty = 0, int maxDifficulty = 30, List<CardInfo> randomReplacementCards = null,
			List<List<CardBlueprint>> turns = null, bool regular = true, bool bossPrep = false, int oldPreviewDifficulty = 0)
        {
			EncounterBlueprintData encounter = ScriptableObject.CreateInstance<EncounterBlueprintData>();

			if (turnMods is not null)
            {
				encounter.turnMods = turnMods;
            }

			if (dominantTribes is not null)
            {
				encounter.dominantTribes = dominantTribes;
            }
			
			if (redundantAbilities is not null)
            {
				encounter.redundantAbilities = redundantAbilities;
            }

			if (unlockedCardPrerequisites is not null)
            {
				encounter.unlockedCardPrerequisites = unlockedCardPrerequisites;
            }

			encounter.regionSpecific = regionSpecific;
			encounter.minDifficulty = minDifficulty;
			encounter.maxDifficulty = maxDifficulty;

			if (randomReplacementCards is not null)
            {
				encounter.randomReplacementCards = randomReplacementCards;
            }

			if (turns is not null)
            {
				encounter.turns = turns;
            }

			encounter.oldPreviewDifficulty = oldPreviewDifficulty;
			new NewEncounter(name, encounter, regionName, regular, bossPrep);
		}
	}
}
