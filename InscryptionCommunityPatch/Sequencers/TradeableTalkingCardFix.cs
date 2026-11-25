using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Sequencers;

/// <summary>
/// This patch fixes Tradable Talking Card related Issues.
/// </summary>
[HarmonyPatch]
public class TradeableTalkingCardFix
{
    /// <summary>
    /// Patches the <see cref="TradeCardsForPelts.OnTradableSelected"/> to allow for better handling regarding the Trade of Talking Cards.
    /// </summary>
    /// <param name="__instance">The Class Instance associated with the function e.g. <see cref="TradeCardsForPelts"/>.</param>
    /// <param name="slot">The slot the card was found in.</param>
    /// <param name="card">The card being traded for.</param>
    /// <returns>This only returns our patched variant if the card is a talking card, otherwise it uses vanilla handling.</returns>
    [HarmonyPrefix, HarmonyPatch(typeof(TradeCardsForPelts), nameof(TradeCardsForPelts.OnTradableSelected))]
    private static bool OnTalkingCardSelected(TradeCardsForPelts __instance, HighlightedInteractable slot, PlayableCard card)
    {
        if (card != null && card.GetComponent<TalkingCard>() == null)
            return true;

        // the card we selected is a talking card
        // talking cards change the view when drawn - this changes it back
        // transpiler would probably be better but feh
        if (__instance.PeltInHand())
        {
            AscensionStatsData.TryIncrementStat(AscensionStat.Type.PeltsTraded);
            PlayableCard pelt = Singleton<PlayerHand>.Instance.CardsInHand.Find((PlayableCard x) => x.Info.HasTrait(Trait.Pelt));
            Singleton<PlayerHand>.Instance.RemoveCardFromHand(pelt);
            pelt.SetEnabled(enabled: false);
            pelt.Anim.SetTrigger("fly_off");
            Tween.Position(pelt.transform, pelt.transform.position + new Vector3(0f, 3f, 5f), 0.4f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate
            {
                UnityObject.Destroy(pelt.gameObject);
            });
            card.UnassignFromSlot();
            Tween.Position(card.transform, card.transform.position + new Vector3(0f, 0.25f, -5f), 0.3f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate
            {
                UnityObject.Destroy(card.gameObject);
            });
            __instance.StartCoroutine(TradeForTalkingCard(card));
            slot.ClearDelegates();
            slot.HighlightCursorType = CursorType.Default;
        }

        return false;
    }

    /// <summary>
    /// Handles the View associated with a trade specific to Talking Cards.
    /// </summary>
    /// <param name="card">The card in which is being given to the player.</param>
    /// <returns>The card added to hand, then a switch back to the trading pool.</returns>
    public static IEnumerator TradeForTalkingCard(PlayableCard card)
    {
        yield return Singleton<PlayerHand>.Instance.AddCardToHand(CardSpawner.SpawnPlayableCard(card.Info), new Vector3(0f, 0.5f, -3f), 0f);
        ViewManager.Instance.SwitchToView(View.OpponentQueue);
        ViewManager.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.TraderCardsForPeltsPhase);
    }
}