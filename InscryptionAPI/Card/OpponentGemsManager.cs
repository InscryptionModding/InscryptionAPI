using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
internal class OpponentGemifyPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.Setup))]
    [HarmonyPatch(typeof(PixelResourcesManager), nameof(PixelResourcesManager.Setup))]
    [HarmonyPatch(typeof(Part3ResourcesManager), nameof(Part3ResourcesManager.Setup))]
    private static void SetUpOpponentResources(ResourcesManager __instance)
    {
        if (__instance.GetComponent<OpponentGemsManager>() == null)
            __instance.gameObject.AddComponent<OpponentGemsManager>();
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
    private static void CleanUpOpponentResources()
    {
        if (Singleton<OpponentGemsManager>.Instance)
            Singleton<OpponentGemsManager>.Instance.opponentGems.Clear();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.ForceGemsUpdate))]
    private static void ForceGemsUpdate()
    {
        if (Singleton<OpponentGemsManager>.Instance)
            Singleton<OpponentGemsManager>.Instance.ForceGemsUpdate();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FishHookGrab), nameof(FishHookGrab.PullHook))]
    [HarmonyPatch(typeof(FishHookItem), nameof(FishHookItem.OnValidTargetSelected))]
    private static void UpdateGemsOnGrab()
    {
        Singleton<ResourcesManager>.Instance.ForceGemsUpdate();
    }

    [HarmonyPatch(typeof(GainGem))]
    private static class GainGemPatches
    {
        
        [HarmonyPostfix, HarmonyPatch(nameof(GainGem.OnResolveOnBoard))]
        private static IEnumerator GainGemsForOpponents(IEnumerator enumerator, GainGem __instance)
        {
            yield return enumerator;
            if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance != null)
                Singleton<OpponentGemsManager>.Instance.AddGem(__instance.Gem);
        }
        [HarmonyPostfix, HarmonyPatch(nameof(GainGem.OnDie))]
        private static IEnumerator LoseGemsForOpponents(IEnumerator enumerator, GainGem __instance)
        {
            yield return enumerator;
            if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance != null)
                Singleton<OpponentGemsManager>.Instance.LoseGem(__instance.Gem);
        }
    }
    [HarmonyPatch(typeof(GainGemTriple))]
    private static class GainGemTriplePatches
    {
        [HarmonyPostfix, HarmonyPatch(nameof(GainGemTriple.OnResolveOnBoard))]
        private static IEnumerator GainTripleGemsForOpponents(IEnumerator enumerator, GainGemTriple __instance)
        {
            yield return enumerator;
            if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance != null)
                Singleton<OpponentGemsManager>.Instance.AddGems(GemType.Green, GemType.Orange, GemType.Blue);
        }
        [HarmonyPostfix, HarmonyPatch(nameof(GainGemTriple.OnDie))]
        private static IEnumerator LoseTripleGemsForOpponents(IEnumerator enumerator, GainGemTriple __instance)
        {
            yield return enumerator;
            if (__instance.Card.OpponentCard && Singleton<OpponentGemsManager>.Instance != null)
                Singleton<OpponentGemsManager>.Instance.LoseGems(GemType.Green, GemType.Orange, GemType.Blue);
        }
    }
}

public class OpponentGemsManager : Singleton<OpponentGemsManager>
{
    public List<GemType> opponentGems = new();

    public bool HasGem(GemType gem) => opponentGems.Contains(gem);
    public void AddGem(GemType gem) => opponentGems.Add(gem);
    public void LoseGem(GemType gem) => opponentGems.Remove(gem);
    public void AddGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
            opponentGems.Add(gem);
    }
    public void LoseGems(params GemType[] gems)
    {
        foreach (GemType gem in gems)
            opponentGems.Remove(gem);
    }

    public void ForceGemsUpdate()
    {
        opponentGems.Clear();
        foreach (CardSlot slot in Singleton<BoardManager>.Instance.GetSlots(getPlayerSlots: false))
        {
            if (slot.Card != null && !slot.Card.Dead)
            {
                GainGem[] components = slot.Card.GetComponents<GainGem>();
                foreach (GainGem gainGem in components)
                    opponentGems.Add(gainGem.Gem);

                GainGemTriple[] components2 = slot.Card.GetComponents<GainGemTriple>();
                for (int i = 0; i < components2.Length; i++)
                {
                    _ = components2[i];
                    opponentGems.Add(GemType.Green);
                    opponentGems.Add(GemType.Orange);
                    opponentGems.Add(GemType.Blue);
                }
            }
        }
    }
}