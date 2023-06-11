using DiskCardGame;

namespace InscryptionAPI.Helpers;


[Obsolete("Use DialogueManager.GenerateEvent instead")]
public struct CustomLine
{
    public CustomLine() { }

    public DialogueEvent.Line ToLine(List<DialogueEvent.Speaker> speakers, DialogueEvent.Speaker defaultSpeaker = DialogueEvent.Speaker.Single)
    {
        if (speaker == DialogueEvent.Speaker.Single)
        {
            speaker = defaultSpeaker;
        }
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

    public P03AnimationController.Face p03Face = default;
    public Emotion emotion = default;
    public TextDisplayer.LetterAnimation letterAnimation = default;
    public DialogueEvent.Speaker speaker = default;
    public string text = default;
    public string specialInstruction = default;
    public StoryEvent storyCondition = StoryEvent.BasicTutorialCompleted;
    public bool storyConditionMustBeMet = default;
}
