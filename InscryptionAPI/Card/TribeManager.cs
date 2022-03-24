using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;

namespace InscryptionAPI.Card
{
    [HarmonyPatch]
    public class TribeManager
    {
        private static readonly List<TribeInfo> tribes = new();

        [HarmonyPatch(typeof(CardDisplayer3D), "UpdateTribeIcon")]
        [HarmonyPostfix]
        public static void UpdateTribeIcon(CardDisplayer3D __instance, CardInfo info)
        {
            foreach (TribeInfo tribe in tribes)
            {
                if (tribe.icon != null)
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

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "GetCardbackTexture")]
        [HarmonyPostfix]
        public static void GetCardbackTexture(ref Texture __result, CardChoice choice)
        {
            if (__result == null && choice.resourceType != ResourceType.Blood && choice.resourceType != ResourceType.Bone)
            {
                __result = tribes.Find((x) => x.tribe == choice.tribe).cardback;
            }
        }

        [HarmonyPatch(typeof(Part1CardChoiceGenerator), "GenerateTribeChoices")]
        [HarmonyPrefix]
        public static bool GenerateTribeChoices(ref List<CardChoice> __result, int randomSeed)
        {
            List<Tribe> list = new()
            {
                Tribe.Bird,
                Tribe.Canine,
                Tribe.Hooved,
                Tribe.Insect,
                Tribe.Reptile
            };
            list.AddRange(TribeManager.tribes.FindAll((x) => x.tribeChoice).ConvertAll((x) => x.tribe));
            List<Tribe> tribes = new(RunState.CurrentMapRegion.dominantTribes);
            list.RemoveAll((Tribe x) => tribes.Contains(x));
            while (tribes.Count < 3)
            {
                Tribe item = list[SeededRandom.Range(0, list.Count, randomSeed++)];
                tribes.Add(item);
                list.Remove(item);
            }
            while (tribes.Count > 3)
            {
                tribes.RemoveAt(SeededRandom.Range(0, tribes.Count, randomSeed++));
            }
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

        public static Tribe Add(string guid, string name, Texture2D tribeIcon = null, bool appearInTribeChoices = false, Texture2D choiceCardbackTexture = null)
        {
            Tribe tribe = GuidManager.GetEnumValue<Tribe>(guid, name);
            TribeInfo info = new() { tribe = tribe, icon = tribeIcon.ConvertTexture(), cardback = choiceCardbackTexture, tribeChoice = appearInTribeChoices };
            tribes.Add(info);
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
}
