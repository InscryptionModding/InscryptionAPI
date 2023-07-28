using DiskCardGame;
using HarmonyLib;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch]
internal class RandomConsumablePatch
{
    [HarmonyPatch(typeof(RandomConsumable), nameof(RandomConsumable.RespondsToResolveOnBoard))]
    [HarmonyPostfix]
    private static void DisableInAct2(ref bool __result)
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            PatchPlugin.Logger.LogInfo("Trinket Bearer is disabled in Act 2.");
            __result = false;
        }
    }
}