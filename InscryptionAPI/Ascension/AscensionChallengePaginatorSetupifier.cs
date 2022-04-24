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
                    ChallengeManager.SyncChallengeList();
                    AscensionChallengePaginator manager = challengeScreen.gameObject.AddComponent<AscensionChallengePaginator>();
                    manager.Initialize(null, screen);
                }
            }
        }
    }
}
