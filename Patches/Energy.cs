using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace API.Patches
{
	[HarmonyPatch(typeof(ResourceDrone), "SetOnBoard")]
	public class ResourceDrone_SetOnBoard
	{
		public static void Postfix(ResourceDrone __instance)
		{
			__instance.Gems.gameObject.SetActive(Plugin.configDroneMox.Value);
		}
	}

	[HarmonyPatch(typeof(Part1ResourcesManager), "CleanUp")]
	public class Part1ResourcesManager_CleanUp
	{
		public static void Prefix(Part1ResourcesManager __instance)
		{
			ResourcesManager baseResourceManager = (ResourcesManager)__instance;
			if (Plugin.configEnergy.Value)
			{
				var baseTraverse = Traverse.Create(baseResourceManager);
				baseTraverse.Property("PlayerEnergy").SetValue(0);
				baseTraverse.Property("PlayerMaxEnergy").SetValue(0);
			}

			if (Plugin.configDrone.Value)
			{
				Singleton<ResourceDrone>.Instance.Gems.SetAllGemsOn(false, false);
				Singleton<ResourceDrone>.Instance.CloseAllCells(false);
				Singleton<ResourceDrone>.Instance.SetOnBoard(false, false);
				if (Plugin.configDroneMox.Value)
				{
					Singleton<ResourceDrone>.Instance.Gems.SetAllGemsOn(false, false);
				}
			}

			if (Plugin.configMox.Value)
			{
				__instance.gems.Clear();
			}
		}
	}

	[HarmonyPatch(typeof(ResourcesManager), "Setup")]
	public class ResourcesManager_Setup
	{
		public static void Prefix(ResourcesManager __instance)
		{
			if (__instance is Part1ResourcesManager && Plugin.configDrone.Value)
			{
				Singleton<ResourceDrone>.Instance.SetOnBoard(true, false);
				if (Plugin.configDroneMox.Value)
				{
					Singleton<ResourceDrone>.Instance.Gems.SetAllGemsOn(false, true);
				}
			}
		}
	}

	[HarmonyPatch(typeof(ResourcesManager), "ShowAddMaxEnergy")]
	public class ResourcesManager_ShowAddMaxEnergy
	{
		public static IEnumerator Postfix(IEnumerator result, ResourcesManager __instance)
		{
			if (__instance is Part1ResourcesManager && Plugin.configDrone.Value)
			{
				Singleton<ResourceDrone>.Instance.OpenCell(__instance.PlayerMaxEnergy - 1);
				yield return new WaitForSeconds(0.4f);
			}

			yield return result;
		}
	}

	[HarmonyPatch(typeof(ResourcesManager), "ShowAddEnergy")]
	public class ResourcesManager_ShowAddEnergy
	{
		public static IEnumerator Postfix(IEnumerator result, int amount, ResourcesManager __instance)
		{
			if (__instance is Part1ResourcesManager && Plugin.configDrone.Value)
			{
				int num;
				for (int i = __instance.PlayerEnergy - amount; i < __instance.PlayerEnergy; i = num + 1)
				{
					Singleton<ResourceDrone>.Instance.SetCellOn(i, true, false);
					yield return new WaitForSeconds(0.05f);
					num = i;
				}
			}

			yield return result;
		}
	}

	[HarmonyPatch(typeof(ResourcesManager), "ShowSpendEnergy")]
	public class ResourcesManager_ShowSpendEnergy
	{
		public static IEnumerator Postfix(IEnumerator result, int amount, ResourcesManager __instance)
		{
			if (__instance is Part1ResourcesManager && Plugin.configDrone.Value)
			{
				int num;
				for (int i = __instance.PlayerEnergy + amount - 1; i >= __instance.PlayerEnergy; i = num - 1)
				{
					AudioController.Instance.PlaySound3D("crushBlip3", MixerGroup.TableObjectsSFX,
						__instance.transform.position, 0.4f, 0f,
						new AudioParams.Pitch(0.9f + (float)(__instance.PlayerEnergy + i) * 0.05f), null, null, null,
						false);
					Singleton<ResourceDrone>.Instance.SetCellOn(i, false, false);
					yield return new WaitForSeconds(0.05f);
					num = i;
				}
			}

			yield return result;
		}
	}

	[HarmonyPatch(typeof(ResourcesManager), "ShowAddGem")]
	public class ResourcesManager_ShowAddGem
	{
		public static IEnumerator Postfix(IEnumerator result, GemType gem, ResourcesManager __instance)
		{
			if (__instance is Part1ResourcesManager && Plugin.configDroneMox.Value)
			{
				__instance.SetGemOnImmediate(gem, true);
				yield return new WaitForSeconds(0.05f);
			}

			yield return result;
		}
	}

	[HarmonyPatch(typeof(ResourcesManager), "ShowLoseGem")]
	public class ResourcesManager_ShowLoseGem
	{
		public static IEnumerator Postfix(IEnumerator result, GemType gem, ResourcesManager __instance)
		{
			if (__instance is Part1ResourcesManager && Plugin.configDroneMox.Value)
			{
				__instance.SetGemOnImmediate(gem, false);
				yield return new WaitForSeconds(0.05f);
			}

			yield return result;
		}
	}

	[HarmonyPatch(typeof(ResourcesManager), "SetGemOnImmediate")]
	public class ResourcesManager_SetGemOnImmediate
	{
		public static void Postfix(GemType gem, bool on)
		{
			Singleton<ResourceDrone>.Instance.Gems.SetGemOn(gem, on, false);
		}
	}

	[HarmonyPatch]
	public class TurnManager_PlayerTurn
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			Type targetType = AccessTools.TypeByName("DiskCardGame.TurnManager+<PlayerTurn>d__73");
			return AccessTools.GetDeclaredMethods(targetType).Where(m => m.Name.Equals("MoveNext"));
		}

		static MethodInfo trigger =
			AccessTools.Method(typeof(TurnManager), "DoUpkeepPhase", new Type[] { typeof(bool) });

		public static void ILManipulator(ILContext il, MethodBase original, ILLabel retLabel)
		{
			if (Plugin.configEnergy.Value)
			{
				Type targetType = AccessTools.TypeByName("DiskCardGame.TurnManager+<PlayerTurn>d__73");
				ILCursor c = new ILCursor(il);
				c.GotoNext(inst => inst.MatchCall(AccessTools.Method(typeof(SaveManager), "get_SaveFile")));
				c.RemoveRange(6);

				c.GotoNext(inst => inst.MatchLdcI4(0));
				c.Next.OpCode = OpCodes.Ldc_I4_1;
				foreach (ILLabel branch in c.IncomingLabels)
				{
					c.GotoNext(inst => inst.MatchStfld(AccessTools.Field(targetType, "<showEnergyModule>5__2")));
					branch.Target = c.Next;
					foreach (Instruction inst in branch.Branches)
					{
						inst.OpCode = OpCodes.Br_S;
					}

					c.GotoPrev(inst => inst.MatchBr(branch));
				}

				c.Emit(OpCodes.Clt);

				c.GotoNext(inst => inst.MatchCall(AccessTools.Method(typeof(SaveManager), "get_SaveFile")));
				foreach (ILLabel branch in c.IncomingLabels)
				{
					c.GotoNext(inst => inst.MatchLdcI4(1));
					branch.Target = c.Next;
				}

				c.GotoPrev(inst => inst.MatchCall(AccessTools.Method(typeof(SaveManager), "get_SaveFile")));
				c.RemoveRange(3);
			}
		}
	}
}