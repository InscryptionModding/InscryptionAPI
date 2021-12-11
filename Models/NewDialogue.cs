using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public class NewDialogue
	{
		public static Dictionary<string, DialogueEvent> dialogueEvents = new();

		public static void AddAll(Dictionary<string, DialogueEvent> dialogueEvents)
        {
			foreach (KeyValuePair<string, DialogueEvent> pair in dialogueEvents)
            {
				NewDialogue.dialogueEvents.Add(pair.Key, pair.Value);
            }
			if (dialogueEvents.Count > 0)
			{
				Plugin.Log.LogInfo($"Loaded {dialogueEvents.Count} custom dialogues!");
			}
		}

		public static void Add(string id, DialogueEvent dialogueEvent) {
			NewDialogue.dialogueEvents.Add(id, dialogueEvent);
			Plugin.Log.LogInfo($"Loaded custom dialogue {id}!");
		}
	}
}
