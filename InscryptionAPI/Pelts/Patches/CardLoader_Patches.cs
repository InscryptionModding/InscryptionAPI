using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Pelts.Patches;

[HarmonyPatch(typeof(CardLoader), "PeltNames", MethodType.Getter)]
internal class CardLoader_PeltNames
{
    /// <summary>
    /// Adds new pelts to be used everywhere in the game
    /// </summary>
    public static void Postfix(ref string[] __result)
    {
        __result = PeltManager.AllPelts().Select((a) => a.peltCardName).ToArray();
    }
}
