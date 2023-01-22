using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

namespace InscryptionAPI.Card;

/// <summary>
/// This class handles the addition of new tribes into the game
/// </summary>
/// <remarks>This manager can currently handle watermarking cards with tribes and having 
/// them appear at tribal choice nodes. Totems are not currently supported.</remarks>
[HarmonyPatch]
public class TribeManager
{
    private static readonly List<TribeInfo> tribes = new();
    private static readonly List<Tribe> tribeTypes = new();
    public static readonly ReadOnlyCollection<TribeInfo> NewTribes = new(tribes);
    public static readonly ReadOnlyCollection<Tribe> NewTribesTypes = new(tribeTypes);

    private static Texture2D TribeIconMissing = TextureHelper.GetImageAsTexture("tribeicon_none.png", Assembly.GetExecutingAssembly());

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.UpdateTribeIcon))]
    [HarmonyPostfix]
    private static void UpdateTribeIcon(CardDisplayer3D __instance, CardInfo info)
    {
        if (info != null)
        {
            foreach (TribeInfo tribe in tribes)
            {
                if (tribe?.icon != null)
                {
                    if (info.IsOfTribe(tribe.tribe))
                    {
                        bool foundSpriteRenderer = false;
                        foreach (SpriteRenderer spriteRenderer in __instance.tribeIconRenderers)
                        {
                            if (spriteRenderer.sprite == null)
                            {
                                foundSpriteRenderer = true;
                                spriteRenderer.sprite = tribe.icon;
                                break;
                            }
                        }
                        if (!foundSpriteRenderer)
                        {
                            SpriteRenderer last = __instance.tribeIconRenderers.Last();
                            SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate(last);
                            spriteRenderer.transform.parent = last.transform.parent;
                            spriteRenderer.transform.localPosition = last.transform.localPosition + (__instance.tribeIconRenderers[1].transform.localPosition - __instance.tribeIconRenderers[0].transform.localPosition);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.GetCardbackTexture))]
    [HarmonyPostfix]
    private static void GetCardbackTexture(ref Texture __result, CardChoice choice)
    {
        if (choice != null && choice.tribe != Tribe.None && __result == null)
        {
            __result = tribes?.Find((x) => x != null && x.tribe == choice.tribe)?.cardback;
        }
    }

    [HarmonyPatch(typeof(Part1CardChoiceGenerator), nameof(Part1CardChoiceGenerator.GenerateTribeChoices))]
    [HarmonyPrefix]
    private static bool GenerateTribeChoices(ref List<CardChoice> __result, int randomSeed)
    {
        // create list of chooseable vanilla tribes then add all chooseable custom tribes
        List<Tribe> list = new()
        {
            Tribe.Bird,
            Tribe.Canine,
            Tribe.Hooved,
            Tribe.Insect,
            Tribe.Reptile
        };
        list.AddRange(TribeManager.tribes.FindAll((x) => x != null && x.tribeChoice).ConvertAll((x) => x.tribe));
        // create a list of this region's dominant tribes
        List<Tribe> tribes = new(RunState.CurrentMapRegion.dominantTribes);
        // get a list of cards obtainable at choice nodes
        List<CardInfo> obtainableCards = CardManager.AllCardsCopy.FindAll(c => c.HasCardMetaCategory(CardMetaCategory.ChoiceNode));
        // remove all non-chooseable tribes and all tribes with no cards
        tribes.RemoveAll(t => (TribeManager.tribes.Exists(ct => ct.tribe == t && !ct.tribeChoice)) || !obtainableCards.Exists(c => c.IsOfTribe(t)));
        list.RemoveAll(t => tribes.Contains(t) || !obtainableCards.Exists(c => c.IsOfTribe(t)));
        // if list is empty, add Insect as a fallback
        if (list.Count == 0)
            list.Add(Tribe.Insect);

        while (tribes.Count < 3)
        {
            Tribe item = list[SeededRandom.Range(0, list.Count, randomSeed++)];
            tribes.Add(item);
            if (list.Count > 1) // prevents softlock
                list.Remove(item);
        }
        while (tribes.Count > 3) // if there are more than 3 tribes, reduce it to 3
            tribes.RemoveAt(SeededRandom.Range(0, tribes.Count, randomSeed++));

        List<CardChoice> list2 = new List<CardChoice>();
        foreach (Tribe tribe in tribes.Randomize())
        {
            list2.Add(new CardChoice
            {
                tribe = tribe
            });
        }
        __result = list2;
        return false;
    }

    /// <summary>
    /// Adds a new tribe to the game
    /// </summary>
    /// <param name="guid">The guid of the mod adding the tribe</param>
    /// <param name="name">The name of the tribe</param>
    /// <param name="tribeIcon">The tribal icon that will appear as a watermark on all cards belonging to this tribe</param>
    /// <param name="appearInTribeChoices">Indicates if the card should appear in tribal choice nodes</param>
    /// <param name="choiceCardbackTexture">The card back texture to display if the card should appear in tribal choice nodes</param>
    /// <returns>The unique identifier for the new tribe</returns>
    public static Tribe Add(string guid, string name, Texture2D tribeIcon = null, bool appearInTribeChoices = false, Texture2D choiceCardbackTexture = null)
    {
        Tribe tribe = GuidManager.GetEnumValue<Tribe>(guid, name);
        TribeInfo info = new() { tribe = tribe, icon = tribeIcon?.ConvertTexture(), cardback = choiceCardbackTexture, tribeChoice = appearInTribeChoices };
        tribes.Add(info);
        tribeTypes.Add(tribe);
        return tribe;
    }

    /// <summary>
    /// Adds a new tribe to the game
    /// </summary>
    /// <param name="guid">The guid of the mod adding the tribe</param>
    /// <param name="name">The name of the tribe</param>
    /// <param name="pathToTribeIcon">Path to the tribal icon that will appear as a watermark on all cards belonging to this tribe</param>
    /// <param name="appearInTribeChoices">Indicates if the card should appear in tribal choice nodes</param>
    /// <param name="pathToChoiceCardbackTexture">Path to the card back texture to display if the card should appear in tribal choice nodes</param>
    /// <returns>The unique identifier for the new tribe</returns>
    public static Tribe Add(string guid, string name, string pathToTribeIcon = null, bool appearInTribeChoices = false, string pathToChoiceCardBackTexture = null)
    {
        // Reason for 'is not null' is because if we pass 'null' to GetImageAsTexture, It will thorw an exception.
        return Add(guid, name, pathToTribeIcon is not null ? TextureHelper.GetImageAsTexture(pathToTribeIcon) : null, appearInTribeChoices, pathToChoiceCardBackTexture is not null ? TextureHelper.GetImageAsTexture(pathToChoiceCardBackTexture) : null);
    }

    public static bool IsCustomTribe(Tribe tribe)
    {
        return tribeTypes.Contains(tribe);
    }

    public static Texture2D GetTribeIcon(Tribe tribe, bool useMissingIconIfNull = true)
    {
        Texture2D texture2D = null;
        if (IsCustomTribe(tribe))
        {
            foreach (TribeInfo tribeInfo in NewTribes)
            {
                if (tribeInfo.tribe == tribe)
                {
                    if (tribeInfo.icon != null && tribeInfo.icon.texture != null)
                    {
                        texture2D = tribeInfo.icon.texture;
                    }
                    break;
                }
            }
        }
        else
        {
            // Vanilla tribe icon
            string str = "Art/Cards/TribeIcons/tribeicon_" + tribe.ToString().ToLowerInvariant();
            Sprite sprite = ResourceBank.Get<Sprite>(str);
            if (sprite != null)
            {
                texture2D = sprite.texture;
            }
        }

        if (texture2D == null && useMissingIconIfNull)
        {
            texture2D = TribeIconMissing;
        }
        return texture2D;
    }

    public class TribeInfo
    {
        public Tribe tribe;
        public Sprite icon;
        public bool tribeChoice;
        public Texture2D cardback;
    }
}