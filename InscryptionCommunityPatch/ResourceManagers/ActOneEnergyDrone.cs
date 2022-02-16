using System.Collections;
using System.Reflection;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace InscryptionCommunityPatch.ResourceManagers;

[HarmonyPatch]
public static class ActOneEnergyDrone
{
    public class ActOneEnergyConfig
    {
        public bool configEnergy { get; private set; }
        public bool configDrone { get; private set; }
        public bool configMox { get; private set; }
        public bool configDroneMox { get; private set; }

        public static bool CardVisibleInActOne(CardInfo info)
        {
            if (info.temple != CardTemple.Nature) // Non-nature cards can't be selected in Act 1
                return false;

            // Now we check metacategories
            // If the card's metacategories are set such that it can't actually appear, don't count it
            return (info.metaCategories.Contains(CardMetaCategory.ChoiceNode) || 
                    info.metaCategories.Contains(CardMetaCategory.TraderOffer) || 
                    info.metaCategories.Contains(CardMetaCategory.Rare));
        }

        public ActOneEnergyConfig()
        {
            // Check the entire pool of cards for mox and energy
            bool poolHasEnergy = CardManager.AllCardsCopy.Exists(ci => ci.energyCost > 0 && CardVisibleInActOne(ci));
            bool poolHasMox = CardManager.AllCardsCopy.Exists(ci => ci.gemsCost.Count > 0 && CardVisibleInActOne(ci));

            configEnergy = poolHasEnergy || PatchPlugin.configEnergy.Value;
            configMox = poolHasMox || PatchPlugin.configMox.Value;
            configDroneMox = poolHasMox || PatchPlugin.configDroneMox.Value;
            configDrone = configDroneMox || poolHasEnergy || PatchPlugin.configDrone.Value;

            PatchPlugin.Logger.LogDebug($"Act 1 Energy Config: energy {configEnergy}, mox {configMox}, drone {configDroneMox}, drone mox {configDrone}");
        }
    }

    public static ActOneEnergyConfig EnergyConfig;

    internal static void TryEnableEnergy(string sceneName)
    {
        PatchPlugin.Logger.LogDebug($"Checking to see if I need to enable energy for scene {sceneName}");

        if (sceneName == "Part1_Cabin")
        {
            EnergyConfig = new ActOneEnergyConfig();

            UnityEngine.Object.Instantiate(Resources.Load<ResourceDrone>("prefabs/cardbattle/ResourceModules"));

            if(EnergyConfig.configDrone)
                PatchPlugin.Instance.StartCoroutine(AwakeDrone());
        }
    }

    private static IEnumerator AwakeDrone()
    {
        yield return new WaitForSeconds(1);

        PatchPlugin.Logger.LogDebug($"Awaking drone. Exists? {ResourceDrone.Instance}");

        if (ResourceDrone.Instance != null)
            ResourceDrone.Instance.Awake();

        yield return new WaitForSeconds(1);
        
        if (ResourceDrone.Instance != null)
            ResourceDrone.Instance.AttachGemsModule();
    }

    [HarmonyPatch(typeof(ResourceDrone), "SetOnBoard")]
    [HarmonyPostfix]
	public static void ResourceDrone_SetOnBoard(ResourceDrone __instance)
    {
        if (SaveManager.SaveFile.IsPart1)
        {
            // These three settings came from playing around with the UnityExplorer plugin
            __instance.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            __instance.gameObject.transform.localPosition = new Vector3(-3.038f, 7.24f, -0.42f);
            __instance.gameObject.transform.localEulerAngles = new Vector3(270.3065f, 309.2996f, 180f);

            __instance.gameObject.transform.Find("Anim").gameObject.GetComponent<Animator>().enabled = false;

            __instance.gameObject.transform.Find("Anim/Module-Energy/Propellers").gameObject.SetActive(false);

            __instance.Gems.gameObject.SetActive(EnergyConfig.configDroneMox);
        }
    }

    [HarmonyPatch(typeof(Part1ResourcesManager), "CleanUp")]
    [HarmonyPrefix]
	public static void Part1ResourcesManager_CleanUp(Part1ResourcesManager __instance)
    {
        ResourcesManager baseResourceManager = (ResourcesManager)__instance;
        if (EnergyConfig.configEnergy)
        {
            baseResourceManager.PlayerEnergy = 0;
            baseResourceManager.PlayerMaxEnergy = 0;
        }

        if (EnergyConfig.configDrone)
        {
            ResourceDrone.Instance.CloseAllCells(false);
            ResourceDrone.Instance.SetOnBoard(false, false);
            if (EnergyConfig.configDroneMox)
            {
                ResourceDrone.Instance.Gems.SetAllGemsOn(false, false);
            }
        }

        if (EnergyConfig.configMox)
        {
            __instance.gems.Clear();
        }
    }

    [HarmonyPatch(typeof(ResourcesManager), "Setup")]
    [HarmonyPrefix]
	public static void ResourcesManager_Setup(ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.configDrone)
        {
            PatchPlugin.Logger.LogDebug($"Setting up extra resources: drone {ResourceDrone.Instance}");
            ResourceDrone.Instance.SetOnBoard(true, false);
            if (EnergyConfig.configDroneMox)
            {
                ResourceDrone.Instance.Gems.SetAllGemsOn(false, true);
            }
        }
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowAddMaxEnergy")]
    [HarmonyPostfix]
	public static IEnumerator ResourcesManager_ShowAddMaxEnergy(IEnumerator result, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.configDrone)
        {
            ResourceDrone.Instance.OpenCell(__instance.PlayerMaxEnergy - 1);
            yield return new WaitForSeconds(0.4f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowAddEnergy")]
    [HarmonyPostfix]
	public static IEnumerator ResourcesManager_ShowAddEnergy(IEnumerator result, int amount, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.configDrone)
        {
            int num;
            for (int i = __instance.PlayerEnergy - amount; i < __instance.PlayerEnergy; i = num + 1)
            {
                ResourceDrone.Instance.SetCellOn(i, true, false);
                yield return new WaitForSeconds(0.05f);
                num = i;
            }
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowSpendEnergy")]
    [HarmonyPostfix]
	public static IEnumerator ResourcesManager_ShowSpendEnergy(IEnumerator result, int amount, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.configDrone)
        {
            int num;
            for (int i = __instance.PlayerEnergy + amount - 1; i >= __instance.PlayerEnergy; i = num - 1)
            {
                AudioController.Instance.PlaySound3D("crushBlip3", MixerGroup.TableObjectsSFX,
                    __instance.transform.position, 0.4f, 0f,
                    new AudioParams.Pitch(0.9f + (float)(__instance.PlayerEnergy + i) * 0.05f), null, null, null,
                    false);
                ResourceDrone.Instance.SetCellOn(i, false, false);
                yield return new WaitForSeconds(0.05f);
                num = i;
            }
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowAddGem")]
    [HarmonyPostfix]
	public static IEnumerator ResourcesManager_ShowAddGem(IEnumerator result, GemType gem, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.configDroneMox)
        {
            __instance.SetGemOnImmediate(gem, true);
            yield return new WaitForSeconds(0.05f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowLoseGem")]
    [HarmonyPostfix]
	public static IEnumerator ResourcesManager_ShowLoseGem(IEnumerator result, GemType gem, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.configDroneMox)
        {
            __instance.SetGemOnImmediate(gem, false);
            yield return new WaitForSeconds(0.05f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "SetGemOnImmediate")]
    [HarmonyPostfix]
	public static void ResourcesManager_SetGemOnImmediate(GemType gem, bool on, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager)
            ResourceDrone.Instance.Gems.SetGemOn(gem, on, false);
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
    [HarmonyPostfix]
    public static IEnumerator TurnManager_UpkeepPhase(IEnumerator sequence, bool playerUpkeep)
    {
        // This replaces a complex IL patch
        // If the game is not going to automatically update the energy, I'll do it
        yield return sequence;

        if (SaveManager.SaveFile.IsPart1 && EnergyConfig.configEnergy && playerUpkeep)
        {
            bool showEnergyModule = !ResourcesManager.Instance.EnergyAtMax || ResourcesManager.Instance.PlayerEnergy < ResourcesManager.Instance.PlayerMaxEnergy;
            if (showEnergyModule)
            {
                ViewManager.Instance.SwitchToView(View.Default, false, true);
			    yield return new WaitForSeconds(0.1f);
            }

            yield return ResourcesManager.Instance.AddMaxEnergy(1);
		    yield return ResourcesManager.Instance.RefreshEnergy();

            if (showEnergyModule)
            {
                yield return new WaitForSeconds(0.25f);
			    Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
            }
        }
	}
}