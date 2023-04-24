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

        if (activeSceneName.Contains("part1") ||
            activeSceneName.Contains("magnificus") ||
            activeSceneName.Contains("grimora"))
            return true;

        return false;
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
        if (info.temple != targetTemple) // Non-nature cards can't be selected in Act 1
            return false;

        // Now we check metacategories
        // If the card's metacategories are set such that it can't actually appear, don't count it
        return info.metaCategories.Exists((CardMetaCategory x) =>
        x == CardMetaCategory.ChoiceNode || x == CardMetaCategory.TraderOffer || x == CardMetaCategory.Rare);
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

        UnityObject.Instantiate(Resources.Load<ResourceDrone>("prefabs/cardbattle/ResourceModules"));

        if (EnergyConfig.ConfigDrone)
            PatchPlugin.Instance.StartCoroutine(AwakeDrone());
    }

    private static IEnumerator AwakeDrone()
    {
        yield return new WaitForSeconds(1);

        if (PatchPlugin.configFullDebug.Value)
            PatchPlugin.Logger.LogDebug($"Awaking ResourceDrone, instance exists? {ResourceDrone.Instance != null}.");

        if (ResourceDrone.Instance != null)
            ResourceDrone.Instance.Awake();

        yield return new WaitForSeconds(1);

        if (ResourceDrone.Instance != null)
            ResourceDrone.Instance.AttachGemsModule();
    }

    [HarmonyPatch(typeof(ResourceDrone), "SetOnBoard")]
    [HarmonyPostfix]
    private static void ResourceDrone_SetOnBoard(ResourceDrone __instance, bool onBoard)
    {
        // These settings came from playing around with the UnityExplorer plugin
        if (CurrentSceneCanHaveEnergyDrone)
        {
            __instance.Gems.gameObject.SetActive(EnergyConfig.ConfigDroneMox);
            if (!EnergyConfig.ConfigDefaultDrone)
            {
                // disable the animation and propellers
                __instance.gameObject.transform.Find("Anim").gameObject.GetComponent<Animator>().enabled = false;
                __instance.gameObject.transform.Find("Anim/Module-Energy/Propellers").gameObject.SetActive(false);

                // scale it down and angle it with the scales

                __instance.gameObject.transform.localEulerAngles = new Vector3(270.3f, 309.3f, 180f);
                __instance.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }

            if (onBoard)
            {
                __instance.gameObject.transform.localPosition = EnergyConfig.ConfigDefaultDrone
                    ? __instance.boardPosition + Vector3.up * 5f
                    : new Vector3(-9f, 8f, 0f);

                __instance.gameObject.SetActive(value: true);
                __instance.SetAllCellsOn(on: false);
            }

            Vector3 vector = EnergyConfig.ConfigDefaultDrone
                ? __instance.boardPosition + (onBoard ? Vector3.zero : (Vector3.up * 5f))
                : (onBoard ? new(-3.15f, 7.2f, -0.1f) : new(-7f, 7.8f, -0.2f));

            float duration = EnergyConfig.ConfigDefaultDrone ? 1.5f : (onBoard ? 0.157f : 0.27f);
            float delay = (EnergyConfig.ConfigDefaultDrone  || onBoard) ? 0f : 0.255f;
            AnimationCurve easeCurve = (EnergyConfig.ConfigDefaultDrone || onBoard) ? Tween.EaseInOut : Tween.EaseOut;

            Tween.Position(__instance.gameObject.transform, vector, duration, delay,
                easeCurve, Tween.LoopType.None, null, delegate
                {
                    if (EnergyConfig.ConfigDefaultDrone && onBoard)
                        Tween.Shake(__instance.gameObject.transform, __instance.gameObject.transform.localPosition, Vector3.one * 0.15f, 0.15f, 0f);
                    __instance.gameObject.SetActive(onBoard);
                });

        }
    }

    [HarmonyPatch(typeof(Part1ResourcesManager), "CleanUp")]
    [HarmonyPrefix]
    private static void Part1ResourcesManager_CleanUp(Part1ResourcesManager __instance)
    {
        ResourcesManager baseResourceManager = __instance;
        if (EnergyConfig.ConfigEnergy)
        {
            baseResourceManager.PlayerEnergy = 0;
            baseResourceManager.PlayerMaxEnergy = 0;
        }

        if (EnergyConfig.ConfigDrone)
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
        if (EnergyConfig.ConfigDrone)
            PatchPlugin.Logger.LogDebug("Setting up extra resources for the drone.");

        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            ResourceDrone.Instance.SetOnBoard(true, false);
            if (EnergyConfig.ConfigDroneMox)
                ResourceDrone.Instance.Gems.SetAllGemsOn(false, true);
        }
    }

    [HarmonyPatch(typeof(ResourcesManager), "ShowAddMaxEnergy")]
    [HarmonyPostfix]
    private static IEnumerator ResourcesManager_ShowAddMaxEnergy(IEnumerator result, ResourcesManager __instance)
    {
        if (__instance is Part1ResourcesManager && EnergyConfig.ConfigDrone)
        {
            ResourceDrone.Instance.OpenCell(__instance.PlayerMaxEnergy - 1);
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
                ResourceDrone.Instance.SetCellOn(i, true, false);
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
                ResourceDrone.Instance.SetCellOn(i, false, false);
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
            ResourceDrone.Instance.Gems.SetGemOn(gem, on, false);
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