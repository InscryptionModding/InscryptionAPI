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
				if(ins != null && ins.gameObject != null)
				{
					Destroy(ins.gameObject);
				}
            }
			EnsureInstancesLoaded();
        }

        public static void EnsureInstancesLoaded()
        {
            if(instances == null)
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

		#region overrides
		public virtual bool RespondToUpkeep(bool playerUpkeep)
		{
			return false;
		}

		public virtual bool RespondToDrawn()
		{
			return false;
		}

		public virtual bool RespondToOtherCardDrawn(PlayableCard card)
		{
			return false;
		}

		public virtual bool RespondToPlayFromHand()
		{
			return false;
		}

		public virtual bool RespondToResolveOnBoard()
		{
			return false;
		}

		public virtual bool RespondToOtherCardResolve(PlayableCard otherCard)
		{
			return false;
		}

		public virtual bool RespondToOtherCardAssignedToSlot(PlayableCard otherCard)
		{
			return false;
		}

		public virtual bool RespondToSacrifice()
		{
			return false;
		}

		public virtual bool RespondToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			return false;
		}

		public virtual bool RespondToCardGettingAttacked(PlayableCard source)
		{
			return false;
		}

		public virtual bool RespondToTakeDamage(PlayableCard source)
		{
			return false;
		}

		public virtual bool RespondToDealDamage(int amount, PlayableCard target)
		{
			return false;
		}

		public virtual bool RespondToDealDamageDirectly(int amount)
		{
			return false;
		}

		public virtual bool RespondToOtherCardDealtDamage(PlayableCard attacker, int amount, PlayableCard target)
		{
			return false;
		}

		public virtual bool RespondToDie(bool wasSacrifice, PlayableCard killer)
		{
			return false;
		}

		public virtual bool RespondToPreDeathAnimation(bool wasSacrifice)
		{
			return false;
		}

		public virtual bool RespondToOtherCardPreDeath(CardSlot deathSlot, bool fromCombat, PlayableCard killer)
		{
			return false;
		}

		public virtual bool RespondToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
		{
			return false;
		}

		public virtual bool RespondToAttackEnded()
		{
			return false;
		}

		public virtual bool RespondToTurnEnd(bool playerTurnEnd)
		{
			return false;
		}

		public virtual bool RespondToActivatedAbility(Ability ability)
		{
			return false;
		}

		public override sealed bool RespondsToUpkeep(bool playerUpkeep)
		{
			return SaveFile.IsAscension && RespondToUpkeep(playerUpkeep);
		}

		public override sealed bool RespondsToDrawn()
		{
			return SaveFile.IsAscension && RespondToDrawn();
		}

		public override sealed bool RespondsToOtherCardDrawn(PlayableCard card)
		{
			return SaveFile.IsAscension && RespondToOtherCardDrawn(card);
		}

		public override sealed bool RespondsToPlayFromHand()
		{
			return SaveFile.IsAscension && RespondToPlayFromHand();
		}

		public override sealed bool RespondsToResolveOnBoard()
		{
			return SaveFile.IsAscension && RespondToResolveOnBoard();
		}

		public override sealed bool RespondsToOtherCardResolve(PlayableCard otherCard)
		{
			return SaveFile.IsAscension && RespondToOtherCardResolve(otherCard);
		}

		public override sealed bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
		{
			return SaveFile.IsAscension && RespondToOtherCardAssignedToSlot(otherCard);
		}

		public override sealed bool RespondsToSacrifice()
		{
			return SaveFile.IsAscension && RespondToSacrifice();
		}

		public override sealed bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			return SaveFile.IsAscension && RespondToSlotTargetedForAttack(slot, attacker);
		}

		public override sealed bool RespondsToCardGettingAttacked(PlayableCard source)
		{
			return SaveFile.IsAscension && RespondToCardGettingAttacked(source);
		}

		public override sealed bool RespondsToTakeDamage(PlayableCard source)
		{
			return SaveFile.IsAscension && RespondToTakeDamage(source);
		}

		public override sealed bool RespondsToDealDamage(int amount, PlayableCard target)
		{
			return SaveFile.IsAscension && RespondToDealDamage(amount, target);
		}

		public override sealed bool RespondsToDealDamageDirectly(int amount)
		{
			return SaveFile.IsAscension && RespondToDealDamageDirectly(amount);
		}

		public override sealed bool RespondsToOtherCardDealtDamage(PlayableCard attacker, int amount, PlayableCard target)
		{
			return SaveFile.IsAscension && RespondToOtherCardDealtDamage(attacker, amount, target);
		}

		public override sealed bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
		{
			return SaveFile.IsAscension && RespondToDie(wasSacrifice, killer);
		}

		public override sealed bool RespondsToPreDeathAnimation(bool wasSacrifice)
		{
			return SaveFile.IsAscension && RespondToPreDeathAnimation(wasSacrifice);
		}

		public override sealed bool RespondsToOtherCardPreDeath(CardSlot deathSlot, bool fromCombat, PlayableCard killer)
		{
			return SaveFile.IsAscension && RespondToOtherCardPreDeath(deathSlot, fromCombat, killer);
		}

		public override sealed bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
		{
			return SaveFile.IsAscension && RespondToOtherCardDie(card, deathSlot, fromCombat, killer);
		}

		public override sealed bool RespondsToAttackEnded()
		{
			return SaveFile.IsAscension && RespondToAttackEnded();
		}

		public override sealed bool RespondsToTurnEnd(bool playerTurnEnd)
		{
			return SaveFile.IsAscension && RespondToTurnEnd(playerTurnEnd);
		}

		public override sealed bool RespondsToActivatedAbility(Ability ability)
		{
			return SaveFile.IsAscension && RespondToActivatedAbility(ability);
		}
        #endregion
    }
}
