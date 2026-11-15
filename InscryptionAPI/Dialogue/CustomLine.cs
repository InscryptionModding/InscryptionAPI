using DiskCardGame;

namespace InscryptionAPI.Dialogue;

/// <summary>
/// Struct representing a line of dialogue. Compatible with use in all acts.
/// </summary>
/// <remarks>
/// For simple dialogue, you can substitute strings for this object in methods like <see cref="DialogueManager.GenerateEvent(string, string, List{InscryptionAPI.Dialogue.CustomLine}, List{List{InscryptionAPI.Dialogue.CustomLine}}, DialogueEvent.MaxRepeatsBehaviour, DialogueEvent.Speaker)"/>
/// ; they are automatically converted.
/// </remarks>
public struct CustomLine
{
    /// <summary>
    /// Constructs a new, empty <see cref="CustomLine"/>.
    /// </summary>
    public CustomLine() { }

    /// <summary>
    /// Converts this object to the vanilla <see cref="DialogueEvent.Line"/>.
    /// </summary>
    /// <param name="speakers"></param>
    /// <param name="defaultSpeaker"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Implicitly converts strings to <see cref="CustomLine"/> objects in relevant cases.
    /// </summary>
    /// <param name="str"></param>

    public static implicit operator CustomLine(string str)
    {
        return new CustomLine { text = str };
    }

    /// <summary>
    /// What face P03 should make when playing this dialogue.
    /// </summary>
    public P03AnimationController.Face p03Face = default;
    /// <summary>
    /// What emotion the speaker should use when playing this dialogue.
    /// </summary>
    public Emotion emotion = default;
    /// <summary>
    /// How the dialogue text should be animated.
    /// </summary>
    public TextDisplayer.LetterAnimation letterAnimation = default;
    /// <summary>
    /// What speaker data should be used when playing this dialogue. Controls the voice and font used.
    /// </summary>
    public DialogueEvent.Speaker speaker = default;
    /// <summary>
    /// The actual dialogue text.
    /// </summary>
    public string text = default;
    /// <summary>
    /// String representing special instruction to control other parts of the speaker not otherwise covered.
    /// See <see cref="TextDisplayer.ParseSpecialInstruction(string)"/> for more information.
    /// </summary>
    public string specialInstruction = default;
    /// <summary>
    /// The story condition that must be met if <c>storyConditionMustBemet</c> is true.
    /// </summary>
    public StoryEvent storyCondition = StoryEvent.BasicTutorialCompleted;
    /// <summary>
    /// Whether the specific story condition must be met before this dialogue can be played.
    /// </summary>
    public bool storyConditionMustBeMet = default;
}