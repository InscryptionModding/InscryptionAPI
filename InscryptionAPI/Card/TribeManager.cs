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
public static class TribeManager
{
    private static readonly List<TribeInfo> tribes = new();
    private static readonly List<Tribe> tribeTypes = new();
    public static readonly ReadOnlyCollection<TribeInfo> NewTribes = new(tribes);
    public static readonly ReadOnlyCollection<Tribe> NewTribesTypes = new(tribeTypes);

    private static readonly Texture2D TribeIconMissing = TextureHelper.GetImageAsTexture("tribeicon_none.png", Assembly.GetExecutingAssembly());

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.UpdateTribeIcon))]
    [HarmonyPostfix]
    private static void UpdateTribeIcon(CardDisplayer3D __instance, CardInfo info)
    {
        if (info != null)
        {
            foreach (TribeInfo tribeInfo in tribes)
            {
                if (tribeInfo?.icon == null || info.IsNotOfTribe(tribeInfo.tribe))
                    continue;

                bool foundSpriteRenderer = false;
                foreach (SpriteRenderer spriteRenderer in __instance.tribeIconRenderers)
                {
                    if (spriteRenderer.sprite == null)
                    {
                        spriteRenderer.sprite = tribeInfo.icon;
                        foundSpriteRenderer = true;
                        break;
                    }
                }
                if (!foundSpriteRenderer)
                {
                    SpriteRenderer last = __instance.tribeIconRenderers.Last();
                    SpriteRenderer spriteRenderer = UnityObject.Instantiate(last);
                    spriteRenderer.transform.parent = last.transform.parent;
                    spriteRenderer.transform.localPosition = last.transform.localPosition + (__instance.tribeIconRenderers[1].transform.localPosition - __instance.tribeIconRenderers[0].transform.localPosition);
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
            __result = tribes.Find(x => x?.tribe == choice.tribe)?.cardback;
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

        // randomise the order the Tribes will be displayed
        List<CardChoice> list2 = new();
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
    /// Adds a new tribe to the game.
    /// </summary>
    /// <param name="guid">The guid of the mod adding the tribe.</param>
    /// <param name="name">The name of the tribe.</param>
    /// <param name="tribeIcon">The tribal icon that will appear as a watermark on all cards belonging to this tribe.</param>
    /// <param name="appearInTribeChoices">Indicates if the card should appear in tribal choice nodes.</param>
    /// <param name="choiceCardbackTexture">The card back texture to display if the card appears in tribal choice nodes. If no texture is provided, a placeholder will be generated from the tribal icon.</param>
    /// <returns>The unique identifier for the new tribe.</returns>
    public static Tribe Add(string guid, string name, Texture2D tribeIcon = null, bool appearInTribeChoices = false, Texture2D choiceCardbackTexture = null)
    {
        Tribe tribe = GuidManager.GetEnumValue<Tribe>(guid, name);
        Texture2D cardbackTexture = choiceCardbackTexture ?? (appearInTribeChoices ? MakePlaceholderCardback(tribeIcon) : null);
        TribeInfo info = new()
        {
            guid = guid,
            name = name,
            tribe = tribe,
            icon = tribeIcon?.ConvertTexture(),
            cardback = cardbackTexture,
            tribeChoice = appearInTribeChoices
        };
        tribes.Add(info);
        tribeTypes.Add(tribe);
        return tribe;
    }
    private static Texture2D MakePlaceholderCardback(Texture2D tribeIcon)
    {
        Texture2D emptyCardback = TextureHelper.GetImageAsTexture("empty_rewardCardBack.png", typeof(TribeManager).Assembly);
        if (tribeIcon == null) // if no tribe icon, return the empty card texture
            return emptyCardback;

        // we want the tribe icon to be in the centre of the card texture
        int startX = (emptyCardback.width - tribeIcon.width) / 2;
        int startY = (emptyCardback.height - tribeIcon.height) / 2;

        for (int x = startX; x < emptyCardback.width; x++)
        {
            int tribeX = x - startX;
            if (tribeX > tribeIcon.width) // prevents the icon texture from wrapping around
                break;

            for (int y = startY; y < emptyCardback.height; y++)
            {
                int tribeY = y - startY;
                if (tribeY > tribeIcon.height)
                    break;

                Color bgColor = emptyCardback.GetPixel(x, y);
                Color wmColor = tribeIcon.GetPixel(x - startX, y - startY);
                wmColor.a *= 0.9f;

                Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a);
                emptyCardback.SetPixel(x, y, final_color);
            }
        }

        emptyCardback.Apply();
        return emptyCardback;
    }
    /// <summary>
    /// Adds a new tribe to the game
    /// </summary>
    /// <param name="guid">The guid of the mod adding the tribe.</param>
    /// <param name="name">The name of the tribe.</param>
    /// <param name="pathToTribeIcon">Path to the tribal icon that will appear as a watermark on all cards belonging to this tribe.</param>
    /// <param name="appearInTribeChoices">Indicates if the card should appear in tribal choice nodes.</param>
    /// <param name="pathToChoiceCardbackTexture">Path to the card back texture to display if the card should appear in tribal choice nodes.</param>
    /// <returns>The unique identifier for the new tribe.</returns>
    public static Tribe Add(string guid, string name, string pathToTribeIcon = null, bool appearInTribeChoices = false, string pathToChoiceCardBackTexture = null)
    {
        // Reason for 'is not null' is because if we pass 'null' to GetImageAsTexture, It will thorw an exception.
        return Add(guid, name, pathToTribeIcon is not null ? TextureHelper.GetImageAsTexture(pathToTribeIcon) : null, appearInTribeChoices, pathToChoiceCardBackTexture is not null ? TextureHelper.GetImageAsTexture(pathToChoiceCardBackTexture) : null);
    }

    public static bool IsCustomTribe(Tribe tribe) => tribeTypes.Contains(tribe);

    public static Texture2D GetTribeIcon(Tribe tribe, bool useMissingIconIfNull = true)
    {
        Texture2D texture2D = null;
        if (IsCustomTribe(tribe))
        {
            foreach (TribeInfo tribeInfo in NewTribes)
            {
                if (tribeInfo.tribe != tribe)
                    continue;

                if (tribeInfo.icon != null && tribeInfo.icon.texture != null)
                    texture2D = tribeInfo.icon.texture;

                break;
            }
        }
        else
        {
            // Vanilla tribe icon
            string str = "Art/Cards/TribeIcons/tribeicon_" + tribe.ToString().ToLowerInvariant();
            Sprite sprite = ResourceBank.Get<Sprite>(str);
            if (sprite != null)
                texture2D = sprite.texture;
        }

        if (texture2D == null && useMissingIconIfNull)
            texture2D = TribeIconMissing;

        return texture2D;
    }

    /// <summary>
    /// The internal object used to store all relevant info about a Tribe.
    /// guid - The mod GUID that added the Tribe.
    /// name - The internal name of the Tribe.
    /// tribe - The enum value corresponding to this Tribe.
    /// icon - The sprite displayed on cards with this Tribe.
    /// tribeChoice - Whether or not this Tribe can appear at card tribe choice nodes.
    /// cardBack - The texture displayed at card tribe choice nodes. If null, the API will create one using the icon Sprite.
    /// </summary>
    public class TribeInfo
    {
        public string guid;
        public string name;
        public Tribe tribe;
        public Sprite icon;
        public bool tribeChoice;
        public Texture2D cardback;
    }
}