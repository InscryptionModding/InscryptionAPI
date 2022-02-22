using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class CardManager
{
    // Needs to be defined first so the implicit static constructor works correctly
    private class CardExt
    {
        public readonly Dictionary<Type, object> TypeMap = new();
        public readonly Dictionary<string, string> StringMap = new();
    }
    private static readonly ConditionalWeakTable<CardInfo, CardExt> ExtensionProperties = new();

    public static readonly ReadOnlyCollection<CardInfo> BaseGameCards = new(GetBaseGameCards().ToList());
    private static readonly ObservableCollection<CardInfo> NewCards = new();
    
    public static event Func<List<CardInfo>, List<CardInfo>> ModifyCardList;

    private static IEnumerable<CardInfo> GetBaseGameCards()
    {
        foreach (CardInfo card in Resources.LoadAll<CardInfo>("Data/Cards"))
        {
            card.SetBaseGameCard(true);
            yield return card;
        }
    }

    public static void SyncCardList()
    {
        InscryptionAPIPlugin.Logger.LogDebug($"Synchronizing card list");
        var cards = BaseGameCards.Concat(NewCards).Select(x => CardLoader.Clone(x)).ToList();
        AllCardsCopy = ModifyCardList?.Invoke(cards) ?? cards;
    }

    private static string GetCardPrefixFromName(this CardInfo info)
    {
        string[] splitName = info.name.Split('_');
        return splitName.Length > 1 ? splitName[0] : string.Empty;
    }

    private static void AddPrefixesToCards(IEnumerable<CardInfo> cards, string prefix)
    {
        foreach (CardInfo card in cards)
        {
            if (string.IsNullOrEmpty(card.GetModPrefix()))
            {
                card.SetModPrefix(prefix);

                if (!card.name.StartsWith($"{prefix}_"))
                    card.name = $"{prefix}_{card.name}";
            }
        }
    }

    internal static void ResolveMissingModPrefixes()
    {
        InscryptionAPIPlugin.Logger.LogDebug($"Resolving mod prefixes");

        // Group all cards by the mod guid
        foreach (var group in NewCards.Where(ci => !ci.IsBaseGameCard()).GroupBy(ci => ci.GetModTag(), ci => ci, (key, g) => new { ModId = key, Cards = g.ToList() }))
        {
            InscryptionAPIPlugin.Logger.LogDebug($"Working on mod prefix for {group.ModId}");

            if (group.ModId.Equals("MADH.inscryption.JSONLoader", StringComparison.OrdinalIgnoreCase))
            {
                // This is a special case, but this is the most logical way to handle this

                // We will trust JSON loader to handle its own prefixes
                // This is mainly because it's completely impossible to derive prefixes from JSON loader
                // JSON loader handles loading cards from potentialy dozens of mods.
                // We will be completely unable to observe any sort of pattern from cards loaded by JSON loader
                continue;
            }

            // Get list of unique mod prefixes
            List<string> setPrefixes = group.Cards.Select(ci => ci.GetModPrefix()).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

            // If there is EXACTLY ONE prefix in the group, we can apply it to the rest of the group
            if (setPrefixes.Count == 1)
            {
                AddPrefixesToCards(group.Cards, setPrefixes[0]);                
                continue;
            }

            // We won't try to dynamically generate prefixes unless the count of cards in the group is at least 6
            if (group.Cards.Count < 6)
                continue;

            // Okay, let's try to derive prefixes from card names
            bool appliedPrefixes = false;
            foreach (var nameGroup in group.Cards.Select(ci => ci.GetCardPrefixFromName())
                                                 .GroupBy(s => s)
                                                 .Select(g => new { Prefix = g.Key, Count = g.Count() })
                                                 .ToList())
            {
                if (nameGroup.Count >= group.Cards.Count / 2)
                {
                    AddPrefixesToCards(group.Cards, nameGroup.Prefix);
                    appliedPrefixes = true;
                }

                if (appliedPrefixes)
                    break;
            }

            if (appliedPrefixes)
                continue;

            // Okay, we can't get it from card names
            // We build it from the mod guid
            string[] guidSplit = group.ModId.Split('.');
            string prefix = guidSplit[guidSplit.Length - 1];
            AddPrefixesToCards(group.Cards, prefix);
        }
    }

    static CardManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(CardInfo))
            {
                ScriptableObjectLoader<CardInfo>.allData = AllCardsCopy;
            }
        };
        NewCards.CollectionChanged += static (_, _) =>
        {
            SyncCardList();
        };
    }

    public static List<CardInfo> AllCardsCopy { get; private set; } = BaseGameCards.ToList();

    public static void Add(CardInfo newCard, string modPrefix=default(string)) 
    { 
        newCard.name = !string.IsNullOrEmpty(modPrefix) && !newCard.name.StartsWith(modPrefix) ? $"{modPrefix}_{newCard.name}" : newCard.name;

        newCard.SetModPrefix(modPrefix);
        newCard.SetModTag(TypeManager.GetModIdFromCallstack(Assembly.GetCallingAssembly()));

        if (!NewCards.Contains(newCard)) 
            NewCards.Add(newCard); 
    }
    public static void Remove(CardInfo card) => NewCards.Remove(card);

    public static CardInfo New(string name, string displayName, int attack, int health, string description = default(string), string modPrefix=default(string))
    {
        CardInfo retval = ScriptableObject.CreateInstance<CardInfo>();
        retval.name = !string.IsNullOrEmpty(modPrefix) && !name.StartsWith(modPrefix) ? $"{modPrefix}_{name}" :  name;
        retval.SetBasic(displayName, attack, health, description);

        Add(retval, modPrefix:modPrefix);

        return retval;
    }
    
    /// <summary>
    /// Get a custom extension class that will exist on all clones of a card
    /// </summary>
    /// <param name="card">Card to access</param>
    /// <typeparam name="T">The custom class</typeparam>
    /// <returns>The instance of T for this card</returns>
    public static T GetExtendedClass<T>(this CardInfo card) where T : class, new()
    {
        var typeMap = ExtensionProperties.GetOrCreateValue(card).TypeMap;
        if (typeMap.TryGetValue(typeof(T), out object tObj))
        {
            return (T)tObj;
        }
        else
        {
            T tInst = new();
            typeMap[typeof(T)] = tInst;
            return tInst;
        }
    }

    internal static Dictionary<string, string> GetCardExtensionTable(this CardInfo card)
    {
        return ExtensionProperties.GetOrCreateValue(card).StringMap;
    }

    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.Clone))]
    [HarmonyPrefix]
    private static bool ClonePrefix(CardInfo __instance, ref object __result)
    {
        // so, this patch actually does two things.
        // first, it fixes clone to *actually* clone with scriptableobject, not memberwise
        // then it ensures that every clone has the same CardExt attached to it
        CardInfo ret = ScriptableObject.Instantiate<CardInfo>(__instance);
        ret.name = __instance.name;
        ExtensionProperties.Add(ret as CardInfo, ExtensionProperties.GetOrCreateValue(__instance));
        ret.Mods = new List<CardModificationInfo>();
        __result = ret;
        return false;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    private static void SyncCardsAndAbilitiesWhenTransitioningToAscensionGame()
    {
        CardManager.SyncCardList();
        AbilityManager.SyncAbilityList();
    }

    [HarmonyPatch(typeof(MenuController), nameof(MenuController.TransitionToGame))]
    [HarmonyPrefix]
    private static void SyncCardsAndAbilitiesWhenTransitioningToGame()
    {
        CardManager.SyncCardList();
        AbilityManager.SyncAbilityList();
    }

    [HarmonyPatch(typeof(CardLoader), nameof(CardLoader.GetCardByName))]
    [HarmonyPrefix]
    private static bool GetNonGuidName(string name, ref CardInfo __result)
    {
        CardInfo retval = CardManager.AllCardsCopy.FirstOrDefault(ci => ci.name == name);
        if (retval == null)
            retval = CardManager.AllCardsCopy.FirstOrDefault(ci => ci.name.EndsWith($"_{name}"));
        return CardLoader.Clone(retval);
    }
}