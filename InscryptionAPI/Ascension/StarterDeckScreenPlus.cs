using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Ascension
{
    internal class StarterDeckScreenPlus : ManagedBehaviour
    {
        public IEnumerator ShowCardsSequence()
        {
            tempCards ??= new();
            tempCards.ForEach(delegate (GameObject x)
            {
                x.SetActive(false);
            });
            foreach (GameObject obj in tempCards)
            {
                yield return new WaitForSeconds(0.1f);
                obj.SetActive(true);
                CommandLineTextDisplayer.PlayCommandLineClickSound();
            }
            yield break;
        }

        public List<GameObject> tempCards = new();
        public List<GameObject> noneCardObjects = new();
    }
}
