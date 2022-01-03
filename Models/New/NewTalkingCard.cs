using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace APIPlugin
{
	public static class NewTalkingCard
	{
		private const string TalkingCardPortraitPrefab = "Prefabs/Cards/AnimatedPortraits/TalkingCardPortrait";

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


		#region Static_CreateTalkingCardAnimation

		public static UnityEngine.GameObject CreateTalkingCardAnimation(
			List<CharacterFace.EmotionSprites> emotionSpritesList,
			float blinkRate = 3f
		)
		{
			CharacterFace characterFace = ResourceBank.Get<CharacterFace>(TalkingCardPortraitPrefab);
			characterFace.eyes.blinkRate = blinkRate;
			characterFace.emotionSprites = emotionSpritesList;
			return characterFace.gameObject;
		}

		public static UnityEngine.GameObject CreateTalkingCardAnimation(
			CharacterFace.EmotionSprites emotionSprites,
			float blinkRate = 3f
		)
		{
			return CreateTalkingCardAnimation(
				new List<CharacterFace.EmotionSprites>() { emotionSprites },
				blinkRate
			);
		}

		public static UnityEngine.GameObject CreateTalkingCardAnimation(
			Emotion emotion = Emotion.Neutral,
			string facePng = null,
			string eyesOpenPng = null, string eyesClosedPng = null,
			string eyesOpenEmissionPng = null, string eyesClosedEmissionPng = null,
			string mouthOpenPng = null, string mouthClosedPng = null,
			float blinkRate = 3f
		)
		{
			return CreateTalkingCardAnimation(
				CreateSpritesForEmotion(
					emotion,
					facePng, eyesOpenPng, eyesClosedPng,
					eyesOpenEmissionPng, eyesClosedEmissionPng,
					mouthOpenPng, mouthClosedPng
				),
				blinkRate
			);
		}

		#endregion

		public static CharacterFace.EmotionSprites CreateSpritesForEmotion(
			Emotion emotion = Emotion.Neutral,
			string facePng = null,
			string eyesOpenPng = null, string eyesClosedPng = null,
			string eyesOpenEmissionPng = null, string eyesClosedEmissionPng = null,
			string mouthOpenPng = null, string mouthClosedPng = null
		)
		{
			return new CharacterFace.EmotionSprites()
			{
				emotion = emotion,
				face = ImageUtils.CreateSpriteFromPng(facePng, new Vector2(0.5f, 0f)),
				eyesOpen = ImageUtils.CreateSpriteFromPng(eyesOpenPng),
				eyesClosed = ImageUtils.CreateSpriteFromPng(eyesClosedPng),
				eyesOpenEmission = ImageUtils.CreateSpriteFromPng(eyesOpenEmissionPng),
				eyesClosedEmission = ImageUtils.CreateSpriteFromPng(eyesClosedEmissionPng),
				mouthOpen = ImageUtils.CreateSpriteFromPng(mouthOpenPng),
				mouthClosed = ImageUtils.CreateSpriteFromPng(mouthClosedPng),
			};
		}
	}
}