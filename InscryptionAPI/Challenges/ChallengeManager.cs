using DiskCardGame;
using System;
using System.Linq;
using HarmonyLib;
using Mono.Collections.Generic;
using Sirenix.Serialization.Utilities;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using InscryptionAPI.Guid;

namespace InscryptionAPI.Challenges
{
    [HarmonyPatch]
    public static class ChallengeManager
    {
        public static readonly ReadOnlyCollection<AscensionChallengeInfo> BaseGameChallenges = new(Resources.LoadAll<AscensionChallengeInfo>("Data/"));
        
        private static Dictionary<AscensionChallenge, bool> stackableMap = new();

        private static Dictionary<AscensionChallenge, int> unlockLevelMap = new();

        public static readonly Rect SPRITE_RECT = new Rect(0f, 0f, 49f, 49f);

        public static readonly Vector2 SPRITE_PIVOT = new Vector2(0.5f, 0.5f);

        internal static readonly List<AscensionChallengeInfo> newInfos = new();

        public static event Action<List<AscensionChallengeInfo>> ModifyAscensionChallengeList;

        private static bool _hasLoaded = false;

        public static List<AscensionChallengeInfo> AllInfo
        {
            get
            {
                return BaseGameChallenges.Concat(newInfos).ToList();
            }
        }

        public static bool IsStackable(AscensionChallenge id)
        {
            return stackableMap.ContainsKey(id) ? stackableMap[id] : false;
        }

        public static AscensionChallenge Add(string pluginGuid, AscensionChallengeInfo info, int unlockLevel=0, bool stackable=false)
        {
            info.challengeType = (AscensionChallenge)GuidManager.GetEnumValue<AscensionChallenge>(pluginGuid, info.title);

            newInfos.Add(info);

            ModifyAscensionChallengeList?.Invoke(newInfos);

            stackableMap.Add(info.challengeType, stackable);
            unlockLevelMap.Add(info.challengeType, unlockLevel);

            return info.challengeType;
        }

        public static AscensionChallenge Add(
            string pluginGuid,
            string title,
            string description,
            int pointValue,
            Texture2D iconTexture,
            Texture2D activatedTexture = null,
            int unlockLevel = 0, 
            bool stackable = false
        )
        {
            AscensionChallengeInfo info = ScriptableObject.CreateInstance<AscensionChallengeInfo>();
            info.title = title;
            info.challengeType = AscensionChallenge.None;
            info.description = description;
            info.pointValue = pointValue;
            info.iconSprite = Sprite.Create(iconTexture, SPRITE_RECT, SPRITE_PIVOT);

            Texture2D infoActivationTexture = activatedTexture ?? Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default");
            info.activatedSprite = Sprite.Create(infoActivationTexture, SPRITE_RECT, SPRITE_PIVOT);

            return Add(pluginGuid, info, unlockLevel, stackable);
        }

        [HarmonyPatch(typeof(AscensionChallengesUtil), "GetInfo")]
        [HarmonyPrefix]
        private static void CardLoadPrefix()
        {
            if (!_hasLoaded)
            {
                // Sort the new challenges in order of unlocks
                newInfos.Sort((a, b) => unlockLevelMap[a.challengeType] - unlockLevelMap[b.challengeType]);
                ScriptableObjectLoader<AscensionChallengeInfo>.allData = AllInfo;
            }
        }

        [HarmonyPatch(typeof(AscensionUnlockSchedule), "ChallengeIsUnlockedForLevel")]
        [HarmonyPostfix]
        public static void IsCustomChallengeUnlocked(ref bool __result, int level, AscensionChallenge challenge)
        {
            if (unlockLevelMap.ContainsKey(challenge))
                __result = unlockLevelMap[challenge] <= level;
        }
    }
}