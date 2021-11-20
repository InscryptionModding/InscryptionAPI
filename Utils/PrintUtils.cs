using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public class PrintUtils
	{
		// Print section
		public static void PrintStatIconInfo(StatIconInfo info)
		{
			Plugin.Log.LogInfo($"\nStatIconInfo");
			Plugin.Log.LogInfo($"GBC Description [{info.gbcDescription}]");
			Plugin.Log.LogInfo($"Rulebook Name [{info.rulebookName}]");
			Plugin.Log.LogInfo($"Rulebook Description [{info.rulebookDescription}]");
			Plugin.Log.LogInfo($"Applies to Attack [{info.appliesToAttack}] Health [{info.appliesToAttack}]");
			PrintList("MetaCategory", info.metaCategories);
		}

		public static void PrintAllCardInfo()
		{
			foreach (var info in CardLoader.AllData)
			{
				PrintCardInfo(info);
			}
		}

		public static void PrintCardModInfoList(List<CardModificationInfo> mods)
		{
			if (mods.Count == 0) return;

			Plugin.Log.LogInfo("Card Mods Info");
			foreach (var mod in mods)
			{
				PrintCardModInfo(mod);
			}
		}

		public static void PrintCardModInfo(CardModificationInfo mod)
		{
			Plugin.Log.LogInfo($"ID [{mod.singletonId}]");
			Plugin.Log.LogInfo($"Gemify? [{mod.gemify}]");
			Plugin.Log.LogInfo($"Attack Adjustment [{mod.attackAdjustment}]");
			Plugin.Log.LogInfo($"Health Adjustment [{mod.healthAdjustment}]");
			Plugin.Log.LogInfo($"Blood Cost Adjustment [{mod.bloodCostAdjustment}]");
			Plugin.Log.LogInfo($"Bones Cost Adjustment [{mod.bonesCostAdjustment}]");
			Plugin.Log.LogInfo($"Energy Cost Adjustment [{mod.energyCostAdjustment}]");
			Plugin.Log.LogInfo($"Non-Copyable? [{mod.nonCopyable}]");
			Plugin.Log.LogInfo($"From Card Merge? [{mod.fromCardMerge}]");
			Plugin.Log.LogInfo($"From Duplicate Merge? [{mod.fromDuplicateMerge}]");
			Plugin.Log.LogInfo($"From Latch? [{mod.fromLatch}]");
			Plugin.Log.LogInfo($"From Overclock? [{mod.fromOverclock}]");
			Plugin.Log.LogInfo($"From Totem? [{mod.fromTotem}]");
			Plugin.Log.LogInfo($"Nullify Gems Cost? [{mod.nullifyGemsCost}]");
			Plugin.Log.LogInfo($"Side Deck Mod? [{mod.sideDeckMod}]");
			Plugin.Log.LogInfo($"Special Stat Icon [{mod.statIcon}]");
			Plugin.Log.LogInfo($"Remove On upkeep? [{mod.RemoveOnUpkeep}]");

			PrintList("Abilities", mod.abilities);
			PrintList("Negate Abilities", mod.negateAbilities);
			PrintList("Special Abilities", mod.specialAbilities);
			PrintList("Add Gem Costs", mod.addGemCost);
			PrintList("DecalIds", mod.decalIds);
		}

		public static void PrintList<T>(string title, List<T> items)
		{
			if (items.Count == 0) return;
			Plugin.Log.LogInfo(title);
			items.ForEach(item => Plugin.Log.LogInfo($"-> {item}"));
		}

		public static void PrintCardInfo(CardInfo info)
		{
			Plugin.Log.LogInfo($"===============");
			Plugin.Log.LogInfo($"Name [{info.name}]");
			Plugin.Log.LogInfo($"Displayed Name [{info.displayedName}]");
			Plugin.Log.LogInfo($"Description [{info.description}]");
			Plugin.Log.LogInfo($"Attack [{info.baseAttack}] Health [{info.baseHealth}]");
			Plugin.Log.LogInfo($"Boon [{info.boon}]");
			Plugin.Log.LogInfo($"Bones Cost [{info.bonesCost}]");
			Plugin.Log.LogInfo($"Cost [{info.cost}]");
			Plugin.Log.LogInfo($"Temple [{info.temple}]");
			Plugin.Log.LogInfo($"CardComplexity [{info.cardComplexity}]");
			Plugin.Log.LogInfo($"EnergyCost [{info.energyCost}]");
			if (info.evolveParams is not null)
				Plugin.Log.LogInfo(
					$"EvolveParams = Turns to evolve [{info.evolveParams.turnsToEvolve}] Evolves into [{info.evolveParams.evolution.name}]");
			Plugin.Log.LogInfo($"Can sacrifice? [{info.Sacrificable}]");
			Plugin.Log.LogInfo($"Is Gemified? [{info.Gemified}]");
			if (info.iceCubeParams is not null)
				Plugin.Log.LogInfo($"IceCubeParams = [{info.iceCubeParams.creatureWithin.name}]");
			Plugin.Log.LogInfo($"SpecialStatIcon [{info.specialStatIcon}]");
			Plugin.Log.LogInfo($"One per deck? [{info.onePerDeck}]");
			// TODO: possible to get an NRE if the power level is not set?
			// Plugin.Log.LogInfo($"Power Level [{info.PowerLevel}]");
			PrintList("Abilities", info.Abilities);
			PrintList("Special Abilities", info.SpecialAbilities);
			PrintList("Appearance Behavior", info.appearanceBehaviour);
			PrintList("Gems Cost", info.GemsCost);
			PrintList("MetaCategory", info.metaCategories);
			PrintList("Traits", info.traits);
			PrintList("Tribes", info.tribes);

			PrintCardModInfoList(info.mods);

			Plugin.Log.LogInfo($"===============\n");
		}
	}
}