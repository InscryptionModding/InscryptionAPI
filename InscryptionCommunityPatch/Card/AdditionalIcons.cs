using HarmonyLib;
using DiskCardGame;
using UnityEngine;
using System.Collections.Generic;

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

    private static void AddTripleIconSlotToCard(Transform abilityIconParent)
    {
        //PLugin.Log.LogInfo($"Ability icon parent: {abilityIconParent}");

        if (abilityIconParent == null)
        {
            return;
        }

        if (abilityIconParent.Find("DefaultIcons_3Abilities") == null)
        {
            CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();

            if (controller == null)
            {
                return;
            }

            //PLugin.Log.LogInfo($"Icons: {defaultIconGroups}");

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            //PLugin.Log.LogInfo($"2Abilities: {twoAbilities}");
            GameObject threeAbilities = GameObject.Instantiate(twoAbilities, abilityIconParent);
            threeAbilities.name = "DefaultIcons_3Abilities";

            // Move the existing icons
            List<Transform> icons = new();
            foreach (Transform icon in threeAbilities.transform)
            {
                icons.Add(icon);
            }

            //PLugin.Log.LogInfo($"Moving icons");
            icons[0].localPosition = new Vector3(-0.115f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f);
            icons[1].localPosition = new Vector3(0.115f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f);

            // Make a new icon
            //PLugin.Log.LogInfo($"Making third icon");
            GameObject thirdIcon = GameObject.Instantiate(icons[0].gameObject, threeAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(0f, -.07f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            //PLugin.Log.LogInfo($"Updating list");
            controller.defaultIconGroups.Add(threeAbilities);

        }
    }

    private static void AddQuadrupleIconSlotToCard(Transform abilityIconParent)
    {
        //PLugin.Log.LogInfo($"Ability icon parent: {abilityIconParent}");

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

            //PLugin.Log.LogInfo($"Icons: {defaultIconGroups}");

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            //PLugin.Log.LogInfo($"2Abilities: {twoAbilities}");
            GameObject fourAbilities = GameObject.Instantiate(twoAbilities, abilityIconParent);
            fourAbilities.name = "DefaultIcons_4Abilities";

            // Move the existing icons
            List<Transform> icons = new();
            foreach (Transform icon in fourAbilities.transform)
            {
                icons.Add(icon);
            }

            //PLugin.Log.LogInfo($"Moving icons");
            //PLugin.Log.LogInfo(icons.Count);
            icons[0].localPosition = new Vector3(-0.115f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f);
            icons[1].localPosition = new Vector3(0.115f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f);

            // Make a new icon
            //PLugin.Log.LogInfo($"Making third icon");
            GameObject thirdIcon = GameObject.Instantiate(icons[0].gameObject, fourAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.115f, -.07f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fourth icon");
            GameObject fourthIcon = GameObject.Instantiate(icons[0].gameObject, fourAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0.115f, -.07f, 0f);
            fourthIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            //PLugin.Log.LogInfo($"Updating list");
            controller.defaultIconGroups.Add(fourAbilities);
        }
    }

    private static void AddQuintupleIconSlotToCard(Transform abilityIconParent)
    {
        //PLugin.Log.LogInfo($"Ability icon parent: {abilityIconParent}");

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

            //PLugin.Log.LogInfo($"Icons: {defaultIconGroups}");

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            //PLugin.Log.LogInfo($"2Abilities: {twoAbilities}");
            GameObject fiveAbilities = GameObject.Instantiate(twoAbilities, abilityIconParent);
            fiveAbilities.name = "DefaultIcons_5Abilities";

            // Move the existing icons
            List<Transform> icons = new();
            foreach (Transform icon in fiveAbilities.transform)
            {
                icons.Add(icon);
            }

            //PLugin.Log.LogInfo($"Moving icons");
            //PLugin.Log.LogInfo(icons.Count);
            icons[0].localPosition = new Vector3(-0.0875f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;
            icons[1].localPosition = new Vector3(0.0875f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;

            // Make a new icon
            //PLugin.Log.LogInfo($"Making third icon");
            GameObject thirdIcon = GameObject.Instantiate(icons[0].gameObject, fiveAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(0f, -.07f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fourth icon");
            GameObject fourthIcon = GameObject.Instantiate(icons[0].gameObject, fiveAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0.175f, -.07f, 0f);
            fourthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fifth icon");
            GameObject fifthIcon = GameObject.Instantiate(icons[0].gameObject, fiveAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(-0.175f, -.07f, 0f);
            fifthIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            //PLugin.Log.LogInfo($"Updating list");
            controller.defaultIconGroups.Add(fiveAbilities);
        }
    }

    private static void AddSextupleIconSlotToCard(Transform abilityIconParent)
    {
        //PLugin.Log.LogInfo($"Ability icon parent: {abilityIconParent}");

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

            //PLugin.Log.LogInfo($"Icons: {defaultIconGroups}");

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            //PLugin.Log.LogInfo($"2Abilities: {twoAbilities}");
            GameObject sixAbilities = GameObject.Instantiate(twoAbilities, abilityIconParent);
            sixAbilities.name = "DefaultIcons_6Abilities";

            // Move the existing icons
            List<Transform> icons = new();
            foreach (Transform icon in sixAbilities.transform)
            {
                icons.Add(icon);
            }

            //PLugin.Log.LogInfo($"Moving icons");
            //PLugin.Log.LogInfo(icons.Count);
            icons[0].localPosition = new Vector3(0f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;
            icons[1].localPosition = new Vector3(0.175f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.75f;

            // Make a new icon
            //PLugin.Log.LogInfo($"Making third icon");
            GameObject thirdIcon = GameObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.175f, .06f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fourth icon");
            GameObject fourthIcon = GameObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0f, -.07f, 0f); 
            fourthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fifth icon");
            GameObject fifthIcon = GameObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(0.175f, -.07f, 0f);
            fifthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making sixth icon");
            GameObject sixthIcon = GameObject.Instantiate(icons[0].gameObject, sixAbilities.transform);
            sixthIcon.name = "AbilityIcon";
            sixthIcon.transform.localPosition = new Vector3(-0.175f, -.07f, 0f);
            sixthIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            //PLugin.Log.LogInfo($"Updating list");
            controller.defaultIconGroups.Add(sixAbilities);
        }
    }

    private static void AddSeptupleIconSlotToCard(Transform abilityIconParent)
    {
        //PLugin.Log.LogInfo($"Ability icon parent: {abilityIconParent}");

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

            //PLugin.Log.LogInfo($"Icons: {defaultIconGroups}");

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            //PLugin.Log.LogInfo($"2Abilities: {twoAbilities}");
            GameObject sevenAbilities = GameObject.Instantiate(twoAbilities, abilityIconParent);
            sevenAbilities.name = "DefaultIcons_7Abilities";

            // Move the existing icons
            List<Transform> icons = new();
            foreach (Transform icon in sevenAbilities.transform)
            {
                icons.Add(icon);
            }

            //PLugin.Log.LogInfo($"Moving icons");
            //PLugin.Log.LogInfo(icons.Count);
            icons[0].localPosition = new Vector3(0f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;
            icons[1].localPosition = new Vector3(0.175f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;

            // Make a new icon
            //PLugin.Log.LogInfo($"Making third icon");
            GameObject thirdIcon = GameObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.175f, .063f, 0f); 
            thirdIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fourth icon");
            GameObject fourthIcon = GameObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(-0.066875f, -.06f, 0f); 
            fourthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fifth icon");
            GameObject fifthIcon = GameObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(0.066875f, -.06f, 0f); 
            fifthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making sixth icon");
            GameObject sixthIcon = GameObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            sixthIcon.name = "AbilityIcon";
            sixthIcon.transform.localPosition = new Vector3(-0.200625f, -.067f, 0f);
            sixthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making seventh icon");
            GameObject seventhIcon = GameObject.Instantiate(icons[0].gameObject, sevenAbilities.transform);
            seventhIcon.name = "AbilityIcon";
            seventhIcon.transform.localPosition = new Vector3(0.200625f, -.067f, 0f);
            seventhIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            //PLugin.Log.LogInfo($"Updating list");
            controller.defaultIconGroups.Add(sevenAbilities);
        }
    }

    private static void AddOctupleIconSlotToCard(Transform abilityIconParent)
    {
        //PLugin.Log.LogInfo($"Ability icon parent: {abilityIconParent}");

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

            //PLugin.Log.LogInfo($"Icons: {defaultIconGroups}");

            // Create the three abilities slot
            GameObject twoAbilities = abilityIconParent.Find("DefaultIcons_2Abilities").gameObject;
            //PLugin.Log.LogInfo($"2Abilities: {twoAbilities}");
            GameObject eightAbilities = GameObject.Instantiate(twoAbilities, abilityIconParent);
            eightAbilities.name = "DefaultIcons_8Abilities";

            // Move the existing icons
            List<Transform> icons = new();
            foreach (Transform icon in eightAbilities.transform)
            {
                icons.Add(icon);
            }

            //PLugin.Log.LogInfo($"Moving icons");
            //PLugin.Log.LogInfo(icons.Count);
            icons[0].localPosition = new Vector3(-0.066875f, .06f, 0f);
            icons[0].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;
            icons[1].localPosition = new Vector3(0.066875f, .06f, 0f);
            icons[1].localScale = new Vector3(0.22f, 0.1467f, 1f) * 0.5625f;

            // Make a new icon
            //PLugin.Log.LogInfo($"Making third icon");
            GameObject thirdIcon = GameObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            thirdIcon.name = "AbilityIcon";
            thirdIcon.transform.localPosition = new Vector3(-0.200625f, .063f, 0f);
            thirdIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fourth icon");
            GameObject fourthIcon = GameObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            fourthIcon.name = "AbilityIcon";
            fourthIcon.transform.localPosition = new Vector3(0.200625f, .063f, 0f); 
            fourthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making fifth icon");
            GameObject fifthIcon = GameObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            fifthIcon.name = "AbilityIcon";
            fifthIcon.transform.localPosition = new Vector3(-0.066875f, -.06f, 0f); 
            fifthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making sixth icon");
            GameObject sixthIcon = GameObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            sixthIcon.name = "AbilityIcon";
            sixthIcon.transform.localPosition = new Vector3(0.066875f, -.06f, 0f); 
            sixthIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making seventh icon");
            GameObject seventhIcon = GameObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            seventhIcon.name = "AbilityIcon";
            seventhIcon.transform.localPosition = new Vector3(-0.200625f, -.067f, 0f); 
            seventhIcon.transform.localScale = icons[1].localScale;

            //PLugin.Log.LogInfo($"Making eighth icon");
            GameObject eighthIcon = GameObject.Instantiate(icons[0].gameObject, eightAbilities.transform);
            eighthIcon.name = "AbilityIcon";
            eighthIcon.transform.localPosition = new Vector3(0.200625f, -.067f, 0f);
            eighthIcon.transform.localScale = icons[1].localScale;

            // Update the abilityicon list
            //PLugin.Log.LogInfo($"Updating list");
            controller.defaultIconGroups.Add(eightAbilities);
        }
    }

    [HarmonyPatch(typeof(DiskCardGame.Card), "RenderCard")]
    [HarmonyPrefix]
    public static void UpdateLiveRenderedCard(ref DiskCardGame.Card __instance)
    {
        Transform cardBase = Find(__instance.gameObject.transform);
        if (cardBase != null)
        {
            Transform parent = cardBase.Find("CardAbilityIcons_Invisible");
            AddTripleIconSlotToCard(parent);
            AddQuadrupleIconSlotToCard(parent);
            AddQuintupleIconSlotToCard(parent);
            AddSextupleIconSlotToCard(parent);
            AddSeptupleIconSlotToCard(parent);
            AddOctupleIconSlotToCard(parent);
        }
    }

    [HarmonyPatch(typeof(CardDisplayer3D), "DisplayInfo")]
    [HarmonyPrefix]
    public static void UpdateCardDisplayer(ref CardDisplayer3D __instance)
    {
        Transform cardBase = Find(__instance.gameObject.transform);
        if (cardBase != null)
        {
            Transform parent = cardBase.Find("CardAbilityIcons_Invisible");
            AddTripleIconSlotToCard(parent);
            AddQuadrupleIconSlotToCard(parent);
            AddQuintupleIconSlotToCard(parent);
            AddSextupleIconSlotToCard(parent);
            AddSeptupleIconSlotToCard(parent);
            AddOctupleIconSlotToCard(parent);
        }
    }

    [HarmonyPatch(typeof(CardRenderCamera), "LiveRenderCard")]
    [HarmonyPrefix]
    public static void UpdateCamera(ref CardRenderCamera __instance)
    {
        Transform cardBase = Find(__instance.gameObject.transform);
        if (cardBase != null)
        {
            Transform parent = Find(cardBase, "CardAbilityIcons");
            AddTripleIconSlotToCard(parent);
            AddQuadrupleIconSlotToCard(parent);
            AddQuintupleIconSlotToCard(parent);
            AddSextupleIconSlotToCard(parent);
            AddSeptupleIconSlotToCard(parent);
            AddOctupleIconSlotToCard(parent);
        }
    }
}