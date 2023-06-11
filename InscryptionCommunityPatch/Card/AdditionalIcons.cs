using DiskCardGame;
using GBC;
using HarmonyLib;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class RenderAdditionalSigils // Modifies how cards are rendered so up to 8 sigils can be displayed on a card
{
    private static Vector3 LocalScaleBase3D => SaveManager.SaveFile.IsPart1 ? new(0.2175f, 0.145f, 1f) : new(0.3f, 0.3f, 0.75f);

    private static GameObject NewIconGroup(CardAbilityIcons controller, Transform parent, int newSlotNum)
    {
        GameObject prevIconGroup = parent.Find($"DefaultIcons_{newSlotNum - 1}Abilities").gameObject;

        GameObject newIconGroup = UnityObject.Instantiate(prevIconGroup, parent);
        newIconGroup.name = $"DefaultIcons_{newSlotNum}Abilities";

        controller.defaultIconGroups.Add(newIconGroup);

        return newIconGroup;
    }
    private static List<Transform> NewIcons(GameObject newIconGroup, int slotNum, float act1ScaleMult = 1f)
    {
        List<Transform> icons = new();
        foreach (Transform icon in newIconGroup.transform)
            icons.Add(icon);

        GameObject newIcon = UnityObject.Instantiate(icons[0].gameObject, newIconGroup.transform);
        newIcon.name = SaveManager.SaveFile.IsPart1 ? "AbilityIcon" : $"Ability_{slotNum}";
        icons.Add(newIcon.transform);

        Vector3 newScale = LocalScaleBase3D * (SaveManager.SaveFile.IsPart1 ? act1ScaleMult : 0.65f);
        foreach (Transform icon in icons)
            icon.localScale = newScale;

        return icons;
    }

    private static void AddIconSlotsToCard(Transform abilityIconParent)
    {
        if (abilityIconParent == null)
            return;

        CardAbilityIcons controller = abilityIconParent.gameObject.GetComponent<CardAbilityIcons>();
        if (controller == null)
            return;

        // create the ability icon groups if they don't exist
        if (abilityIconParent.Find("DefaultIcons_4Abilities") == null)
            AddQuadrupleIconSlotToCard(controller, abilityIconParent);

        if (abilityIconParent.Find("DefaultIcons_5Abilities") == null)
            AddQuintupleIconSlotToCard(controller, abilityIconParent);

        if (abilityIconParent.Find("DefaultIcons_6Abilities") == null)
            AddSextupleIconSlotToCard(controller, abilityIconParent);

        if (abilityIconParent.Find("DefaultIcons_7Abilities") == null)
            AddSeptupleIconSlotToCard(controller, abilityIconParent);

        if (abilityIconParent.Find("DefaultIcons_8Abilities") == null)
            AddOctupleIconSlotToCard(controller, abilityIconParent);
    }

    private static void AddQuadrupleIconSlotToCard(CardAbilityIcons controller, Transform abilityIconParent)
    {
        GameObject iconGroup4 = NewIconGroup(controller, abilityIconParent, 4);

        List<Transform> icons = NewIcons(iconGroup4, 4);

        icons[0].localPosition = new Vector3(-0.12f, 0.06f, 0f);
        icons[1].localPosition = new Vector3(0.12f, 0.06f, 0f);
        icons[2].localPosition = new Vector3(-0.12f, -0.077f, 0f);
        icons[3].localPosition = new Vector3(0.12f, -0.077f, 0f);
    }
    private static void AddQuintupleIconSlotToCard(CardAbilityIcons controller, Transform abilityIconParent)
    {
        GameObject iconGroup5 = NewIconGroup(controller, abilityIconParent, 5);

        List<Transform> icons = NewIcons(iconGroup5, 5, 0.75f);

        if (SaveManager.SaveFile.IsPart1)
        {
            icons[0].localPosition = new Vector3(-0.17f, 0.06f, 0f);
            icons[1].localPosition = new Vector3(0f, 0.06f, 0f);
            icons[2].localPosition = new Vector3(0.17f, 0.06f, 0f);
            icons[3].localPosition = new Vector3(-0.09f, -0.077f, 0f);
            icons[4].localPosition = new Vector3(0.09f, -0.077f, 0f);
        }
        else
        {
            icons[0].localPosition = new Vector3(-0.42f, 0.085f, 0f);
            icons[1].localPosition = new Vector3(-0.14f, 0.085f, 0f);
            icons[2].localPosition = new Vector3(0.14f, 0.085f, 0f);
            icons[3].localPosition = new Vector3(0.42f, 0.085f, 0f);
            icons[4].localPosition = new Vector3(0f, -0.125f, 0f);
        }
    }
    private static void AddSextupleIconSlotToCard(CardAbilityIcons controller, Transform abilityIconParent)
    {
        GameObject iconGroup6 = NewIconGroup(controller, abilityIconParent, 6);

        List<Transform> icons = NewIcons(iconGroup6, 6, 0.75f);

        if (SaveManager.SaveFile.IsPart1)
        {
            icons[3].localPosition = new Vector3(-0.17f, -.077f, 0f);
            icons[4].localPosition = new Vector3(0f, -.077f, 0f);
            icons[5].localPosition = new Vector3(0.17f, -.077f, 0f);
        }
        else
        {
            icons[4].localPosition = new Vector3(-0.14f, -0.125f, 0f);
            icons[5].localPosition = new Vector3(0.14f, -0.125f, 0f);
        }
    }
    private static void AddSeptupleIconSlotToCard(CardAbilityIcons controller, Transform abilityIconParent)
    {
        GameObject iconGroup7 = NewIconGroup(controller, abilityIconParent, 7);

        List<Transform> icons = NewIcons(iconGroup7, 7, 0.5625f);

        if (SaveManager.SaveFile.IsPart1)
        {
            icons[0].localPosition = new Vector3(-0.18f, 0.06f, 0f);
            icons[1].localPosition = new Vector3(-0.06f, 0.06f, 0f);
            icons[2].localPosition = new Vector3(0.06f, 0.06f, 0f);
            icons[3].localPosition = new Vector3(0.18f, 0.06f, 0f);
            icons[4].localPosition = new Vector3(-0.12f, -0.067f, 0f);
            icons[5].localPosition = new Vector3(0f, -0.067f, 0f);
            icons[6].localPosition = new Vector3(0.12f, -.067f, 0f);
        }
        else
        {
            icons[4].localPosition = new Vector3(-0.28f, -0.125f, 0f);
            icons[5].localPosition = new Vector3(0f, -0.125f, 0f);
            icons[6].localPosition = new Vector3(0.28f, -0.125f, 0f);
        }
    }
    private static void AddOctupleIconSlotToCard(CardAbilityIcons controller, Transform abilityIconParent)
    {
        GameObject iconGroup8 = NewIconGroup(controller, abilityIconParent, 8);

        List<Transform> icons = NewIcons(iconGroup8, 8, 0.5625f);

        if (SaveManager.SaveFile.IsPart1)
        {
            icons[4].localPosition = new Vector3(-0.18f, -0.067f, 0f);
            icons[5].localPosition = new Vector3(-0.06f, -0.067f, 0f);
            icons[6].localPosition = new Vector3(0.06f, -0.067f, 0f);
            icons[7].localPosition = new Vector3(0.18f, -0.067f, 0f);
        }
        else
        {
            icons[4].localPosition = new Vector3(-0.42f, -0.125f, 0f);
            icons[5].localPosition = new Vector3(-0.14f, -0.125f, 0f);
            icons[6].localPosition = new Vector3(0.14f, -0.125f, 0f);
            icons[7].localPosition = new Vector3(0.42f, -0.125f, 0f);
        }
    }

    private static void AddPixelIconSlotsToCard(Transform pixelAbilityIconParent)
    {
        if (pixelAbilityIconParent == null)
            return;

        PixelCardAbilityIcons controller = pixelAbilityIconParent.gameObject.GetComponent<PixelCardAbilityIcons>();
        if (controller == null)
            return;

        // create the ability icon groups if they don't exist
        if (pixelAbilityIconParent.Find("AbilityIcons_3") == null)
            AddTriplePixelicon(controller, pixelAbilityIconParent);

        if (pixelAbilityIconParent.Find("AbilityIcons_4") == null)
            AddQuadruplePixelIcon(controller, pixelAbilityIconParent);

        if (pixelAbilityIconParent.Find("AbilityIcons_5") == null)
            AddQuintuplePixelIcon(controller, pixelAbilityIconParent);

        if (pixelAbilityIconParent.Find("AbilityIcons_6") == null)
            AddSextuplePixelIcon(controller, pixelAbilityIconParent);

        if (pixelAbilityIconParent.Find("AbilityIcons_7") == null)
            AddSeptuplePixelIcon(controller, pixelAbilityIconParent);

        if (pixelAbilityIconParent.Find("AbilityIcons_8") == null)
            AddOctuplePixelIcon(controller, pixelAbilityIconParent);
    }

    private static GameObject NewPixelIconGroup(PixelCardAbilityIcons controller, Transform parent, int newSlotNum)
    {
        GameObject prevIconGroup = parent.Find($"AbilityIcons_{newSlotNum - 1}").gameObject;

        GameObject newIconGroup = UnityObject.Instantiate(prevIconGroup, parent);
        newIconGroup.name = $"AbilityIcons_{newSlotNum}";

        controller.abilityIconGroups.Add(newIconGroup);

        return newIconGroup;
    }
    private static List<Transform> NewPixelIcons(GameObject newIconGroup, string newIconName, float scaleMult = 1f)
    {
        List<Transform> icons = new();
        foreach (Transform icon in newIconGroup.transform)
            icons.Add(icon);

        GameObject newIcon = UnityObject.Instantiate(icons[0].gameObject, newIconGroup.transform);
        newIcon.name = newIconName;
        icons.Add(newIcon.transform);

        Vector3 newScale = Vector3.one * scaleMult;
        foreach (Transform icon in icons)
        {
            icon.localScale = newScale;

            foreach (Transform subIcon in icon)
                subIcon.localScale = Vector3.one * 0.5f;
        }

        return icons;
    }

    private static void AddTriplePixelicon(PixelCardAbilityIcons controller, Transform pixelParent)
    {
        GameObject pixelIconGroup3 = NewPixelIconGroup(controller, pixelParent, 3);

        List<Transform> icons = NewPixelIcons(pixelIconGroup3, "Right", 0.6471f); // ~11 pixels

        icons[1].name = "Center";

        icons[0].localPosition = new Vector3(-0.13f, 0f, 0f);
        icons[1].localPosition = new Vector3(0f, 0f, 0f);
        icons[2].localPosition = new Vector3(0.12f, 0f, 0f);
    }
    private static void AddQuadruplePixelIcon(PixelCardAbilityIcons controller, Transform pixelParent)
    {
        GameObject pixelIconGroup4 = NewPixelIconGroup(controller, pixelParent, 4);

        List<Transform> icons = NewPixelIcons(pixelIconGroup4, "Bottom Right", 0.5294f); // ~ 9 pixels

        icons[1].name = "Right";
        icons[2].name = "Bottom Left";

        icons[0].localPosition = new Vector3(-0.08f, 0.04f, 0f);
        icons[1].localPosition = new Vector3(0.07f, 0.04f, 0f);
        icons[2].localPosition = new Vector3(-0.08f, -0.06f, 0f);
        icons[3].localPosition = new Vector3(0.07f, -0.06f, 0f);
    }
    private static void AddQuintuplePixelIcon(PixelCardAbilityIcons controller, Transform pixelParent)
    {
        GameObject pixelIconGroup5 = NewPixelIconGroup(controller, pixelParent, 5);

        List<Transform> icons = NewPixelIcons(pixelIconGroup5, "Bottom Right", 0.5294f); // ~9 pixels

        icons[1].name = "Center";
        icons[2].name = "Right";
        icons[3].name = "Bottom Left";

        icons[0].localPosition = new Vector3(-0.13f, 0.04f, 0f);
        icons[1].localPosition = new Vector3(0f, 0.04f, 0f);
        icons[2].localPosition = new Vector3(0.12f, 0.04f, 0f);
        icons[3].localPosition = new Vector3(-0.08f, -0.06f, 0f);
        icons[4].localPosition = new Vector3(0.07f, -0.06f, 0f);
    }
    private static void AddSextuplePixelIcon(PixelCardAbilityIcons controller, Transform pixelParent)
    {
        GameObject pixelIconGroup6 = NewPixelIconGroup(controller, pixelParent, 6);

        List<Transform> icons = NewPixelIcons(pixelIconGroup6, "Bottom Right", 0.4706f); // ~8 pixels

        icons[4].name = "Bottom Center";

        icons[3].localPosition = new Vector3(-0.13f, -0.04f, 0f);
        icons[4].localPosition = new Vector3(0f, -0.04f, 0f);
        icons[5].localPosition = new Vector3(0.12f, -0.04f, 0f);
    }
    private static void AddSeptuplePixelIcon(PixelCardAbilityIcons controller, Transform pixelParent)
    {
        GameObject pixelIconGroup7 = NewPixelIconGroup(controller, pixelParent, 7);

        List<Transform> icons = NewPixelIcons(pixelIconGroup7, "Bottom Right", 0.4706f); // ~8 pixels

        icons[1].name = "Center Left";
        icons[2].name = "Center Right";
        icons[3].name = "Right";
        icons[4].name = "Bottom Left";
        icons[5].name = "Bottom Center";

        icons[0].localPosition = new Vector3(-0.15f, 0.04f, 0f);
        icons[1].localPosition = new Vector3(-0.06f, 0.04f, 0f);
        icons[2].localPosition = new Vector3(0.05f, 0.04f, 0f);
        icons[3].localPosition = new Vector3(0.14f, 0.04f, 0f);
        icons[4].localPosition = new Vector3(-0.09f, -0.04f, 0f);
        icons[5].localPosition = new Vector3(0f, -0.04f, 0f);
        icons[6].localPosition = new Vector3(0.08f, -0.04f, 0f);
    }
    private static void AddOctuplePixelIcon(PixelCardAbilityIcons controller, Transform pixelParent)
    {
        GameObject pixelIconGroup8 = NewPixelIconGroup(controller, pixelParent, 8);

        List<Transform> icons = NewPixelIcons(pixelIconGroup8, "Bottom Right", 0.4706f); // ~8 pixels

        icons[5].name = "Bottom Center Left";
        icons[6].name = "Bottom Center Right";

        icons[4].localPosition = new Vector3(-0.15f, -0.04f, 0f);
        icons[5].localPosition = new Vector3(-0.06f, -0.04f, 0f);
        icons[6].localPosition = new Vector3(0.05f, -0.04f, 0f);
        icons[7].localPosition = new Vector3(0.14f, -0.04f, 0f);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PixelCardAbilityIcons), nameof(PixelCardAbilityIcons.DisplayAbilities),
        new Type[] { typeof(CardRenderInfo), typeof(PlayableCard) })]
    private static void AddExtraPixelAbilityIcons(PixelCardAbilityIcons __instance) => AddPixelIconSlotsToCard(__instance.transform);

    [HarmonyPrefix, HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.UpdateAbilityIcons))]
    private static void AddExtraAbilityIcons(CardAbilityIcons __instance)
    {
        if (SaveManager.SaveFile.IsPart1 || SaveManager.SaveFile.IsPart3)
            AddIconSlotsToCard(__instance.transform);
    }
}