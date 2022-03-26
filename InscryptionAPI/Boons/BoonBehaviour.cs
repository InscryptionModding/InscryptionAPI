using DiskCardGame;
using System.Collections;

namespace InscryptionAPI.Boons
{
    public class BoonBehaviour : NonCardTriggerReceiver
    {
        private static List<BoonBehaviour> instances;
        public BoonManager.FullBoon boon;
		public int instanceNumber;
		public int InstanceIndex
        {
            get
            {
				return instanceNumber - 1;
            }
        }

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

		public static List<BoonBehaviour> FindInstancesOfType(BoonData.Type type)
        {
			return Instances.FindAll((x) => x.boon.boon.type == type);
		}

		public static int CountInstancesOfType(BoonData.Type type)
        {
			return FindInstancesOfType(type).Count;
        }

		public static bool AnyInstancesOfType(BoonData.Type type)
        {
			return CountInstancesOfType(type) > 0;
        }

        public static List<BoonBehaviour> Instances
        {
            get
            {
                EnsureInstancesLoaded();
                return instances;
            }
        }

		public static void DestroyAllInstances()
        {
			List<BoonBehaviour> instance = Instances;
			foreach (BoonBehaviour ins in instance)
            {
				if(ins != null && ins.gameObject != null)
				{
					Destroy(ins.gameObject);
				}
            }
			EnsureInstancesLoaded();
			Instances.Clear();
        }

        public static void EnsureInstancesLoaded()
        {
            if(instances == null)
            {
                instances = new List<BoonBehaviour>();
            }
			instances.RemoveAll((x) => x == null || x.gameObject == null);
        }

        public IEnumerator PlayBoonAnimation()
        {
            if(Singleton<BoonsHandler>.Instance != null)
            {
                yield return BoonsHandler.Instance.PlayBoonAnimation(boon.boon.type);
            }
            yield break;
        }

		public virtual bool RespondToPreBoonActivation()
		{
			return false;
		}

		public virtual IEnumerator OnPreBoonActivation()
		{
			yield break;
		}

		public virtual bool RespondToPostBoonActivation()
		{
			return false;
		}

		public virtual IEnumerator OnPostBoonActivation()
		{
			yield break;
		}

		public virtual bool RespondToPreBattleCleanup()
		{
			return false;
		}

		public virtual IEnumerator OnPreBattleCleanup()
		{
			yield break;
		}

		public virtual bool RespondToPostBattleCleanup()
		{
			return false;
		}

		public virtual IEnumerator OnPostBattleCleanup()
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
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToUpkeep(playerUpkeep);
		}

		public override sealed bool RespondsToDrawn()
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToDrawn();
		}

		public override sealed bool RespondsToOtherCardDrawn(PlayableCard card)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToOtherCardDrawn(card);
		}

		public override sealed bool RespondsToPlayFromHand()
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToPlayFromHand();
		}

		public override sealed bool RespondsToResolveOnBoard()
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToResolveOnBoard();
		}

		public override sealed bool RespondsToOtherCardResolve(PlayableCard otherCard)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToOtherCardResolve(otherCard);
		}

		public override sealed bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToOtherCardAssignedToSlot(otherCard);
		}

		public override sealed bool RespondsToSacrifice()
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToSacrifice();
		}

		public override sealed bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToSlotTargetedForAttack(slot, attacker);
		}

		public override sealed bool RespondsToCardGettingAttacked(PlayableCard source)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToCardGettingAttacked(source);
		}

		public override sealed bool RespondsToTakeDamage(PlayableCard source)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToTakeDamage(source);
		}

		public override sealed bool RespondsToDealDamage(int amount, PlayableCard target)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToDealDamage(amount, target);
		}

		public override sealed bool RespondsToDealDamageDirectly(int amount)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToDealDamageDirectly(amount);
		}

		public override sealed bool RespondsToOtherCardDealtDamage(PlayableCard attacker, int amount, PlayableCard target)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToOtherCardDealtDamage(attacker, amount, target);
		}

		public override sealed bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToDie(wasSacrifice, killer);
		}

		public override sealed bool RespondsToPreDeathAnimation(bool wasSacrifice)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToPreDeathAnimation(wasSacrifice);
		}

		public override sealed bool RespondsToOtherCardPreDeath(CardSlot deathSlot, bool fromCombat, PlayableCard killer)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToOtherCardPreDeath(deathSlot, fromCombat, killer);
		}

		public override sealed bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToOtherCardDie(card, deathSlot, fromCombat, killer);
		}

		public override sealed bool RespondsToAttackEnded()
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToAttackEnded();
		}

		public override sealed bool RespondsToTurnEnd(bool playerTurnEnd)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToTurnEnd(playerTurnEnd);
		}

		public override sealed bool RespondsToActivatedAbility(Ability ability)
		{
			return BoonsHandler.Instance != null && BoonsHandler.Instance.BoonsEnabled && RespondToActivatedAbility(ability);
		}
        #endregion
    }
}
