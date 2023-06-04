using DiskCardGame;
using HarmonyLib;
using Sirenix.Serialization.Utilities;
using System.Runtime.CompilerServices;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class CardModificationInfoManager
{
    // Needs to be defined first so the implicit static constructor works correctly
    private class CardModExt
    {
        public readonly Dictionary<Type, object> TypeMap = new();
        public readonly Dictionary<string, string> StringMap = new();
    }
    private static readonly ConditionalWeakTable<CardModificationInfo, CardModExt> CardModExtensionProperties = new();

    /// <summary>
    /// Get a custom extension class that will exist on all clones of a card
    /// </summary>
    /// <param name="card">Card to access</param>
    /// <typeparam name="T">The custom class</typeparam>
    /// <returns>The instance of T for this card</returns>
    public static T GetExtendedClass<T>(this CardModificationInfo card) where T : class, new()
    {
        var typeMap = CardModExtensionProperties.GetOrCreateValue(card).TypeMap;
        if (typeMap.TryGetValue(typeof(T), out object tObj))
            return (T)tObj;

        else
        {
            T tInst = new();
            typeMap[typeof(T)] = tInst;
            return tInst;
        }
    }

    internal static Dictionary<string, string> GetCardModExtensionTable(this CardModificationInfo card)
    {
        return CardModExtensionProperties.GetOrCreateValue(card).StringMap;
    }
    public static void SyncCardMods()
    {
        // Re-add persistent properties
        foreach (CardInfo card in CardManager.AllCardsCopy)
        {
            foreach (CardModificationInfo mod in card.Mods.Where(x => x.HasCustomPropertiesId()))
            {
                foreach (string property in GetCustomPropertiesFromId(mod.singletonId))
                {
                    string[] split = property.Split(',');
                    mod.SetExtendedProperty(split[0], split[1]);
                }
            }
            foreach (CardModificationInfo mod in card.Mods.Where(x => x.HasCustomCostsId()))
            {
                foreach (string pair in GetCustomCostsFromId(mod.singletonId))
                {
                    string[] splitCost = pair.Split(',');
                    card.SetExtendedProperty(splitCost[0], splitCost[1]);
                }
            }
        }
    }
    public static List<string> GetCustomCostsFromId(string singletonId)
    {
        List<string> costList = new();
        if (singletonId.Contains("[CustomCosts:"))
        {
            int startIndex = singletonId.IndexOf("[CustomCosts:");
            string customCosts = singletonId.Substring(startIndex).Replace("[CustomCosts:", "");
            if (customCosts.Contains("]"))
            {
                int endIndex = customCosts.IndexOf("]");
                customCosts = customCosts.Substring(0, endIndex).Replace("]", "");

                customCosts.Split(';').ForEach(x => costList.Add(x));
            }
        }
        return costList;
    }
    public static List<string> GetCustomPropertiesFromId(string singletonId)
    {
        List<string> costList = new();
        if (singletonId.Contains("[Properties:"))
        {
            int startIndex = singletonId.IndexOf("[Properties:");
            string customCosts = singletonId.Substring(startIndex).Replace("[Properties:", "");
            if (customCosts.Contains("]"))
            {
                int endIndex = customCosts.IndexOf("]");
                customCosts = customCosts.Substring(0, endIndex).Replace("]", "");

                customCosts.Split(';').ForEach(x => costList.Add(x));
            }
        }
        return costList;
    }
    [HarmonyPatch(typeof(CardModificationInfo), nameof(CardModificationInfo.Clone))]
    [HarmonyPostfix]
    private static void ClonePostfix(CardModificationInfo __instance, ref object __result)
    {
        // just ensures that clones of a card mod have the same extension properties
        CardModExtensionProperties.Add((CardModificationInfo)__result, CardModExtensionProperties.GetOrCreateValue(__instance));
    }

    [HarmonyPatch(typeof(DiskCardGame.Card), nameof(DiskCardGame.Card.SetInfo))]
    [HarmonyPrefix]
    private static void AddCustomCostsFromMods(CardInfo info)
    {
        foreach (CardModificationInfo mod in info.Mods.Where(x => x.HasCustomCostsId()))
        {
            foreach (string pair in GetCustomCostsFromId(mod.singletonId))
            {
                string[] splitCost = pair.Split(',');
                info.SetExtendedProperty(splitCost[0], splitCost[1]);
            }
        }
    }
}
