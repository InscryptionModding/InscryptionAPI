using InscryptionAPI.Dialogue;
using InscryptionAPI.Guid;
using System.Reflection;

namespace InscryptionAPI.Helpers;

[Obsolete("Use DialogueManager instead")]
public static class DialogueEventGenerator
{
    [Obsolete("Use DialogueManager.GenerateEvent instead")]
    public static DialogueEvent GenerateEvent(string name, List<CustomLine> mainLines, List<List<CustomLine>> repeatLines = null, DialogueEvent.MaxRepeatsBehaviour afterMaxRepeats =
        DialogueEvent.MaxRepeatsBehaviour.RandomDefinedRepeat, DialogueEvent.Speaker defaultSpeaker = DialogueEvent.Speaker.Single)
    {
        DialogueEvent ev = new();
        List<DialogueEvent.Speaker> speakers = new() { DialogueEvent.Speaker.Single };
        ev.id = name;
        ev.mainLines = new(mainLines != null ? mainLines.ConvertAll((x) => x.ToLine(speakers, defaultSpeaker)) : new());
        ev.repeatLines = repeatLines != null ? repeatLines.ConvertAll((x) => new DialogueEvent.LineSet(x.ConvertAll((x2) => x2.ToLine(speakers, defaultSpeaker)))) : new();
        ev.maxRepeatsBehaviour = afterMaxRepeats;
        ev.speakers = new(speakers);

        string pluginGUID = TypeManager.GetModIdFromCallstack(Assembly.GetCallingAssembly());
        DialogueManager.Add(pluginGUID, ev);
        return ev;
    }
}