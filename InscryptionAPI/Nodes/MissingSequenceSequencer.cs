using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;

namespace InscryptionAPI.Nodes
{
    public class MissingSequenceSequencer : CustomSpecialNodeSequencer
    {
        public override IEnumerator DoCustomSequence()
        {
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Hmm. The event picture is here, but it doesn't seem to mean anything?", -2.5f, 0.5f, Emotion.Curious, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Perhaps I forgot to write what this event does.", -2.5f, 0.5f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Oh well.", -2.5f, 0.5f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
            yield break;
        }
    }
}
