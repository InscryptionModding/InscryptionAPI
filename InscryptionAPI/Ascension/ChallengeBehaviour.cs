using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Ascension
{
    public class ChallengeBehaviour : NonCardTriggerReceiver
    {
        private static List<ChallengeBehaviour> instances;
        public ChallengeManager.FullChallenge challenge;

        public void Start()
        {
            if (!Instances.Contains(this))
            {
                Instances.Add(this);
            }
        }

        public new void OnDestroy()
        {
            Instances.Remove(this);
        }

        public static List<ChallengeBehaviour> FindInstancesOfType(AscensionChallenge type)
        {
            return Instances.FindAll((x) => x.challenge.Info.challengeType == type);
        }

        public static int CountInstancesOfType(AscensionChallenge type)
        {
            return FindInstancesOfType(type).Count;
        }

        public static bool AnyInstancesOfType(AscensionChallenge type)
        {
            return CountInstancesOfType(type) > 0;
        }

        public static List<ChallengeBehaviour> Instances
        {
            get
            {
                EnsureInstancesLoaded();
                return instances;
            }
        }

        public static void DestroyAllInstances()
        {
            List<ChallengeBehaviour> instance = Instances;
            foreach (ChallengeBehaviour ins in instance)
            {
                if (ins != null && ins.gameObject != null)
                {
                    Destroy(ins.gameObject);
                }
            }
            EnsureInstancesLoaded();
        }

        public static void EnsureInstancesLoaded()
        {
            if (instances == null)
            {
                instances = new List<ChallengeBehaviour>();
            }
            instances.RemoveAll((x) => x == null || x.gameObject == null);
        }

        public void ShowActivation()
        {
            ChallengeActivationUI.TryShowActivation(challenge.Info.challengeType);
        }

        public virtual bool RespondToPreBattleStart()
        {
            return false;
        }

        public virtual IEnumerator OnPreBattleStart()
        {
            yield break;
        }

        public virtual bool RespondToBattleStart()
        {
            return false;
        }

        public virtual IEnumerator OnBattleStart()
        {
            yield break;
        }

        public virtual bool RespondToPreCleanup()
        {
            return false;
        }

        public virtual IEnumerator OnPreCleanup()
        {
            yield break;
        }

        public virtual bool RespondToPostCleanup()
        {
            return false;
        }

        public virtual IEnumerator OnPostCleanup()
        {
            yield break;
        }
    }
}
