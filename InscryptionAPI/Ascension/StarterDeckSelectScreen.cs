using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class StarterDeckSelectScreenPatches
{
    [HarmonyPatch(typeof(AscensionStarterDeckIcon), nameof(AscensionStarterDeckIcon.AssignInfo))]
    [HarmonyPrefix]
    public static void ForceAssignInfo(ref AscensionStarterDeckIcon __instance, StarterDeckInfo info)
    {
        __instance.starterDeckInfo = info;
        __instance.conqueredRenderer.enabled = false;
    }

    [HarmonyPatch(typeof(AscensionMenuScreens), "ConfigurePostGameScreens")]
    [HarmonyPostfix]
    public static void AddPaginationToStarterDeckScreen()
    {
        StarterDeckPaginator paginator = AscensionMenuScreens.Instance.starterDeckSelectScreen.GetComponent<StarterDeckPaginator>();
        if (paginator == null)
            paginator = AscensionMenuScreens.Instance.starterDeckSelectScreen.AddComponent<StarterDeckPaginator>();


        GameObject starterDeckContainer = AscensionMenuScreens.Instance.starterDeckSelectScreen.transform.Find("Icons").gameObject;

        paginator.icons = new List<AscensionStarterDeckIcon>();
        for (int i = 1; i <= 8; i++)
            paginator.icons.Add(starterDeckContainer.transform.Find($"StarterDeckIcon_{i}").gameObject.GetComponent<AscensionStarterDeckIcon>());

        var (leftController, rightController) = AscensionRunSetupScreenBase.BuildPaginators(starterDeckContainer.transform);

        Action<MainInputInteractable> leftClickAction = i => paginator.StarterDeckPageLeft(i);
        Action<MainInputInteractable> rightClickAction = i => paginator.StarterDeckPageRight(i);

        leftController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(leftController.CursorSelectStarted, leftClickAction);
        rightController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(rightController.CursorSelectStarted, rightClickAction);

        paginator.starterDeckPageIndex = 0;

        if (StarterDeckManager.NewDecks.Count == 0)
        {
            UnityObject.Destroy(leftController.gameObject);
            UnityObject.Destroy(rightController.gameObject);
        }
    }
}
