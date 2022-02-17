using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class CardManager
{
    public static readonly ReadOnlyCollection<CardInfo> BaseGameCards = new(Resources.LoadAll<CardInfo>("Data/Cards"));
    private static readonly ObservableCollection<CardInfo> NewCards = new();
    
    public static event Func<List<CardInfo>, List<CardInfo>> ModifyCardList;

    public static void SyncCardList()
    {
        var cards = BaseGameCards.Concat(NewCards).Select(x => CardLoader.Clone(x)).ToList();
        //var cards = BaseGameCards.Concat(NewCards).ToList();
        AllCardsCopy = ModifyCardList?.Invoke(cards) ?? cards;
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

    public static void Add(CardInfo newCard) { if (!NewCards.Contains(newCard)) NewCards.Add(newCard); }
    public static void Remove(CardInfo card) => NewCards.Remove(card);

    public static CardInfo New(string name, string displayName, int attack, int health, string description = default(string), bool addToPool = true)
    {
        CardInfo retval = ScriptableObject.CreateInstance<CardInfo>();
        retval.name = name;
        retval.SetBasic(displayName, attack, health, description);

        // Go ahead and add this as well
        if (addToPool)
            Add(retval);

        return retval;
    }

    private class CardExt
    {
        public readonly Dictionary<Type, object> TypeMap = new();
        public readonly Dictionary<string, string> StringMap = new();
    }
    private static readonly ConditionalWeakTable<CardInfo, CardExt> ExtensionProperties = new();
    
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
    private static bool ClonePrefix(CardInfo c, out object __result)
    {
        // so, this patch actually does two things.
        // first, it fixes clone to *actually* clone with scriptableobject, not memberwise
        // then it ensures that every clone has the same CardExt attached to it
        CardInfo ret = CardInfo.Instantiate(c);
        ExtensionProperties.Add(ret, ExtensionProperties.GetOrCreateValue(c));
        __result = ret;
        return false;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
    [HarmonyPrefix]
    public static void SyncCardsAndAbilitiesWhenTransitioningToAscensionGame()
    {
        CardManager.SyncCardList();
        AbilityManager.SyncAbilityList();
    }

    [HarmonyPatch(typeof(MenuController), nameof(MenuController.TransitionToGame))]
    [HarmonyPrefix]
    public static void SyncCardsAndAbilitiesWhenTransitioningToGame()
    {
        CardManager.SyncCardList();
        AbilityManager.SyncAbilityList();
    }
}