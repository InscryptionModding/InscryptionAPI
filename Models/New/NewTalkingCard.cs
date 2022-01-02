using System;
using System.Collections.Generic;
using DiskCardGame;

namespace APIPlugin
{
	public static class NewTalkingCard
	{
		private const string TalkingCardPortraitPrefab = "Prefabs/Cards/AnimatedPortraits/TalkingCardPortrait";

		private static readonly CharacterFace
			CharacterFaceBase = ResourceBank.Get<CharacterFace>(TalkingCardPortraitPrefab);

		public static readonly Dictionary<string, Type> TalkingCards = new();
		public static readonly List<Type> Types = new(); // For easy access by CardTriggerHandler.GetType

		public static void Add<T>(string name, Dictionary<string, DialogueEvent> dialogueEvents)
		{
			NewTalkingCard.TalkingCards.Add(name.Replace(" ", "_"), typeof(T));
			NewTalkingCard.Types.Add(typeof(T));
			NewDialogue.AddAll(dialogueEvents);
			Plugin.Log.LogInfo($"Added talking card {name}!");
		}

		public static void Add<T>(string name)
		{
			Add<T>(name, new Dictionary<string, DialogueEvent>());
		}
	}
}
