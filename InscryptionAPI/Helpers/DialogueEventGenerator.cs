namespace InscryptionAPI.Helpers;

public static class DialogueEventGenerator
{
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
        DialogueDataUtil.Data?.events?.Add(ev);
        return ev;
    }
}