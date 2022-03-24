using DiskCardGame;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Ascension
{
    public class AscensionChallengePaginatorSetupifier : ManagedBehaviour
    {
        public void Start()
        {
            if (gameObject.GetComponent<AscensionMenuScreenTransition>() != null)
            {
                List<T> Repeat<T>(T h, int h2)
                {
                    List<T> ret = new();
                    for (int i = 0; i < h2; i++)
                    {
                        ret.Add(h);
                    }
                    return ret;
                }
                GameObject challengeScreen = gameObject;
                AscensionMenuScreenTransition screen = challengeScreen.GetComponent<AscensionMenuScreenTransition>();
                List<AscensionIconInteractable> icons = new(screen.screenInteractables.FindAll((x) => x.GetComponent<AscensionIconInteractable>() != null).ConvertAll((x) =>
                    x.GetComponent<AscensionIconInteractable>()));
                if (icons.Count > 0 && challengeScreen.GetComponent<AscensionChallengePaginator>() == null)
                {
                    AscensionChallengePaginator manager = challengeScreen.gameObject.AddComponent<AscensionChallengePaginator>();
                    manager.Initialize(null, screen);
                    List<ChallengeManager.FullChallenge> fullchallengesToAdd = new(ChallengeManager.NewInfos);
                    fullchallengesToAdd.Sort((x, x2) => Math.Sign(x.Info.pointValue) == Math.Sign(x2.Info.pointValue) ? x.UnlockLevel - x2.UnlockLevel : Math.Sign(x2.Info.pointValue) - Math.Sign(x.Info.pointValue));
                    List<AscensionChallengeInfo> challengesToAdd = fullchallengesToAdd.ConvertAll(x => Repeat(x.Info, x.AppearancesInChallengeScreen)).SelectMany(x => x).ToList();
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
                                page.Add(challengesToAdd[0]);
                                challengesToAdd.RemoveAt(0);
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
