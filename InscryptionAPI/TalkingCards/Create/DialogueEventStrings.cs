using InscryptionAPI.Dialogue;

#nullable enable

namespace InscryptionAPI.TalkingCards.Create;

/// <summary>
/// Utility class for creating dialogue events used for creating dialogue events using exclusively strings and arrays.
/// </summary>
/// <remarks>Intended for use with JSONLoader.</remarks>
[Serializable]
public class DialogueEventStrings {
    /// <summary>
    /// Name of this dialogue event.
    /// </summary>
    public string eventName { get; set; }
    /// <summary>
    /// Main lines for the dialogue event.
    /// </summary>
    public string[] mainLines { get; set; }
    /// <summary>
    /// Groups of lines that play when the dialogue event is replayed.
    /// </summary>
    public string[][] repeatLines { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="DialogueEventStrings"/>.
    /// </summary>
    /// <param name="eventName">Name of this dialogue event.</param>
    /// <param name="mainLines">Main lines for the dialogue event.</param>
    /// <param name="repeatLines">Groups of lines that play when the dialogue event is replayed.</param>
    public DialogueEventStrings(string eventName, string[] mainLines, string[][] repeatLines)
    {
        this.eventName = eventName;
        this.mainLines = mainLines;
        this.repeatLines = repeatLines;
    }

    /// <summary>
    /// Calls <see cref="DialogueManager.GenerateEvent(string, string, List{CustomLine}, List{List{CustomLine}}, DialogueEvent.MaxRepeatsBehaviour, DialogueEvent.Speaker)"/> using this object's data.
    /// </summary>
    /// <param name="cardName">The name of the card this dialogue event is tied to. Used for the event's internal name.</param>
    /// <returns>The created <see cref="DialogueEvent"/>.</returns>
    public DialogueEvent CreateEvent(string cardName) => DialogueManager.GenerateEvent(
            InscryptionAPIPlugin.ModGUID,
            $"{cardName}_{eventName}",
            mainLines.Select(x => (CustomLine)x).ToList(),
            repeatLines.Select(x => x.Select(y => (CustomLine)y).ToList()).ToList()
        );
}