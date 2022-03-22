using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;

namespace InscryptionAPI.Helpers
{
    public class CustomLine
    {
		public DialogueEvent.Line ToLine(List<DialogueEvent.Speaker> speakers)
        {
            if (!speakers.Contains(speaker))
            {
				speakers.Add(speaker);
            }
			return new DialogueEvent.Line
			{
				p03Face = p03Face,
				emotion = emotion,
				letterAnimation = letterAnimation,
				speakerIndex = speakers.IndexOf(speaker),
				text = text ?? "",
				specialInstruction = specialInstruction ?? "",
				storyCondition = storyCondition,
				storyConditionMustBeMet = storyConditionMustBeMet
			};
        }

		public static implicit operator CustomLine(string str)
        {
			return new CustomLine { text = str };
        }

		public static implicit operator CustomLine((string, TextDisplayer.LetterAnimation, Emotion) param)
		{
			return new CustomLine { text = param.Item1, emotion = param.Item3, letterAnimation = param.Item2 };
		}

		public static implicit operator CustomLine((string, TextDisplayer.LetterAnimation) param)
		{
			return new CustomLine { text = param.Item1, letterAnimation = param.Item2 };
		}

		public static implicit operator CustomLine((string, TextDisplayer.LetterAnimation, DialogueEvent.Speaker) param)
		{
			return new CustomLine { text = param.Item1, speaker = param.Item3, letterAnimation = param.Item2 };
		}

		public static implicit operator CustomLine((string, TextDisplayer.LetterAnimation, DialogueEvent.Speaker, Emotion) param)
		{
			return new CustomLine { text = param.Item1, speaker = param.Item3, letterAnimation = param.Item2, emotion = param.Item4 };
		}

		public static implicit operator CustomLine((string, Emotion) param)
		{
			return new CustomLine { text = param.Item1, emotion = param.Item2 };
		}

		public static implicit operator CustomLine((string, Emotion, DialogueEvent.Speaker) param)
		{
			return new CustomLine { text = param.Item1, speaker = param.Item3, emotion = param.Item2 };
		}

		public static implicit operator CustomLine((string, DialogueEvent.Speaker) param)
        {
			return new CustomLine { text = param.Item1, speaker = param.Item2 };
        }

		public P03AnimationController.Face p03Face;
		public Emotion emotion;
		public TextDisplayer.LetterAnimation letterAnimation;
		public DialogueEvent.Speaker speaker;
		public string text;
		public string specialInstruction;
		public StoryEvent storyCondition = StoryEvent.BasicTutorialCompleted;
		public bool storyConditionMustBeMet;
	}
}
