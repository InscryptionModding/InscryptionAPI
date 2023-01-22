using DiskCardGame;
using InscryptionAPI.Dialogue;
using System.Collections;
using UnityEngine;

#nullable enable

namespace InscryptionAPI.TalkingCards.Animation;
internal class DialogueDummy : PaperTalkingCard
{
    public static List<string> AllDialogueAdded = new();

    public const string DUMMY = "___dummy";

    private static readonly DialogueEvent _dummyEvent = DialogueManager.GenerateEvent(
            InscryptionAPIPlugin.ModGUID,
            DUMMY,
            new List<CustomLine>(),
            new List<List<CustomLine>>()
        );

    public string GetEventForCard(string eventName)
    {
        string id = $"{Card.Info.name}_{eventName}";
        return AllDialogueAdded.Contains(id) ? id : DUMMY;
    }

    public override string OnDrawnDialogueId
        => GetEventForCard("OnDrawn");

    public override string OnPlayFromHandDialogueId
        => GetEventForCard("OnPlayFromHand");

    public override string OnAttackedDialogueId
        => GetEventForCard("OnAttacked");

    public override string OnBecomeSelectablePositiveDialogueId
        => GetEventForCard("OnBecomeSelectablePositive");

    public override string OnBecomeSelectableNegativeDialogueId
        => GetEventForCard("OnBecomeSelectableNegative");

    public override string OnSacrificedDialogueId
        => GetEventForCard("OnSacrificed");

    public override string OnSelectedForDeckTrialDialogueId
        => GetEventForCard("OnSelectedForDeckTrial");

    public override string OnSelectedForCardMergeDialogueId
        => GetEventForCard("OnSelectedForCardMerge");

    public override string OnSelectedForCardRemoveDialogueId
        => GetEventForCard("OnSelectedForCardRemove");

    public override string OnDiscoveredInExplorationDialogueId
        => GetEventForCard("OnDiscoveredInExploration");

    public override string OnDrawnFallbackDialogueId
    {
        /* This is a fallback. Thus, I'm making it not required.
         * However, users will still have the choice of using it if they want to. */
        get
        {
            string x = GetEventForCard("OnDrawnFallback");
            return x == DUMMY ? GetEventForCard("OnDrawn") : x;
        }
    }

    public override Dictionary<Opponent.Type, string> OnDrawnSpecialOpponentDialogueIds => new()
    {
        { Opponent.Type.ProspectorBoss,     GetEventForCard("ProspectorBoss")       },
        { Opponent.Type.AnglerBoss,         GetEventForCard("AnglerBoss")           },
        { Opponent.Type.TrapperTraderBoss,  GetEventForCard("TrapperTraderBoss")    },
        { Opponent.Type.LeshyBoss,          GetEventForCard("LeshyBoss")            },
        { Opponent.Type.RoyalBoss,          GetEventForCard("RoyalBoss")            },
        { Opponent.Type.Default,            GetEventForCard("DefaultOpponent")      },
    };

    public override DialogueEvent.Speaker SpeakerType => DialogueEvent.Speaker.Stoat;

    public override IEnumerator OnShownForCardSelect(bool forPositiveEffect)
    {
        yield return new WaitForEndOfFrame();
        yield return base.OnShownForCardSelect(forPositiveEffect);
        yield break;
    }
}
