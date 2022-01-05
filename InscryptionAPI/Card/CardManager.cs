using System.Diagnostics.CodeAnalysis;
using DiskCardGame;
using HarmonyLib;
using Mono.Collections.Generic;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public static class CardManager
{
    public static readonly ReadOnlyCollection<CardInfo> BaseGameCards = new(Resources.LoadAll<CardInfo>("Data/Cards"));
    private static readonly List<CardInfo> NewCards = new();

    private static long _counter = 0;
    private static long _lastBuilt = -1;
    
    public static event Action<List<CardInfo>> ModifyCardList;

    private static List<CardInfo> _allCards;

    public static List<CardInfo> AllCards
    {
        get
        {
            if (_counter != _lastBuilt)
            {
                _lastBuilt = _counter;
                _allCards = BaseGameCards.Append(NewCards).ToList();
                ModifyCardList?.Invoke(_allCards);
            }
            return _allCards;
        }
    }

    public static void Add(CardInfo newCard)
    {
        NewCards.Add(newCard);
        ++_counter;
    }
    public static void Remove(CardInfo card)
    {
        NewCards.Remove(card);
        ++_counter;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScriptableObjectLoader<UnityObject>), nameof(ScriptableObjectLoader<UnityObject>.LoadData))]
    [SuppressMessage("Member Access", "Publicizer001")]
    private static void CardLoadPrefix()
    {
        ScriptableObjectLoader<CardInfo>.allData = AllCards;
    }
}
