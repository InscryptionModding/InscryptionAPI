using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System.Collections;
using UnityEngine;
namespace InscryptionAPI.Card;

/// <summary>
/// A version of <see cref="ActivatedAbilityBehaviour"/> that allows for the creation of activated abilities that cost Blood, Gems, or Health. Also supports dynamic activation costs.
/// </summary>
public abstract class ExtendedActivatedAbilityBehaviour : AbilityBehaviour
{
    /// <summary>
    /// Stores the total Blood activation cost modifier.
    /// </summary>
    public int bloodCostMod;
    /// <summary>
    /// Stores the total Bones activation cost modifier.
    /// </summary>
    public int bonesCostMod;
    /// <summary>
    /// Stores the total Energy activation cost modifier.
    /// </summary>
    public int energyCostMod;
    /// <summary>
    /// Stores the total added Gems activation requirement modifier.
    /// </summary>
    public List<GemType> gemsCostMod = new();
    /// <summary>
    /// Stores the total removed Gems activation requirement modifier.
    /// </summary>
    public List<GemType> gemsCostRemovedMod = new();
    /// <summary>
    /// Stores the total Blood activation cost modifier.
    /// </summary>
    public int healthCostMod;

    /// <summary>
    /// Initial amount of Blood needed to activate this ability.
    /// </summary>
    public virtual int StartingBloodCost { get; } = 0;
    /// <summary>
    /// Initial amount of Bones needed to activate this ability.
    /// </summary>
    public virtual int StartingBonesCost { get; } = 0;
    /// <summary>
    /// Initial amount of Energy needed to activate this ability.
    /// </summary>
    public virtual int StartingEnergyCost { get; } = 0;
    /// <summary>
    /// Initial types of Gems needed to activate this ability.
    /// </summary>
    public virtual List<GemType> StartingGemsCost { get; } = null;
    /// <summary>
    /// Initial amount of Health needed to activate this ability.
    /// </summary>
    public virtual int StartingHealthCost { get; }
    /// <summary>
    /// Amount the Blood activation cost is modified whenever this ability is activated.
    /// </summary>
    public virtual int OnActivateBloodCostMod { get; set; }
    /// <summary>
    /// Amount the Bones activation cost is modified whenever this ability is activated.
    /// </summary>
    public virtual int OnActivateBonesCostMod { get; set; }
    /// <summary>
    /// Amount the Energy activation cost is modified whenever this ability is activated.
    /// </summary>
    public virtual int OnActivateEnergyCostMod { get; set; }
    /// <summary>
    /// What Gems are added to the activation requirement whenever this ability is activated.
    /// </summary>
    public virtual List<GemType> OnActivateGemsCostMod { get; set; }
    /// <summary>
    /// What Gems are removed from the activation requirement whenever this ability is activated.
    /// </summary>
    public virtual bool OnActivateGemsCostModRemovesGems { get; set; }
    /// <summary>
    /// Amount the Health activation cost is modified whenever this ability is activated.
    /// </summary>
    public virtual int OnActivateHealthCostMod { get; set; }

    /// <summary>
    /// How much Blood this ability currently costs to activate.
    /// </summary>
    public int BloodCost => Mathf.Max(0, StartingBloodCost + bloodCostMod);
    /// <summary>
    /// How much Bones this ability currently costs to activate.
    /// </summary>
    public int BonesCost => Mathf.Max(0, StartingBonesCost + bonesCostMod);
    /// <summary>
    /// How much Energy this ability currently costs to activate.
    /// </summary>
    public int EnergyCost => Mathf.Max(0, StartingEnergyCost + energyCostMod);
    /// <summary>
    /// What Gems this ability currently requires to activate.
    /// </summary>
    public List<GemType> GemsCost {
        get {
            List<GemType> retval = new();
            if (StartingGemsCost != null && StartingGemsCost.Count > 0) {
                retval.AddRange(StartingGemsCost);
            }
            retval.AddRange(gemsCostMod);
            foreach (GemType gem in gemsCostRemovedMod) {
                retval.Remove(gem);
            }
            return retval;
        }
    }
    /// <summary>
    /// How much Health this ability currently costs to activate.
    /// </summary>
    public int HealthCost => Mathf.Max(0, StartingHealthCost + healthCostMod);

    /// <summary>
    /// Dictionary of <see cref="CardSlot"/>s that have been selected for sacrifices and the associated CardInfo.
    /// </summary>
    public Dictionary<CardSlot, CardInfo> currentSacrificedCardInfos = new();

    /// <summary>
    /// Determines if <see cref="OnPostResolveOnBoard"/> should activate.
    /// </summary>
    /// <returns>True if <see cref="OnPostResolveOnBoard"/> should activate.</returns>
    public virtual bool RespondsToPostResolveOnBoard()
    {
        return false;
    }
    /// <summary>
    /// Trigger that's called after card's with this ability resolve on the board.
    /// Used since <see cref="OnResolveOnBoard"/> is sealed to prevent the ability breaking.
    /// </summary>
    public virtual IEnumerator OnPostResolveOnBoard()
    {
        yield break;
    }
    /// <summary>
    /// If the <see cref="OnResolveOnBoard"/> trigger should be called.
    /// </summary>
    /// <returns>True if it should be activated.</returns>
    public sealed override bool RespondsToResolveOnBoard() => RespondsToPostResolveOnBoard() || LearnMechanic();
    /// <summary>
    /// Runs the logic that teaches the player how activated abilities work in Act 2, plus any <see cref="OnPostResolveOnBoard"/> logic.
    /// </summary>
    public sealed override IEnumerator OnResolveOnBoard()
    {
        if (LearnMechanic()) // Act 2 tutorial dialogue
        {
            yield return new WaitForSeconds(0.15f);
            if (StoryEventsData.EventCompleted(StoryEvent.GBCUndeadAmbition))
                yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent("ActivatedAbilityTutorial", TextBox.Style.Undead, Singleton<InBattleDialogueSpeakers>.Instance.GetSpeaker(DialogueSpeaker.Character.Grimora), null, TextBox.ScreenPosition.ForceTop);

            else if (StoryEventsData.EventCompleted(StoryEvent.GBCNatureAmbition))
                yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent("ActivatedAbilityTutorial", TextBox.Style.Nature, Singleton<InBattleDialogueSpeakers>.Instance.GetSpeaker(DialogueSpeaker.Character.Leshy), null, TextBox.ScreenPosition.ForceTop);

            else if (StoryEventsData.EventCompleted(StoryEvent.GBCTechAmbition))
                yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent("ActivatedAbilityTutorial", TextBox.Style.Tech, Singleton<InBattleDialogueSpeakers>.Instance.GetSpeaker(DialogueSpeaker.Character.P03), null, TextBox.ScreenPosition.ForceTop);

            else
                yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent("ActivatedAbilityTutorial", TextBox.Style.Magic, Singleton<InBattleDialogueSpeakers>.Instance.GetSpeaker(DialogueSpeaker.Character.Magnificus), null, TextBox.ScreenPosition.ForceTop);
        }

        if (RespondsToPostResolveOnBoard())
            yield return OnPostResolveOnBoard();
    }
    /// <summary>
    /// Whetehr this ability should be triggered.
    /// </summary>
    /// <param name="ability">The ability being checked for activation.</param>
    /// <returns>True if this ability is being checked for activation.</returns>
    public sealed override bool RespondsToActivatedAbility(Ability ability) => this.Ability == ability;
    /// <summary>
    /// The logic sequence for when this ability is activated.
    /// Contains logic for paying activation costs.
    /// </summary>
    public sealed override IEnumerator OnActivatedAbility()
    {
        if (CanAfford() && CanActivate())
        {
            if (BloodCost > 0)
            {
                yield return ChooseSacrifices();

                if (Singleton<BoardManager>.Instance.CancelledSacrifice)
                    yield break;
            }

            if (HealthCost > 0)
            {
                base.Card.Anim.LightNegationEffect();
                base.Card.Status.damageTaken += HealthCost;
            }

            if (EnergyCost > 0)
            {
                yield return Singleton<ResourcesManager>.Instance.SpendEnergy(EnergyCost);
                if (Singleton<ConduitCircuitManager>.Instance != null)
                {
                    CardSlot cardSlot = Singleton<BoardManager>.Instance.GetSlots(getPlayerSlots: true).Find((CardSlot x) => x.Card != null && x.Card.HasAbility(Ability.ConduitEnergy));
                    if (cardSlot != null)
                    {
                        ConduitEnergy component = cardSlot.Card.GetComponent<ConduitEnergy>();
                        if (component != null && component.CompletesCircuit())
                            yield return Singleton<ResourcesManager>.Instance.AddEnergy(EnergyCost);
                    }
                }
            }
            if (BonesCost > 0) {
                yield return Singleton<ResourcesManager>.Instance.SpendBones(BonesCost);
            }

            yield return new WaitForSeconds(0.1f);
            yield return base.PreSuccessfulTriggerSequence();
            yield return Activate();
            ProgressionData.SetMechanicLearned(MechanicsConcept.GBCActivatedAbilities);

            if (HealthCost > 0) // card still exists and has 0 Health
            {
                if (base.Card && base.Card.Health <= 0 && !base.Card.Dead)
                {
                    yield return base.Card.Die(true);
                    yield return PostActivate();
                    currentSacrificedCardInfos.Clear();
                    yield break;
                }
            }
            int energyMod = OnActivateEnergyCostMod;
            int bonesMod = OnActivateBonesCostMod;
            List<GemType> gemsMod = OnActivateGemsCostMod;
            int healthMod = OnActivateHealthCostMod;

            if (energyMod != 0)
                energyCostMod += energyMod;

            if (bonesMod != 0)
                bonesCostMod += bonesMod;

            if (gemsMod != null && gemsMod.Count > 0) {
                if (OnActivateGemsCostModRemovesGems) {
                    gemsMod.ForEach(x => gemsCostMod.Remove(x));
                }
                else {
                    gemsCostMod.AddRange(gemsMod);
                }
            }

            if (healthMod != 0)
                healthCostMod += healthMod;

            yield return PostActivate();
            currentSacrificedCardInfos.Clear();
        }
        else
        {
            base.Card.Anim.LightNegationEffect();
            AudioController.Instance.PlaySound2D("toneless_negate", MixerGroup.GBCSFX, 0.2f);
            yield return new WaitForSeconds(0.25f);
        }
    }
    /// <summary>
    /// Sequence called after the ability finishes activation.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator PostActivate()
    {
        yield break;
    }
    /// <summary>
    /// Whether or not this ability can be activated.
    /// </summary>
    /// <returns>true if this ability can be activated.</returns>
    public virtual bool CanActivate() => true;
    /// <summary>
    /// Player-implemented sequence for what this ability will actually do when activated.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator Activate();

    private IEnumerator ChooseSacrifices()
    {
        BoardManager manager = Singleton<BoardManager>.Instance;
        List<CardSlot> validSlots = manager.GetCardSlots(
            !base.Card.OpponentCard, x => x.Card != null & x.Card != base.Card);

        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
        Singleton<ViewManager>.Instance.SwitchToView(manager.BoardView);
        Singleton<InteractionCursor>.Instance.ForceCursorType(CursorType.Sacrifice);
        manager.cancelledPlacementWithInput = false;
        manager.currentValidSlots = validSlots;
        manager.currentSacrificeDemandingCard = base.Card;
        manager.CancelledSacrifice = false;
        manager.LastSacrificesInfo.Clear();
        manager.SetQueueSlotsEnabled(false);
        foreach (CardSlot allSlot in manager.AllSlots.Where(x => x.Card != base.Card))
        {
            if (!allSlot.IsPlayerSlot || allSlot.Card == null)
            {
                allSlot.SetEnabled(enabled: false);
                allSlot.ShowState(HighlightedInteractable.State.NonInteractable);
            }
            if (allSlot.IsPlayerSlot && allSlot.Card != null && allSlot.Card.CanBeSacrificed)
                allSlot.Card.Anim.SetShaking(true);
        }
        yield return manager.SetSacrificeMarkersShown(BloodCost);
        while (manager.GetValueOfSacrifices(manager.currentSacrifices) < BloodCost && !manager.cancelledPlacementWithInput)
        {
            manager.SetSacrificeMarkersValue(manager.currentSacrifices.Count);
            yield return new WaitForEndOfFrame();
        }

        foreach (CardSlot allSlot2 in manager.AllSlots)
        {
            allSlot2.SetEnabled(false);
            if (allSlot2.IsPlayerSlot && allSlot2.Card != null)
                allSlot2.Card.Anim.SetShaking(shaking: false);
        }

        foreach (CardSlot currentSacrifice in manager.currentSacrifices)
            manager.LastSacrificesInfo.Add(currentSacrifice.Card.Info);

        if (manager.cancelledPlacementWithInput)
        {
            manager.HideSacrificeMarkers();

            foreach (CardSlot slot in manager.GetSlots(getPlayerSlots: true))
            {
                if (slot.Card != null)
                {
                    slot.Card.Anim.SetSacrificeHoverMarkerShown(false);
                    if (manager.currentSacrifices.Contains(slot))
                        slot.Card.Anim.SetMarkedForSacrifice(false);
                }
            }
            Singleton<ViewManager>.Instance.SwitchToView(manager.defaultView);
            Singleton<InteractionCursor>.Instance.ClearForcedCursorType();
            manager.CancelledSacrifice = true;
        }
        else
        {
            manager.SetSacrificeMarkersValue(manager.GetValueOfSacrifices(manager.currentSacrifices));
            yield return new WaitForSeconds(0.2f);
            manager.HideSacrificeMarkers();
            foreach (CardSlot currentSacrifice2 in manager.currentSacrifices)
            {
                if (currentSacrifice2.Card != null && !currentSacrifice2.Card.Dead)
                {
                    manager.SacrificesMadeThisTurn++;
                    currentSacrificedCardInfos.Add(currentSacrifice2, currentSacrifice2.Card.Info);
                    yield return currentSacrifice2.Card.Sacrifice();
                    Singleton<ViewManager>.Instance.SwitchToView(manager.BoardView);
                }
            }
        }
        manager.SetQueueSlotsEnabled(slotsEnabled: true);
        foreach (CardSlot allSlot3 in manager.AllSlots)
        {
            allSlot3.SetEnabled(enabled: true);
            allSlot3.ShowState(HighlightedInteractable.State.Interactable);
        }
        manager.currentSacrificeDemandingCard = null;
        manager.currentSacrifices.Clear();
    }

    private bool CanAfford() {
        // if no blood cost or we can fulfill our blood cost
        if (BloodCost < 1 || SacrificeValue() >= BloodCost) {
            // if we have enough health, Energy, and Bones
            if (base.Card.Health >= HealthCost && ResourcesManager.Instance.PlayerEnergy >= EnergyCost && ResourcesManager.Instance.PlayerBones >= BonesCost) {
                bool enoughOrange = ResourcesManagerHelpers.GemCount(!base.Card.OpponentCard, GemType.Orange) >= GemsCost.Count(x => x == GemType.Orange);
                bool enoughGreen = ResourcesManagerHelpers.GemCount(!base.Card.OpponentCard, GemType.Green) >= GemsCost.Count(x => x == GemType.Green);
                bool enoughBlue = ResourcesManagerHelpers.GemCount(!base.Card.OpponentCard, GemType.Blue) >= GemsCost.Count(x => x == GemType.Blue);

                // if we have enough gems
                return enoughOrange && enoughGreen && enoughBlue;
            }
        }
        return false;
    }
    private bool LearnMechanic() => SaveManager.SaveFile.IsPart2 && !ProgressionData.LearnedMechanic(MechanicsConcept.GBCActivatedAbilities);
    private int SacrificeValue()
    {
        return Singleton<BoardManager>.Instance.GetValueOfSacrifices(
            Singleton<BoardManager>.Instance.GetCardSlots(!base.Card.OpponentCard, x => x.Card != null && x.Card != base.Card));
    }
}

[HarmonyPatch(typeof(CardSlot))]
internal class CardSlotPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(CardSlot.OnCursorEnter))]
    private static void DisableSacrificeMarker(CardSlot __instance)
    {
        if (__instance.Card != null && Singleton<BoardManager>.Instance.ChoosingSacrifices)
        {
            if (__instance.Card == Singleton<BoardManager>.Instance.CurrentSacrificeDemandingCard)
                __instance.Card.Anim.SetSacrificeHoverMarkerShown(false);
        }
    }
}
