using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InscryptionAPI.Ascension;

/// <summary>
/// This manager class handles the creation and management of mod-added starter decks.
/// </summary>
[HarmonyPatch]
public static class StarterDeckManager {

    /// <summary>
    /// A utility class that holds all of the required information about a starter deck in order to be able to use it in-game.
    /// </summary>
    public class FullStarterDeck
    {
        /// <summary>
        /// The description object for this starter deck.
        /// </summary>   
        public StarterDeckInfo Info { get; set; }
        /// <summary>
        /// The challenge level this starter deck becomes available.
        /// </summary>   
        public int UnlockLevel { get; set; }

        /// <summary>
        /// A function that needs to return true for the starter deck to be unlocked. Optional. If this isn't set, this check will be bypassed. The int argument is the current Kaycee's Mod challenge level.
        /// </summary>
        public Func<int, bool> CustomUnlockCheck { get; set; }
        /// <summary>
        /// The internal names of the cards contained in this starter deck.
        /// </summary>
        public List<string> CardNames { get; set; } // For delayed loading of actual card names
    }

    /// <summary>
    /// All of the vanilla game's starter decks.
    /// </summary>
    public static readonly ReadOnlyCollection<FullStarterDeck> BaseGameDecks = new(GetBaseGameDecks());

    internal static readonly ObservableCollection<FullStarterDeck> NewDecks = new();

    /// <summary>
    /// The current processed list of all starter decks in the game, including vanilla and mod-added starter decks.
    /// </summary>
    public static List<FullStarterDeck> AllDecks { get; private set; } = BaseGameDecks.ToList();
    /// <summary>
    /// The current processed list of all <see cref="StarterDeckInfo"/> objects in the game, including vanilla and mod-added starter decks.
    /// </summary>
    public static List<StarterDeckInfo> AllDeckInfos { get; private set; } = BaseGameDecks.Select(fsd => fsd.Info).ToList();

    private static List<FullStarterDeck> GetBaseGameDecks()
    {
        List<StarterDeckInfo> decks = new(Resources.LoadAll<StarterDeckInfo>("Data/Ascension/StarterDecks"));

        GameObject screen = Resources.Load<GameObject>("prefabs/ui/ascension/AscensionStarterDeckScreen");
        Transform iconContainer = screen.transform.Find("Icons");
        List<FullStarterDeck> retval = new();
        for (int i = 1; i <= 8; i++)
        {
            AscensionStarterDeckIcon icon = iconContainer.Find($"StarterDeckIcon_{i}").gameObject.GetComponent<AscensionStarterDeckIcon>();

            FullStarterDeck fdeck = new() { Info = decks.First(d => d.name == icon.Info.name), UnlockLevel = -1 };
            fdeck.CardNames = fdeck.Info.cards.Select(ci => ci.name).ToList();
            retval.Add(fdeck);
        }
        return retval;
    }

    private static FullStarterDeck CloneStarterDeck(FullStarterDeck info)
    {
        FullStarterDeck retval = new();
        StarterDeckInfo deckInfo = ScriptableObject.CreateInstance<StarterDeckInfo>();
        deckInfo.cards = info.Info.cards == null ? null : new List<CardInfo>(info.Info.cards);
        deckInfo.iconSprite = info.Info.iconSprite;
        deckInfo.name = info.Info.name;
        deckInfo.title = info.Info.title;
        retval.Info = deckInfo;
        retval.CardNames = new(info.CardNames);
        retval.CustomUnlockCheck = info.CustomUnlockCheck;
        retval.UnlockLevel = info.UnlockLevel;
        return retval;
    }

    /// <summary>
    /// A hook for modders to add custom code to modify the starter deck list dynamically.
    /// </summary>
    public static event Func<List<FullStarterDeck>, List<FullStarterDeck>> ModifyDeckList;

    /// <summary>
    /// Resynchronizes the starter deck list.
    /// </summary>
    public static void SyncDeckList()
    {
        var decks = BaseGameDecks.Concat(NewDecks).Select(CloneStarterDeck).ToList();

        foreach (var deck in decks)
        {
            void UpdateCardsList(List<CardInfo> allcards)
            {
                List<CardInfo> cards = new();
                foreach (var c in deck.CardNames)
                {
                    if (allcards.Exists(x => x.name == c))
                    {
                        cards.Add(CardLoader.Clone(allcards.Find(x => x.name == c)));
                    }
                }
                deck.Info.cards = cards;
            }
            UpdateCardsList(CardLoader.AllData);
            if (deck.Info.cards.Count != deck.CardNames.Count)
            {
                List<CardInfo> TryAddMissingCards(List<CardInfo> x)
                {
                    UpdateCardsList(x);
                    if (deck.Info.cards.Count == deck.CardNames.Count)
                    {
                        CardManager.ModifyCardList -= TryAddMissingCards;
                    }
                    return x;
                }
                CardManager.ModifyCardList += TryAddMissingCards;
            }
        }

        AllDecks = ModifyDeckList?.Invoke(decks) ?? decks;
        AllDeckInfos = AllDecks.Select(fsd => fsd.Info).ToList();
    }

    static StarterDeckManager()
    {
        InscryptionAPIPlugin.ScriptableObjectLoaderLoad += static type =>
        {
            if (type == typeof(StarterDeckInfo))
            {
                ScriptableObjectLoader<StarterDeckInfo>.allData = AllDecks.Select(fsd => fsd.Info).ToList();
            }
        };
        NewDecks.CollectionChanged += static (_, _) =>
        {
            SyncDeckList();
        };
    }

    /// <summary>
    /// Creates a new starter deck using the given <see cref="StarterDeckInfo"/> and registers it with the API.
    /// </summary>
    /// <param name="pluginGuid">The GUID of the mod adding this starter deck.</param>
    /// <param name="info">The <see cref="StarterDeckInfo"/> for this starter deck.</param>
    /// <param name="unlockLevel">The minimum challenge level for this starter deck to become available.</param>
    /// <returns>The <see cref="FullStarterDeck"/> object containing this starter deck's information.</returns>
    public static FullStarterDeck Add(string pluginGuid, StarterDeckInfo info, int unlockLevel = 0)
    {
        info.name = pluginGuid + "_" + (info.title ?? info.name);

        FullStarterDeck fsd = new();
        fsd.Info = info;
        fsd.UnlockLevel = unlockLevel;
        fsd.CardNames = info.cards.Select(ci => ci.name).ToList();

        if (NewDecks.FirstOrDefault(sdi => sdi.Info.name == info.name) == null)
            NewDecks.Add(fsd);

        return fsd;
    }

    /// <summary>
    /// Creates a new starter deck and registers it with the API.
    /// </summary>
    /// <param name="pluginGuid">The GUID of the mod adding this starter deck.</param>
    /// <param name="title">The title and internal name of this starter deck.</param>
    /// <param name="iconTexture">The icon texture to display for this starter deck in the deck selection screen.</param>
    /// <param name="cardNames">The internal names of the cards contained in this starter deck.</param>
    /// <param name="unlockLevel">The minimum challenge level for this starter deck to become available.</param>
    /// <returns>The <see cref="FullStarterDeck"/> object containing this starter deck's information.</returns>
    public static FullStarterDeck New(string pluginGuid, string title, Texture2D iconTexture, string[] cardNames, int unlockLevel = 0)
    {
        StarterDeckInfo info = ScriptableObject.CreateInstance<StarterDeckInfo>();
        info.title = title;
        info.iconSprite = TextureHelper.ConvertTexture(iconTexture, TextureHelper.SpriteType.StarterDeckIcon);
        info.name = pluginGuid + "_" + title;

        FullStarterDeck fsd = new();
        fsd.Info = info;
        fsd.UnlockLevel = unlockLevel;
        fsd.CardNames = cardNames.ToList();

        NewDecks.Add(fsd);

        return fsd;
    }

    /// <summary>
    /// Variant of <see cref="New(string, string, Texture2D, string[], int)"/> that allows for a separate internal name to be defined rather than using the title.
    /// </summary>
    /// <param name="pluginGuid">The GUID of the mod adding this starter deck.</param>
    /// <param name="name">The internal name of this starter deck.</param>
    /// <param name="title">The title/name of this starter deck.</param>
    /// <param name="iconTexture">The icon texture to display for this starter deck in the deck selection screen.</param>
    /// <param name="cardNames">The internal names of the cards contained in this starter deck.</param>
    /// <param name="unlockLevel">The minimum challenge level for this starter deck to become available.</param>
    /// <returns>The <see cref="FullStarterDeck"/> object containing this starter deck's information.</returns>
    public static FullStarterDeck New(string pluginGuid, string name, string title, Texture2D iconTexture, string[] cardNames, int unlockLevel = 0)
    {
        StarterDeckInfo info = ScriptableObject.CreateInstance<StarterDeckInfo>();
        info.title = title;
        info.iconSprite = TextureHelper.ConvertTexture(iconTexture, TextureHelper.SpriteType.StarterDeckIcon);
        info.name = pluginGuid + "_" + name;

        FullStarterDeck fsd = new();
        fsd.Info = info;
        fsd.UnlockLevel = unlockLevel;
        fsd.CardNames = cardNames.ToList();

        NewDecks.Add(fsd);

        return fsd;
    }

    /// <summary>
    /// Creates a new starter deck and registers it with the API.
    /// </summary>
    /// <param name="pluginGuid">The GUID of the mod adding this starter deck.</param>
    /// <param name="title">The title/name of this starter deck.</param>
    /// <param name="pathToIconTexture">The file path for the icon texture to display for this starter deck in the deck selection screen (relative to the Plugins folder).</param>
    /// <param name="cardNames">The internal names of the cards contained in this starter deck.</param>
    /// <param name="unlockLevel">The minimum challenge level for this starter deck to become available.</param>
    /// <returns>The <see cref="FullStarterDeck"/> object containing this starter deck's information.</returns>
    public static FullStarterDeck New(string pluginGuid, string title, string pathToIconTexture, string[] cardNames, int unlockLevel = 0)
    {
        Texture2D texture = TextureHelper.GetImageAsTexture(pathToIconTexture);
        return New(pluginGuid, title, texture, cardNames, unlockLevel);
    }

    /// <summary>
    /// Variant of <see cref="New(string, string, string, string[], int)"/> that allows for a separate internal name to be defined rather than using the title.
    /// </summary>
    /// <param name="pluginGuid">The GUID of the mod adding this starter deck.</param>
    /// <param name="name">The internal name of this starter deck.</param>
    /// <param name="title">The title/name of this starter deck.</param>
    /// <param name="pathToIconTexture">The file path for the icon texture to display for this starter deck in the deck selection screen (relative to the Plugins folder).</param>
    /// <param name="cardNames">The internal names of the cards contained in this starter deck.</param>
    /// <param name="unlockLevel">The minimum challenge level for this starter deck to become available.</param>
    /// <returns>The <see cref="FullStarterDeck"/> object containing this starter deck's information.</returns>
    public static FullStarterDeck New(string pluginGuid, string name, string title, string pathToIconTexture, string[] cardNames, int unlockLevel = 0)
    {
        Texture2D texture = TextureHelper.GetImageAsTexture(pathToIconTexture);
        return New(pluginGuid, name, title, texture, cardNames, unlockLevel);
    }

    [HarmonyPatch(typeof(AscensionUnlockSchedule), "StarterDeckIsUnlockedForLevel")]
    [HarmonyPrefix]
    private static bool CustomStarterLevel(ref bool __result, string id, int level)
    {
        FullStarterDeck fsd = AllDecks.FirstOrDefault(d => d.Info.name == id);
        if (fsd == null || (fsd.UnlockLevel < 0 && fsd.CustomUnlockCheck == null))
            return true;

        __result = level >= fsd.UnlockLevel && (fsd.CustomUnlockCheck == null || fsd.CustomUnlockCheck(level));
        return false;
    }
}