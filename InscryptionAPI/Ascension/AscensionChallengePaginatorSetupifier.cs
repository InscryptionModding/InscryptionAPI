using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DiskCardGame;
using InscryptionAPI.Helpers.Extensions;

namespace InscryptionAPI.Ascension
{
    public class AscensionChallengePaginatorSetupifier : ManagedBehaviour
    {
        public void Start()
        {
            if (gameObject.GetComponent<AscensionMenuScreenTransition>() != null)
            {
                GameObject challengeScreen = gameObject;
                AscensionMenuScreenTransition screen = challengeScreen.GetComponent<AscensionMenuScreenTransition>();
                List<AscensionIconInteractable> icons = new(screen.screenInteractables.FindAll((x) => x.GetComponent<AscensionIconInteractable>() != null).ConvertAll((x) =>
                    x.GetComponent<AscensionIconInteractable>()));
                if (icons.Count > 0 && challengeScreen.GetComponent<AscensionChallengePaginator>() == null)
                {
                    AscensionChallengePaginator manager = challengeScreen.gameObject.AddComponent<AscensionChallengePaginator>();
                    manager.Initialize(null, screen);
                    ChallengeManager.SyncChallengeList();
                    List<ChallengeManager.FullChallenge> fcs = ChallengeManager.NewInfos.ToList();
                    fcs.Sort((x, x2) => x.SortValue != x2.SortValue ? x.SortValue - x2.SortValue : x2.UnlockLevel - x.UnlockLevel);
                    List<AscensionChallengeInfo> challengesToAdd = new(fcs.ConvertAll(x => x.Challenge.Repeat(x.AppearancesInChallengeScreen)).SelectMany(x => x));
                    icons.ForEach(delegate (AscensionIconInteractable ic)
                    {
                        if (ic != null && ic.Info == null && challengesToAdd.Count > 0)
                        {
                            ic.challengeInfo = challengesToAdd[0];
                            ic.AssignInfo(challengesToAdd[0]);
                            challengesToAdd.RemoveAt(0);
                        }
                    });
                    List<List<AscensionChallengeInfo>> pagesToAdd = new();
                    while (challengesToAdd.Count > 0)
                    {
                        List<AscensionChallengeInfo> page = new();
                        for (int i = 0; i < icons.Count; i++)
                        {
                            if (challengesToAdd.Count > 0)
                            {
                                page.Add(challengesToAdd.Last());
                                challengesToAdd.RemoveAt(challengesToAdd.Count - 1);
                            }
                        }
                        pagesToAdd.Add(page);
                    }
                    if (pagesToAdd.Count > 0)
                    {
                        foreach (List<AscensionChallengeInfo> page in pagesToAdd)
                        {
                            manager.AddPage(page);
                        }
                        Vector3 topRight = new(float.MinValue, float.MinValue);
                        Vector3 bottomLeft = new(float.MaxValue, float.MaxValue);
                        foreach (AscensionIconInteractable icon in icons)
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
                        GameObject leftArrow = Instantiate(challengeScreen.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageLeftButton.gameObject);
                        leftArrow.transform.parent = challengeScreen.transform;
                        leftArrow.transform.position = Vector3.Lerp(new Vector3(bottomLeft.x, topRight.y, topRight.z), new Vector3(bottomLeft.x, bottomLeft.y, topRight.z), 0.5f) + (Vector3.left / 3f);
                        leftArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                        leftArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.PreviousPage();
                        GameObject rightArrow = Instantiate(challengeScreen.GetComponentInParent<AscensionMenuScreens>().cardUnlockSummaryScreen.GetComponent<AscensionCardsSummaryScreen>().pageRightButton.gameObject);
                        rightArrow.transform.parent = challengeScreen.transform;
                        rightArrow.transform.position = Vector3.Lerp(new Vector3(topRight.x, topRight.y, topRight.z), new Vector3(topRight.x, bottomLeft.y, topRight.z), 0.5f) + (Vector3.right / 3f);
                        rightArrow.GetComponent<AscensionMenuInteractable>().ClearDelegates();
                        rightArrow.GetComponent<AscensionMenuInteractable>().CursorSelectStarted += (x) => manager.NextPage();
                    }
                }
            }
        }
    }
}
