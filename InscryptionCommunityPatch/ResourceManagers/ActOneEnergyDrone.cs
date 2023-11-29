using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using Pixelplacement;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InscryptionCommunityPatch.ResourceManagers;

[HarmonyPatch]
public static class EnergyDrone
{
    public class EnergyConfigInfo
    {
        public bool ConfigEnergy => PoolHasEnergy || PatchPlugin.configEnergy.Value;
        public bool ConfigDrone => PoolHasEnergy || ConfigDroneMox || PatchPlugin.configDrone.Value;
        public bool ConfigDefaultDrone => PatchPlugin.configDefaultDrone.Value;
        public bool ConfigMox => PoolHasGems || PatchPlugin.configMox.Value;
        public bool ConfigDroneMox => PoolHasGems || PatchPlugin.configDroneMox.Value;
    }

    public static bool SceneCanHaveEnergyDrone(string sceneName)
    {
        string activeSceneName = sceneName.ToLowerInvariant();
        return (activeSceneName.Contains("part1") && !activeSceneName.Contains("sanctum")) || activeSceneName.Contains("magnificus") || activeSceneName.Contains("grimora");
    }

    public static bool CurrentSceneCanHaveEnergyDrone
    {
        get
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene == null || string.IsNullOrEmpty(activeScene.name))
                return false;

            return SceneCanHaveEnergyDrone(activeScene.name);
        }
    }

    public static Dictionary<CardTemple, EnergyConfigInfo> ZoneConfigs = new()
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

    public static bool PoolHasEnergy { get; private set; }
    public static bool PoolHasGems { get; private set; }

    private static bool CardIsVisible(this CardInfo info, CardTemple targetTemple)
    {
        // if the CardInfo can't appear in this part of the game - Act 1 = Nature, Grimora = Undead, Magnificus = Wizard
        if (info.temple != targetTemple)
            return false;

        // If the card's metacategories are set such that it can't actually appear, don't count it
        return info.HasAnyOfCardMetaCategories(CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare);
    }

    internal static void TryEnableEnergy(string sceneName)
    {
        // We only need to do this in these specific scenes.
        if (!SceneCanHaveEnergyDrone(sceneName))
            return;

        // Check the entire pool of cards for mox and energy
        CardTemple targetTemple = SaveManager.SaveFile.IsGrimora ? CardTemple.Undead :
                                  SaveManager.SaveFile.IsMagnificus ? CardTemple.Wizard :
                                  CardTemple.Nature;

        PoolHasEnergy = CardManager.AllCardsCopy.Any(ci => ci.energyCost > 0 && ci.CardIsVisible(targetTemple));
        PoolHasGems = CardManager.AllCardsCopy.Any(ci => ci.gemsCost.Count > 0 && ci.CardIsVisible(targetTemple));

        PatchPlugin.Logger.LogDebug($"Card pool has Energy cards? {PoolHasEnergy}. Card pool has Gem cards? {PoolHasGems}.");

        ResourceDrone drone = UnityObject.Instantiate(Resources.Load<ResourceDrone>("prefabs/cardbattle/ResourceModules"));

        if (EnergyConfig.ConfigDrone)
            PatchPlugin.Instance.StartCoroutine(AwakeDrone());
    }

    private static void AttachDroneToScale()
    {
        try
        {
            // disable the animations and propeller models
            ResourceDrone.Instance.transform.Find("Anim").gameObject.GetComponent<Animator>().enabled = false;
            ResourceDrone.Instance.transform.Find("Anim/Module-Energy/Propellers").gameObject.SetActive(false);

            // parent it to the scale's model
            Transform scale = LifeManager.Instance.Scales3D.gameObject.transform.Find("Scale");

            ResourceDrone.Instance.transform.SetParent(scale);
            ResourceDrone.Instance.transform.localEulerAngles = Vector3.zero;
            ResourceDrone.Instance.transform.localScale = new(0.6f, 0.6f, 0.6f);
            ResourceDrone.Instance.transform.localPosition = new(0.02f, -0.2f, 2f);
            ResourceDrone.Instance.boardPosition = scale.position; // update this just in case
        }
        catch { PatchPlugin.Logger.LogError("Couldn't attach ResourceDrone to LifeManager.Scale3D!"); }
    }
    private static IEnumerator AwakeDrone()
    {
        yield return new WaitForSeconds(1f);

        if (PatchPlugin.configFullDebug.Value)
            PatchPlugin.Logger.LogDebug($"Awaking ResourceDrone, instance exists? {ResourceDrone.Instance != null}.");

        ResourceDrone.Instance?.Awake();
        yield return new WaitForSeconds(1f);
        ResourceDrone.Instance?.AttachGemsModule();

        // not sure if the drone can be null in this method, but we'll check just in case
        if (ResourceDrone.m_Instance != null)
        {
            if (EnergyConfig.ConfigDefaultDrone)
                ResourceDrone.Instance.gameObject.transform.localPosition = ResourceDrone.Instance.boardPosition + Vector3.up * 5f;
            else
                AttachDroneToScale();
        }
    }

    [HarmonyPatch(typeof(ResourceDrone), nameof(ResourceDrone.SetOnBoard))]
    [HarmonyPrefix]
    private static bool Act1ResourceDrone_SetOnBoard(ResourceDrone __instance, bool onBoard, bool immediate)
    {
        // return default logic if not using the custom drone
        if (!CurrentSceneCanHaveEnergyDrone)
            return true;

        __instance.Gems.gameObject.SetActive(EnergyConfig.ConfigDroneMox);
        if (onBoard)
        {
            __instance.gameObject.SetActive(true);
            __instance.SetAllCellsOn(on: false);
        }

        if (EnergyConfig.ConfigDefaultDrone)
        {
            Vector3 vector = __instance.boardPosition + (onBoard ? Vector3.zero : (Vector3.up * 5f));
            if (immediate)
            {
                __instance.gameObject.transform.position = vector;
                if (!onBoard)
                    __instance.gameObject.SetActive(false);
            }
            else
            {
                Tween.Position(__instance.gameObject.transform, vector, 1.5f, 0f, Tween.EaseInOut, default, null, delegate
                {
                    if (onBoard)
                        Tween.Shake(__instance.gameObject.transform, __instance.gameObject.transform.localPosition, Vector3.one * 0.15f, 0.15f, 0f);
                    __instance.gameObject.SetActive(onBoard);
                });
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(Part1ResourcesManager), nameof(Part1ResourcesManager.CleanUp))]
    [HarmonyPrefix]
    private static void Part1ResourcesManager_CleanUp(Part1ResourcesManager __instance)
    {
        ResourcesManager baseResourceManager = __instance;
        if (EnergyConfig.ConfigEnergy)
        {
            baseResourceManager.PlayerEnergy = 0;
            baseResourceManager.PlayerMaxEnergy = 0;
        }

        if (EnergyConfig.ConfigDrone && ResourceDrone.m_Instance != null)
        {
            ResourceDrone.Instance.CloseAllCells(false);
            ResourceDrone.Instance.SetOnBoard(false, false);
            if (EnergyConfig.ConfigDroneMox)
                ResourceDrone.Instance.Gems.SetAllGemsOn(false, false);
        }
        if (EnergyConfig.ConfigMox)
            __instance.gems.Clear();
    }

    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Setup))]
    [HarmonyPrefix]
    private static void ResourcesManager_Setup(ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            ResourceDrone.Instance?.SetOnBoard(true, false);
            if (EnergyConfig.ConfigDroneMox)
            {
                PatchPlugin.Logger.LogDebug("Setting up extra resources for the drone.");
                ResourceDrone.Instance?.Gems.SetAllGemsOn(false, true);
            }
        }
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowAddMaxEnergy")]
    [HarmonyPostfix]
    private static IEnumerator ResourcesManager_ShowAddMaxEnergy(IEnumerator result, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            int cellsToOpen = __instance.PlayerMaxEnergy - 1;
            ResourceDrone.Instance?.OpenCell(cellsToOpen);
            yield return new WaitForSeconds(0.4f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowAddEnergy")]
    [HarmonyPostfix]
    private static IEnumerator ResourcesManager_ShowAddEnergy(IEnumerator result, int amount, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            int num;
            for (int i = __instance.PlayerEnergy - amount; i < __instance.PlayerEnergy; i = num + 1)
            {
                ResourceDrone.Instance?.SetCellOn(i, true, false);
                yield return new WaitForSeconds(0.05f);
                num = i;
            }
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowSpendEnergy")]
    [HarmonyPostfix]
    private static IEnumerator ResourcesManager_ShowSpendEnergy(IEnumerator result, int amount, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            int num;
            for (int i = __instance.PlayerEnergy + amount - 1; i >= __instance.PlayerEnergy; i = num - 1)
            {
                AudioController.Instance.PlaySound3D("crushBlip3", MixerGroup.TableObjectsSFX,
                    __instance.transform.position, 0.4f, 0f,
                    new AudioParams.Pitch(0.9f + (float)(__instance.PlayerEnergy + i) * 0.05f), null, null, null,
                    false);
                ResourceDrone.Instance?.SetCellOn(i, false, false);
                yield return new WaitForSeconds(0.05f);
                num = i;
            }
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowAddGem")]
    [HarmonyPostfix]
    private static IEnumerator ResourcesManager_ShowAddGem(IEnumerator result, GemType gem, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDroneMox)
        {
            __instance.SetGemOnImmediate(gem, true);
            yield return new WaitForSeconds(0.05f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowLoseGem")]
    [HarmonyPostfix]
    private static IEnumerator ResourcesManager_ShowLoseGem(IEnumerator result, GemType gem, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDroneMox)
        {
            __instance.SetGemOnImmediate(gem, false);
            yield return new WaitForSeconds(0.05f);
        }

        yield return result;
    }

    [HarmonyPatch(typeof(ResourcesManager), "SetGemOnImmediate")]
    [HarmonyPostfix]
    private static void ResourcesManager_SetGemOnImmediate(GemType gem, bool on, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager)
            ResourceDrone.Instance.Gems?.SetGemOn(gem, on, false);
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
    [HarmonyPostfix]
    private static IEnumerator TurnManager_UpkeepPhase(IEnumerator sequence, bool playerUpkeep)
    {
        // This replaces a complex IL patch
        // If the game is not going to automatically update the energy, I'll do it
        yield return sequence;

        if (CurrentSceneCanHaveEnergyDrone && EnergyConfig.ConfigEnergy && playerUpkeep)
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
