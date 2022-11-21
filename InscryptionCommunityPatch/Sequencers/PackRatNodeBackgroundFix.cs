using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Sequencers;

internal class PackRatNodeBackgroundFix
{
    [HarmonyPatch(typeof(GainConsumablesSequencer), nameof(GainConsumablesSequencer.FullConsumablesSequence))]
    private class GainConsumablesPatch
    {
        private static IEnumerator Postfix(IEnumerator enumerator, GainConsumablesSequencer __instance)
        {
            yield return new WaitForSeconds(1f);
            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("GainConsumablesFull", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[1] { RunState.Run.MaxConsumables.ToString() });
            yield return new WaitForSeconds(0.5f);
            __instance.rat.SetActive(value: true);
            yield return new WaitUntil(() => __instance.ratCard.gameObject.activeInHierarchy);
            bool cardGrabbed = false;
            __instance.ratCard.SetInfo(__instance.fullConsumablesReward);
            __instance.ratCard.ClearDelegates();
            SelectableCard selectableCard = __instance.ratCard;
            selectableCard.CursorSelectEnded = (Action<MainInputInteractable>)Delegate.Combine(selectableCard.CursorSelectEnded, (Action<MainInputInteractable>)delegate
            {
                cardGrabbed = true;
            });
            yield return new WaitUntil(() => cardGrabbed);
            Singleton<RuleBookController>.Instance.SetShown(shown: false);
            Singleton<TableRuleBook>.Instance.SetOnBoard(onBoard: false);
            __instance.rat.GetComponentInChildren<Animator>().SetTrigger("exit");
            yield return new WaitForEndOfFrame();
            GameObject cardObj = UnityEngine.Object.Instantiate(__instance.ratCard.gameObject);
            cardObj.SetActive(value: true);
            cardObj.transform.parent = null;
            cardObj.transform.position = __instance.ratCard.transform.position;
            cardObj.transform.rotation = __instance.ratCard.transform.rotation;
            cardObj.transform.localScale = Vector3.one;
            SelectableCard component = SelectableCard.Instantiate(__instance.ratCard);
            component.SetInteractionEnabled(interactionEnabled: false);
            string text = __instance.fullConsumablesReward.description;
            if (ProgressionData.LearnedCard(__instance.fullConsumablesReward))
            {
                text = string.Format(Localization.Translate("A [c:bR]{0}[c:]... Always useful."), __instance.fullConsumablesReward.DisplayedNameLocalized);
            }
            yield return __instance.LearnObjectSequence(cardObj.transform, 1f, new Vector3(20f, 0f, 0f), text);
            Tween.Position(cardObj.transform, cardObj.transform.position + Vector3.up * 2f + Vector3.forward * 0.5f, 0.1f, 0f, null, Tween.LoopType.None, null, delegate
            {
                UnityEngine.Object.Destroy(cardObj);
                UnityEngine.Object.Destroy(component);
            });
            yield return new WaitForSeconds(0.5f);
            __instance.rat.SetActive(value: false);
            RunState.Run.playerDeck.AddCard(__instance.fullConsumablesReward);
            yield break;
        }
    }
}