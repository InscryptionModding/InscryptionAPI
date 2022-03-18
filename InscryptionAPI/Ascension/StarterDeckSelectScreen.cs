using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class StarterDeckSelectscreenPatches
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

        var pageTuple = AscensionRunSetupScreenBase.BuildPaginators(starterDeckContainer.transform);

        AscensionMenuInteractable leftController = pageTuple.Item1;
        AscensionMenuInteractable rightController = pageTuple.Item2;

        Action<MainInputInteractable> leftClickAction = (MainInputInteractable i) => paginator.StarterDeckPageLeft(i);
        Action<MainInputInteractable> rightClickAction = (MainInputInteractable i) => paginator.StarterDeckPageRight(i);

        leftController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(leftController.CursorSelectStarted, leftClickAction);
        rightController.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(rightController.CursorSelectStarted, rightClickAction);

        paginator.starterDeckPageIndex = 0;

        if (StarterDeckManager.NewDecks.Count == 0)
        {
            GameObject.Destroy(leftController.gameObject);
            GameObject.Destroy(rightController.gameObject);
        }
    }
}