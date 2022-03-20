using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Ascension;

[HarmonyPatch]
public static class StarterDeckSelectscreenPatches
{
    [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), "Start")]
    [HarmonyPostfix]
    public static void Postfix(AscensionChooseStarterDeckScreen __instance)
    {
        if (__instance.GetComponent<StarterDeckPaginator>() == null)
        {
            List<StarterDeckInfo> decksToAdd = new(StarterDeckManager.AllDecks.ConvertAll((x) => x.Info));
            List<AscensionStarterDeckIcon> icons = __instance.deckIcons;
            icons.ForEach(delegate (AscensionStarterDeckIcon ic)
            {
                if (ic != null && ic.Info != null && !string.IsNullOrEmpty(ic.Info.title) && decksToAdd.FindAll((StarterDeckInfo inf) => inf.title.ToLower() == ic.Info.title.ToLower()).Count > 0)
                {
                    decksToAdd.Remove(decksToAdd.Find((StarterDeckInfo inf) => inf.title.ToLower() == ic.Info.title.ToLower()));
                }
            });
            icons.ForEach(delegate (AscensionStarterDeckIcon ic)
            {
                if (ic != null && ic.Info == null && decksToAdd.Count > 0)
                {
                    ic.starterDeckInfo = decksToAdd[0];
                    ic.AssignInfo(decksToAdd[0]);
                    decksToAdd.RemoveAt(0);
                }
            });
            List<List<StarterDeckInfo>> pagesToAdd = new List<List<StarterDeckInfo>>();
            while (decksToAdd.Count > 0)
            {
                List<StarterDeckInfo> page = new List<StarterDeckInfo>();
                for (int i = 0; i < icons.Count; i++)
                {
                    if (decksToAdd.Count > 0)
                    {
                        page.Add(decksToAdd[0]);
                        decksToAdd.RemoveAt(0);
                    }
                }
                pagesToAdd.Add(page);
            }
            if (pagesToAdd.Count > 0)
            {
                StarterDeckPaginator manager = __instance.gameObject.AddComponent<StarterDeckPaginator>();
                manager.Initialize(__instance);
                foreach (List<StarterDeckInfo> page in pagesToAdd)
                {
                    manager.AddPage(page);
                }
                Vector3 topRight = new Vector3(float.MinValue, float.MinValue);
                Vector3 bottomLeft = new Vector3(float.MaxValue, float.MaxValue);
                foreach (AscensionStarterDeckIcon icon in icons)
                {
                    if (icon != null && icon.iconRenderer != null)
                    {
                        if (icon.iconRenderer.transform.position.x < bottomLeft.x)
                        {
                            bottomLeft.x = icon.iconRenderer.transform.position.x;
                        }
                        if (icon.iconRenderer.transform.position.x > topRight.x)
                        {
                            topRight.x = icon.iconRenderer.transform.position.x;
                        }
                        if (icon.iconRenderer.transform.position.y < bottomLeft.y)
                        {
                            bottomLeft.y = icon.iconRenderer.transform.position.y;
                        }
                        if (icon.iconRenderer.transform.position.y > topRight.y)
                        {
                            topRight.y = icon.iconRenderer.transform.position.y;
                        }
                    }
                }
                GameObject leftArrow = UnityEngine.Object.Instantiate(__instance.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageLeftButton.gameObject);
                leftArrow.transform.parent = __instance.transform;
                leftArrow.transform.position = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.left / 2f;
                leftArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                leftArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.PreviousPage();
                GameObject rightArrow = UnityEngine.Object.Instantiate(__instance.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject);
                rightArrow.transform.parent = __instance.transform;
                rightArrow.transform.position = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + Vector3.right / 2f;
                rightArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                rightArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.NextPage();
            }
        }
    }
}