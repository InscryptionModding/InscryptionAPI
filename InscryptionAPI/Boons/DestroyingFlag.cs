using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Boons
{
    internal class DestroyingFlag : MonoBehaviour
    {
        public IEnumerator WaitForStackClearThenDestroy()
        {
            yield return new WaitUntil(() => GlobalTriggerHandler.Instance == null || GlobalTriggerHandler.Instance.StackSize <= 0);
            if(gameObject != null)
            {
                Destroy(gameObject);
            }
            yield break;
        }
    }
}
