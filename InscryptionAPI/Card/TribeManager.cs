using DiskCardGame;
using UnityEngine;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;

namespace InscryptionAPI.Card;

[HarmonyPatch]
public class TribeManager
{
    private static readonly List<TribeInfo> Tribes = new();

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.UpdateTribeIcon))]
    [HarmonyPostfix]
    public static void UpdateTribeIcon(CardDisplayer3D __instance, CardInfo info)
    {
        foreach (TribeInfo tribeInfo in Tribes.Where(t => t.icon))
        {
            if (info.IsOfTribe(tribeInfo.tribe))
            {
                bool foundSpriteRenderer = false;
                foreach (SpriteRenderer spriteRenderer in __instance.tribeIconRenderers)
                {
                    if (spriteRenderer.sprite == null)
                    {
                        foundSpriteRenderer = true;
                        spriteRenderer.sprite = tribeInfo.icon;
                        break;
                    }
                }
                if (!foundSpriteRenderer)
                {
                    SpriteRenderer last = __instance.tribeIconRenderers.Last();
                    SpriteRenderer spriteRenderer = UnityObject.Instantiate(last);
                    spriteRenderer.transform.parent = last.transform.parent;
                    spriteRenderer.transform.localPosition =
                        last.transform.localPosition + (__instance.tribeIconRenderers[1].transform.localPosition - __instance.tribeIconRenderers[0].transform.localPosition);
                }
            }

        }
    }

    [HarmonyPatch(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.GetCardbackTexture))]
    [HarmonyPostfix]
    public static void GetCardbackTexture(ref Texture __result, CardChoice choice)
    {
        if (__result == null && choice.resourceType != ResourceType.Blood && choice.resourceType != ResourceType.Bone)
        {
            __result = Tribes.Find((x) => x.tribe == choice.tribe).cardback;
        }
    }

    [HarmonyPatch(typeof(Part1CardChoiceGenerator), nameof(Part1CardChoiceGenerator.GenerateTribeChoices))]
    [HarmonyPrefix]
    public static bool GenerateTribeChoices(ref List<CardChoice> __result, int randomSeed)
    {
        List<Tribe> validTribes = new()
        {
            Tribe.Bird,
            Tribe.Canine,
            Tribe.Hooved,
            Tribe.Insect,
            Tribe.Reptile
        };
        validTribes.AddRange(TribeManager.Tribes.FindAll((x) => x.tribeChoice).ConvertAll((x) => x.tribe));
        List<Tribe> tribes = new(RunState.CurrentMapRegion.dominantTribes);
        validTribes.RemoveAll((Tribe x) => tribes.Contains(x));
        while (tribes.Count < 3)
        {
            Tribe item = validTribes[SeededRandom.Range(0, validTribes.Count, randomSeed++)];
            tribes.Add(item);
            validTribes.Remove(item);
        }
        while (tribes.Count > 3)
        {
            tribes.RemoveAt(SeededRandom.Range(0, tribes.Count, randomSeed++));
        }
        List<CardChoice> tribeList = tribes.Randomize().Select(tribe => new CardChoice { tribe = tribe }).ToList();
        __result = tribeList;
        return false;
    }

    public static Tribe Add(string guid, string name, Texture2D tribeIcon = null, bool appearInTribeChoices = false, Texture2D choiceCardbackTexture = null)
    {
        Tribe tribe = GuidManager.GetEnumValue<Tribe>(guid, name);
        TribeInfo info = new()
        {
            cardback = choiceCardbackTexture, 
            icon = tribeIcon.ConvertTexture(TextureHelper.SpriteType.TribeIcon), 
            tribeChoice = appearInTribeChoices,
            tribe = tribe, 
        };
        Tribes.Add(info);
        return tribe;
    }

    private class TribeInfo
    {
        public Tribe tribe;
        public Sprite icon;
        public bool tribeChoice;
        public Texture2D cardback;
    }
}