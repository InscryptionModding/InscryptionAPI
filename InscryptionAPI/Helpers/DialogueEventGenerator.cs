using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Helpers
{
    public static class DialogueEventGenerator
    {
        public static DialogueEvent GenerateEvent(string name, List<CustomLine> mainLines, List<List<CustomLine>> repeatLines, DialogueEvent.MaxRepeatsBehaviour afterMaxRepeats)
        {
            DialogueEvent ev = new();
            List<DialogueEvent.Speaker> speakers = new() { DialogueEvent.Speaker.Single };
            ev.id = name;
            ev.mainLines = new DialogueEvent.LineSet(mainLines.ConvertAll((x) => x.ToLine(speakers)));
            ev.repeatLines = repeatLines.ConvertAll((x) => new DialogueEvent.LineSet(x.ConvertAll((x2) => x2.ToLine(speakers))));
            ev.maxRepeatsBehaviour = afterMaxRepeats;
            ev.speakers = new List<DialogueEvent.Speaker>(speakers);
            DialogueDataUtil.Data?.events?.Add(ev);
            return ev;
        }
    }
}
