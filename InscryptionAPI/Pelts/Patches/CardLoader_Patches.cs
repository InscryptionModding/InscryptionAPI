using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Pelts;

[HarmonyPatch(typeof(CardLoader), "PeltNames", MethodType.Getter)]
public class CardLoader_PeltNames
{
    /// <summary>
    /// Adds new pelts to be used everywhere in the game
    /// </summary>
    public static void Postfix(ref string[] __result)
    {
        List<string> list = new(__result);
        list.AddRange(PeltManager.AllNewPelts.Select((a) => a.peltCardName));
        __result = list.ToArray();
    }
}
