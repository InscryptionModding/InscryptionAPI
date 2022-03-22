using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;

namespace InscryptionAPI.Nodes
{
    public class MissingEventSequencer : CustomSpecialNodeSequencer
    {
        public override IEnumerator DoCustomSequence()
        {
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Huh. I don't remember leaving that \"MISSING\" here.", -2.5f, 0.5f, Emotion.Curious, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("It seems like whatever was here before... Left.", -2.5f, 0.5f, Emotion.Curious, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("And it doesn't look like it's going to come back anytime soon.", -2.5f, 0.5f, Emotion.Curious, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Oh well.", -2.5f, 0.5f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
            yield break;
        }
    }
}
