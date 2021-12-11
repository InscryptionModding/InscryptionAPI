using System;
using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public class NewTalkingCard
	{
		public static Dictionary<string,Type> talkingCards = new();
		public static List<Type> types = new(); // For easy access by CardTriggerHandler.GetType

		public static void Add<T>(string name, Dictionary<string, DialogueEvent> dialogueEvents)
        {
			NewTalkingCard.talkingCards.Add(name.Replace(" ", "_"), typeof(T));
			NewTalkingCard.types.Add(typeof(T));
			NewDialogue.AddAll(dialogueEvents);
			Plugin.Log.LogInfo($"Loaded talking card {name}!");
		}

		public static void Add<T>(string name) {
			Add<T>(name, new Dictionary<string, DialogueEvent>());
		}
	}
}
