using HarmonyLib;
using DiskCardGame;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public class RenderAdditionalSigils
{
    // This patch modifies the way cards are rendered so that more than two sigils can be displayed on a single card
    private static Transform Find(Transform start, string target = "CardBase")
    {
        foreach (Transform child in start)
        {
            if (child.name == target)
            {
                return child;
            }

            Transform result = Find(child, target);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private static void AddQuadrupleIconSlotToCard(Transform abilityIconParent)
    {
        if (abilityIconParent == null)
        {
            return;
        }

        if (abilityIconParent.Find("DefaultIcons_4Abilities") == null)
        {
            CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();

            if (controller == null)
            {
                return;
            }

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            GameObject fourAbilities = UnityObject.Instantiate(twoAbilities, abilityIconParent);
            fourAbilities.name = "DefaultIcons_4Abilities";

            // Move the existing icons
            List<Transform> icons = fourAbilities.transform.Cast<Transform>().ToList();

            icons[0].localPosition = new Vector3(-0.115f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f);
            icons[1].localPosition = new Vector3(0.115f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f);

            // Make a new icon
            GameObject thirdIcon = UnityObject.Instantiate(icons[0].gameObject, fourAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.115f, -.07f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            GameObject fourthIcon = UnityObject.Instantiate(icons[0].gameObject, fourAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0.115f, -.07f, 0f);
            fourthIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            controller.defaultIconGroups.Add(fourAbilities);
        }
    }

    private static void AddQuintupleIconSlotToCard(Transform abilityIconParent)
    {
        if (abilityIconParent == null)
        {
            return;
        }

        if (abilityIconParent.Find("DefaultIcons_5Abilities") == null)
        {
            CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();

            if (controller == null)
            {
                return;
            }

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            GameObject fiveAbilities = UnityObject.Instantiate(twoAbilities, abilityIconParent);
            fiveAbilities.name = "DefaultIcons_5Abilities";

            // Move the existing icons
            List<Transform> icons = fiveAbilities.transform.Cast<Transform>().ToList();

            icons[0].localPosition = new Vector3(-0.0875f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;
            icons[1].localPosition = new Vector3(0.0875f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;

            // Make a new icon
            GameObject thirdIcon = UnityObject.Instantiate(icons[0].gameObject, fiveAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(0f, -.07f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            GameObject fourthIcon = UnityObject.Instantiate(icons[0].gameObject, fiveAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0.175f, -.07f, 0f);
            fourthIcon.transform.localScale = icons[1].localScale;

            GameObject fifthIcon = UnityObject.Instantiate(icons[0].gameObject, fiveAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(-0.175f, -.07f, 0f);
            fifthIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            controller.defaultIconGroups.Add(fiveAbilities);
        }
    }

    private static void AddSextupleIconSlotToCard(Transform abilityIconParent)
    {
        if (abilityIconParent == null)
        {
            return;
        }

        if (abilityIconParent.Find("DefaultIcons_6Abilities") == null)
        {
            CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();

            if (controller == null)
            {
                return;
            }

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            GameObject sixAbilities = UnityObject.Instantiate(twoAbilities, abilityIconParent);
            sixAbilities.name = "DefaultIcons_6Abilities";

            // Move the existing icons
            List<Transform> icons = sixAbilities.transform.Cast<Transform>().ToList();

            icons[0].localPosition = new Vector3(0f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;
            icons[1].localPosition = new Vector3(0.175f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;

            // Make a new icon
            GameObject thirdIcon = UnityObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.175f, .06f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            GameObject fourthIcon = UnityObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0f, -.07f, 0f);
            fourthIcon.transform.localScale = icons[1].localScale;

            GameObject fifthIcon = UnityObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(0.175f, -.07f, 0f);
            fifthIcon.transform.localScale = icons[1].localScale;

            GameObject sixthIcon = UnityObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            sixthIcon.name = "AbilityIcon";
            sixthIcon.transform.localPosition = new Vector3(-0.175f, -.07f, 0f);
            sixthIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            controller.defaultIconGroups.Add(sixAbilities);
        }
    }

    private static void AddSeptupleIconSlotToCard(Transform abilityIconParent)
    {
        if (abilityIconParent == null)
        {
            return;
        }

        if (abilityIconParent.Find("DefaultIcons_7Abilities") == null)
        {
            CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();

            if (controller == null)
            {
                return;
            }

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            GameObject sevenAbilities = UnityObject.Instantiate(twoAbilities, abilityIconParent);
            sevenAbilities.name = "DefaultIcons_7Abilities";

            // Move the existing icons
            List<Transform> icons = sevenAbilities.transform.Cast<Transform>().ToList();

            icons[0].localPosition = new Vector3(0f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;
            icons[1].localPosition = new Vector3(0.175f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;

            // Make a new icon
            GameObject thirdIcon = UnityObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.175f, .063f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            GameObject fourthIcon = UnityObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(-0.066875f, -.06f, 0f);
            fourthIcon.transform.localScale = icons[1].localScale;

            GameObject fifthIcon = UnityObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(0.066875f, -.06f, 0f);
            fifthIcon.transform.localScale = icons[1].localScale;

            GameObject sixthIcon = UnityObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            sixthIcon.name = "AbilityIcon";
            sixthIcon.transform.localPosition = new Vector3(-0.200625f, -.067f, 0f);
            sixthIcon.transform.localScale = icons[1].localScale;

            GameObject seventhIcon = UnityObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            seventhIcon.name = "AbilityIcon";
            seventhIcon.transform.localPosition = new Vector3(0.200625f, -.067f, 0f);
            seventhIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            controller.defaultIconGroups.Add(sevenAbilities);
        }
    }

    private static void AddOctupleIconSlotToCard(Transform abilityIconParent)
    {
        if (abilityIconParent == null)
        {
            return;
        }

        if (abilityIconParent.Find("DefaultIcons_8Abilities") == null)
        {
            CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();

            if (controller == null)
            {
                return;
            }

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            GameObject eightAbilities = UnityObject.Instantiate(twoAbilities, abilityIconParent);
            eightAbilities.name = "DefaultIcons_8Abilities";

            // Move the existing icons
            List<Transform> icons = eightAbilities.transform.Cast<Transform>().ToList();

            icons[0].localPosition = new Vector3(-0.066875f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;
            icons[1].localPosition = new Vector3(0.066875f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;

            // Make a new icon
            GameObject thirdIcon = UnityObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.200625f, .063f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            GameObject fourthIcon = UnityObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0.200625f, .063f, 0f);
            fourthIcon.transform.localScale = icons[1].localScale;

            GameObject fifthIcon = UnityObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(-0.066875f, -.06f, 0f);
            fifthIcon.transform.localScale = icons[1].localScale;

            GameObject sixthIcon = UnityObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            sixthIcon.name = "AbilityIcon";
            sixthIcon.transform.localPosition = new Vector3(0.066875f, -.06f, 0f);
            sixthIcon.transform.localScale = icons[1].localScale;

            GameObject seventhIcon = UnityObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            seventhIcon.name = "AbilityIcon";
            seventhIcon.transform.localPosition = new Vector3(-0.200625f, -.067f, 0f);
            seventhIcon.transform.localScale = icons[1].localScale;

            GameObject eighthIcon = UnityObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            eighthIcon.name = "AbilityIcon";
            eighthIcon.transform.localPosition = new Vector3(0.200625f, -.067f, 0f);
            eighthIcon.transform.localScale = icons[1].localScale;

            controller.defaultIconGroups.Add(eightAbilities);
        }
    }

    [HarmonyPatch(typeof(DiskCardGame.Card), nameof(DiskCardGame.Card.RenderCard))]
    [HarmonyPrefix]
    public static void UpdateLiveRenderedCard(ref DiskCardGame.Card __instance)
    {
        Transform cardBase = Find(__instance.gameObject.transform);
        if (cardBase != null)
        {
            Transform parent = cardBase.Find("CardAbilityIcons_Invisible");
            AddQuadrupleIconSlotToCard(parent);
            AddQuintupleIconSlotToCard(parent);
            AddSextupleIconSlotToCard(parent);
            AddSeptupleIconSlotToCard(parent);
            AddOctupleIconSlotToCard(parent);
        }
    }

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
    [HarmonyPrefix]
    public static void UpdateCardDisplayer(ref CardDisplayer3D __instance)
    {
        Transform cardBase = Find(__instance.gameObject.transform);
        if (cardBase != null)
        {
            Transform parent = cardBase.Find("CardAbilityIcons_Invisible");
            AddQuadrupleIconSlotToCard(parent);
            AddQuintupleIconSlotToCard(parent);
            AddSextupleIconSlotToCard(parent);
            AddSeptupleIconSlotToCard(parent);
            AddOctupleIconSlotToCard(parent);
        }
    }

    [HarmonyPatch(typeof(CardRenderCamera), nameof(CardRenderCamera.LiveRenderCard))]
    [HarmonyPrefix]
    public static void UpdateCamera(ref CardRenderCamera __instance)
    {
        Transform cardBase = Find(__instance.gameObject.transform);
        if (cardBase != null)
        {
            Transform parent = Find(cardBase, "CardAbilityIcons");
            AddQuadrupleIconSlotToCard(parent);
            AddQuintupleIconSlotToCard(parent);
            AddSextupleIconSlotToCard(parent);
            AddSeptupleIconSlotToCard(parent);
            AddOctupleIconSlotToCard(parent);
        }
    }
}
