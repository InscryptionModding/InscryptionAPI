using System.Collections;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using UnityEngine;

namespace InscryptionCommunityPatch.ResourceManagers;

[HarmonyPatch]
public static class EnergyDrone
{
    public class EnergyConfigInfo
    {
        private bool _configEnergyOverride;
        public bool ConfigEnergy
        {
            get => PoolHasEnergy || PatchPlugin.configEnergy.Value || _configEnergyOverride;
            set => _configEnergyOverride = value;
        }

        private bool _configDroneOverride;
        public bool ConfigDrone
        {
            get => ConfigDroneMox || PatchPlugin.configDrone.Value || _configDroneOverride;
            set => _configDroneOverride = value;
        }

        private bool _configMoxOverride;
        public bool ConfigMox
        {
            get => PoolHasGems || PatchPlugin.configMox.Value || _configMoxOverride;
            set => _configMoxOverride = value;
        }

        private bool _configDroneMoxOverride;
        public bool ConfigDroneMox
        {
            get => ConfigMox || PatchPlugin.configDroneMox.Value || _configDroneMoxOverride;
            set => _configDroneMoxOverride = value;
        }
    }

    public static readonly Dictionary<CardTemple, EnergyConfigInfo> ZoneConfigs = new()
    {
        { CardTemple.Nature, new() },
        { CardTemple.Undead, new() },
        { CardTemple.Tech, new() },
        { CardTemple.Wizard, new() }
    };

    private static EnergyConfigInfo EnergyConfig
    {
        get
        {
            if (SaveManager.SaveFile.IsPart3)
                return ZoneConfigs[CardTemple.Tech];

            if (SaveManager.SaveFile.IsGrimora)
                return ZoneConfigs[CardTemple.Undead];

            if (SaveManager.SaveFile.IsMagnificus)
                return ZoneConfigs[CardTemple.Wizard];

            return ZoneConfigs[CardTemple.Nature];
        }
    }

    private static bool PoolHasEnergy { get; set; }

    private static bool PoolHasGems { get; set; }

    private static bool CardIsVisible(this CardInfo info, CardTemple targetTemple)
    {
        return info.temple == targetTemple
            // Now we check metacategories
            // If the card's metacategories are set such that it can't actually appear, don't count it
            && info.metaCategories.Exists(cat => cat is CardMetaCategory.ChoiceNode or CardMetaCategory.TraderOffer or CardMetaCategory.Rare);

    }

    internal static void TryEnableEnergy(string sceneName)
    {
        PatchPlugin.Logger.LogDebug($"Checking to see if I need to enable energy for scene {sceneName}");

        // Check the entire pool of cards for mox and energy
        CardTemple targetTemple = SaveManager.saveFile.IsGrimora
            ? CardTemple.Undead
            : SaveManager.saveFile.IsMagnificus
                ? CardTemple.Wizard
                : CardTemple.Nature;


        PoolHasEnergy = CardManager.AllCardsCopy.Exists(ci => ci.energyCost > 0 && ci.CardIsVisible(targetTemple));
        PoolHasGems = CardManager.AllCardsCopy.Exists(ci => ci.gemsCost.Count > 0 && ci.CardIsVisible(targetTemple));

        if (!SaveManager.SaveFile.IsPart3)
        {
            UnityObject.Instantiate(Resources.Load<ResourceDrone>("prefabs/cardbattle/ResourceModules"));

            if (EnergyConfig.ConfigDrone)
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

    [HarmonyPatch(typeof(ResourceDrone), nameof(ResourceDrone.SetOnBoard))]
    [HarmonyPostfix]
    public static void ResourceDrone_SetOnBoard(ResourceDrone __instance)
    {
        if (SaveManager.SaveFile.IsPart1 || SaveManager.SaveFile.IsGrimora)
        {
            // These three settings came from playing around with the UnityExplorer plugin
            __instance.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            __instance.gameObject.transform.localPosition = new Vector3(-3.038f, 7.24f, -0.42f);
            __instance.gameObject.transform.localEulerAngles = new Vector3(270.3065f, 309.2996f, 180f);

            __instance.gameObject.transform.Find("Anim").gameObject.GetComponent<Animator>().enabled = false;

            __instance.gameObject.transform.Find("Anim/Module-Energy/Propellers").gameObject.SetActive(false);

            __instance.Gems.gameObject.SetActive(EnergyConfig.ConfigDroneMox);
        }
    }

    [HarmonyPatch(typeof(Part1ResourcesManager), nameof(Part1ResourcesManager.CleanUp))]
    [HarmonyPrefix]
    public static void Part1ResourcesManager_CleanUp(Part1ResourcesManager __instance)
    {
        ResourcesManager baseResourceManager = __instance;
        if (EnergyConfig.ConfigEnergy)
        {
            baseResourceManager.PlayerEnergy = 0;
            baseResourceManager.PlayerMaxEnergy = 0;
        }

        if (EnergyConfig.ConfigDrone)
        {
            ResourceDrone.Instance.CloseAllCells();
            ResourceDrone.Instance.SetOnBoard(false);
            if (EnergyConfig.ConfigDroneMox)
            {
                ResourceDrone.Instance.Gems.SetAllGemsOn(false);
            }
        }

        if (EnergyConfig.ConfigMox)
        {
            __instance.gems.Clear();
        }
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Setup))]
    [HarmonyPrefix]
    public static void ResourcesManager_Setup(ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            PatchPlugin.Logger.LogDebug($"Setting up extra resources: drone {ResourceDrone.Instance}");
            ResourceDrone.Instance.SetOnBoard(true);
            if (EnergyConfig.ConfigDroneMox)
            {
                ResourceDrone.Instance.Gems.SetAllGemsOn(false, true);
            }
        }
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ShowAddMaxEnergy))]
    [HarmonyPostfix]
	public static IEnumerator ResourcesManager_ShowAddMaxEnergy(IEnumerator result, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            ResourceDrone.Instance.OpenCell(__instance.PlayerMaxEnergy - 1);
            yield return new WaitForSeconds(0.4f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ShowAddEnergy))]
    [HarmonyPostfix]
    public static IEnumerator ResourcesManager_ShowAddEnergy(IEnumerator result, int amount, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            int num;
            for (int i = __instance.PlayerEnergy - amount; i < __instance.PlayerEnergy; i = num + 1)
            {
                ResourceDrone.Instance.SetCellOn(i, true);
                yield return new WaitForSeconds(0.05f);
                num = i;
            }
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ShowSpendEnergy))]
    [HarmonyPostfix]
    public static IEnumerator ResourcesManager_ShowSpendEnergy(IEnumerator result, int amount, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            int num;
            for (int i = __instance.PlayerEnergy + amount - 1; i >= __instance.PlayerEnergy; i = num - 1)
            {
                AudioController.Instance.PlaySound3D(
                    "crushBlip3",
                    MixerGroup.TableObjectsSFX,
                    __instance.transform.position,
                    0.4f,
                    0f,
                    new AudioParams.Pitch(0.9f + (__instance.PlayerEnergy + i) * 0.05f)
                );
                ResourceDrone.Instance.SetCellOn(i, false);
                yield return new WaitForSeconds(0.05f);
                num = i;
            }
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ShowAddGem))]
    [HarmonyPostfix]
    public static IEnumerator ResourcesManager_ShowAddGem(IEnumerator result, GemType gem, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDroneMox)
        {
            __instance.SetGemOnImmediate(gem, true);
            yield return new WaitForSeconds(0.05f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ShowLoseGem))]
    [HarmonyPostfix]
    public static IEnumerator ResourcesManager_ShowLoseGem(IEnumerator result, GemType gem, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDroneMox)
        {
            __instance.SetGemOnImmediate(gem, false);
            yield return new WaitForSeconds(0.05f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.SetGemOnImmediate))]
    [HarmonyPostfix]
    public static void ResourcesManager_SetGemOnImmediate(GemType gem, bool on, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager)
            ResourceDrone.Instance.Gems.SetGemOn(gem, on);
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
    [HarmonyPostfix]
    public static IEnumerator TurnManager_UpkeepPhase(IEnumerator sequence, bool playerUpkeep)
    {
        // This replaces a complex IL patch
        // If the game is not going to automatically update the energy, I'll do it
        yield return sequence;

        if (SaveManager.SaveFile.IsPart1 && EnergyConfig.ConfigEnergy && playerUpkeep)
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
