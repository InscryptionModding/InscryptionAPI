using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes a number of warnings that get spammed in Act 2 by removing the cause of the warnings
[HarmonyPatch]
internal class Act2LogSpamFixes
{
    // For parts of the game that aren't needed in Act 2
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameFlowManager), nameof(GameFlowManager.UpdateForTransitionToFirstPerson))]
    [HarmonyPatch(typeof(ViewManager), nameof(ViewManager.UpdateViewControlsHintsForView))]
    private static bool DisableInAct2() => !SaveManager.SaveFile.IsPart2;

    // Since GameFlowManager is null in Act 2, there's no point in checking for that
    [HarmonyPatch(typeof(ConduitCircuitManager), nameof(ConduitCircuitManager.UpdateCircuits))]
    [HarmonyPrefix]
    private static bool RemoveConduitManagerSpam(ConduitCircuitManager __instance)
    {
        if (!SaveManager.SaveFile.IsPart2)
            return true;

        __instance.UpdateCircuitsForSlots(Singleton<BoardManager>.Instance.GetSlots(getPlayerSlots: true));
        __instance.UpdateCircuitsForSlots(Singleton<BoardManager>.Instance.GetSlots(getPlayerSlots: false));
        return false;
    }

    // Prevents log spam when you open a card pack with an activated sigil in it
    [HarmonyPatch(typeof(PixelActivatedAbilityButton), nameof(PixelActivatedAbilityButton.ManagedUpdate))]
    [HarmonyPrefix]
    private static bool RemovePixelActivatedSpam() => SceneLoader.ActiveSceneName == "GBC_CardBattle";

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
    [HarmonyPostfix]
    private static IEnumerator RemoveSetupPhaseSpam(IEnumerator enumerator, TurnManager __instance, EncounterData encounterData)
    {
        // moved here from PassiveAttackBuffPatches - always instantiate ConduitCircuitManager on setup
        if (ConduitCircuitManager.Instance == null)
            BoardManager.Instance.gameObject.AddComponent<ConduitCircuitManager>();

        // if this isn't Act 2, return the default logic
        if (!SaveManager.SaveFile.IsPart2)
        {
            yield return enumerator;
            yield break;
        }

        // Removes unnecessary checks for Boons and Rulebook
        __instance.IsSetupPhase = true;
        Singleton<PlayerHand>.Instance.PlayingLocked = true;
        if (__instance.SpecialSequencer != null)
            yield return __instance.SpecialSequencer.PreBoardSetup();

        yield return new WaitForSeconds(0.15f);
        yield return Singleton<LifeManager>.Instance.Initialize(__instance.SpecialSequencer == null || __instance.SpecialSequencer.ShowScalesOnStart);
        __instance.StartCoroutine(Singleton<BoardManager>.Instance.Initialize());
        __instance.StartCoroutine(Singleton<ResourcesManager>.Instance.Setup());
        yield return new WaitForSeconds(0.2f);
        yield return __instance.opponent.IntroSequence(encounterData);
        __instance.StartCoroutine(__instance.PlacePreSetCards(encounterData));
        if (__instance.SpecialSequencer != null)
            yield return __instance.SpecialSequencer.PreDeckSetup();

        Singleton<PlayerHand>.Instance.Initialize();
        yield return Singleton<CardDrawPiles>.Instance.Initialize();
        if (__instance.SpecialSequencer != null)
            yield return __instance.SpecialSequencer.PreHandDraw();

        yield return Singleton<CardDrawPiles>.Instance.DrawOpeningHand(__instance.GetFixedHand());
        if (__instance.opponent.QueueFirstCardBeforePlayer)
            yield return __instance.opponent.QueueNewCards(doTween: true, changeView: false);

        __instance.IsSetupPhase = false;
        yield break;
    }

    [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
    [HarmonyPostfix]
    private static IEnumerator RemoveAct2CleanupPhaseSpam(IEnumerator enumerator, TurnManager __instance)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            yield return enumerator;
            yield break;
        }

        // Removes unnecessary check for Rulebook
        __instance.PlayerWon = __instance.PlayerIsWinner();
        __instance.GameEnding = true;

        if (!__instance.PlayerWon && __instance.opponent != null && __instance.opponent.Blueprint != null)
            AnalyticsManager.SendFailedEncounterEvent(__instance.opponent.Blueprint, __instance.opponent.Difficulty, __instance.TurnNumber);

        if (__instance.SpecialSequencer != null)
            yield return __instance.SpecialSequencer.PreCleanUp();

        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
        if (__instance.PlayerWon && __instance.PostBattleSpecialNode == null)
            Singleton<ViewManager>.Instance.SwitchToView(View.MapDefault);

        else
            Singleton<ViewManager>.Instance.SwitchToView(View.Default);

        yield return new WaitForSeconds(0.1f);
        __instance.StartCoroutine(Singleton<PlayerHand>.Instance.CleanUp());
        __instance.StartCoroutine(Singleton<CardDrawPiles>.Instance.CleanUp());
        yield return __instance.opponent.CleanUp();
        yield return __instance.opponent.OutroSequence(__instance.PlayerWon);
        __instance.StartCoroutine(Singleton<BoardManager>.Instance.CleanUp());
        __instance.StartCoroutine(Singleton<ResourcesManager>.Instance.CleanUp());

        yield return Singleton<LifeManager>.Instance.CleanUp();
        if (__instance.SpecialSequencer != null)
            yield return __instance.SpecialSequencer.GameEnd(__instance.PlayerWon);

        UnityObject.Destroy(__instance.opponent.gameObject);

        Singleton<PlayerHand>.Instance.SetShown(shown: false);
        UnityObject.Destroy(__instance.SpecialSequencer);
        yield break;
    }

    [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.SelectSlotForCard))]
    [HarmonyPostfix]
    private static IEnumerator FixSelectSlotSpamInAct2(IEnumerator enumerator, PlayerHand __instance, PlayableCard card)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            yield return enumerator;
            yield break;
        }

        // Removes unnecessary calls to unused classes
        __instance.CardsInHand.ForEach(delegate (PlayableCard x)
        {
            x.SetEnabled(enabled: false);
        });
        yield return new WaitWhile(() => __instance.ChoosingSlot);
        __instance.OnSelectSlotStartedForCard(card);

        Singleton<BoardManager>.Instance.CancelledSacrifice = false;
        __instance.choosingSlotCard = card;
        if (card != null && card.Anim != null)
            card.Anim.SetSelectedToPlay(selected: true);

        Singleton<BoardManager>.Instance.ShowCardNearBoard(card, showNearBoard: true);
        if (Singleton<TurnManager>.Instance.SpecialSequencer != null)
            yield return Singleton<TurnManager>.Instance.SpecialSequencer.CardSelectedFromHand(card);

        bool cardWasPlayed = false;
        bool requiresSacrifices = card.Info.BloodCost > 0;
        if (requiresSacrifices)
        {
            List<CardSlot> validSlots = Singleton<BoardManager>.Instance.PlayerSlotsCopy.FindAll((CardSlot x) => x.Card != null);
            yield return Singleton<BoardManager>.Instance.ChooseSacrificesForCard(validSlots, card);
        }
        if (!Singleton<BoardManager>.Instance.CancelledSacrifice)
        {
            List<CardSlot> validSlots2 = Singleton<BoardManager>.Instance.PlayerSlotsCopy.FindAll((CardSlot x) => x.Card == null);
            yield return Singleton<BoardManager>.Instance.ChooseSlot(validSlots2, !requiresSacrifices);
            CardSlot lastSelectedSlot = Singleton<BoardManager>.Instance.LastSelectedSlot;
            if (lastSelectedSlot != null)
            {
                cardWasPlayed = true;
                card.Anim.SetSelectedToPlay(selected: false);
                yield return __instance.PlayCardOnSlot(card, lastSelectedSlot);
                if (card.Info.BonesCost > 0)
                    yield return Singleton<ResourcesManager>.Instance.SpendBones(card.Info.BonesCost);

                if (card.EnergyCost > 0)
                    yield return Singleton<ResourcesManager>.Instance.SpendEnergy(card.EnergyCost);
            }
        }
        if (!cardWasPlayed)
            Singleton<BoardManager>.Instance.ShowCardNearBoard(card, showNearBoard: false);

        __instance.choosingSlotCard = null;
        if (card != null && card.Anim != null)
            card.Anim.SetSelectedToPlay(selected: false);

        __instance.CardsInHand.ForEach(delegate (PlayableCard x)
        {
            x.SetEnabled(enabled: true);
        });
        yield break;
    }

    [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
    [HarmonyPostfix]
    private static IEnumerator FixSequenceSpamInAct2(IEnumerator enumerator, LifeManager __instance, int damage, int numWeights, bool toPlayer, float waitAfter = 0.125f, GameObject alternateWeightPrefab = null, float waitBeforeCalcDamage = 0f, bool changeView = true)
    {
        if (!SaveManager.SaveFile.IsPart2)
        {
            yield return enumerator;
            yield break;
        }

        // removes unnecessary calls to OpponentAnimationController
        if (__instance.scales != null)
        {
            if (changeView)
            {
                Singleton<ViewManager>.Instance.SwitchToView(__instance.scalesView);
                yield return new WaitForSeconds(0.1f);
            }
            yield return __instance.scales.AddDamage(damage, numWeights, toPlayer, alternateWeightPrefab);
            if (waitBeforeCalcDamage > 0f)
                yield return new WaitForSeconds(waitBeforeCalcDamage);

            if (toPlayer)
                __instance.PlayerDamage += damage;
            else
                __instance.OpponentDamage += damage;

            yield return new WaitForSeconds(waitAfter);
        }
        yield break;
    }
}
