using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.UI;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public class MissingSquirrelTribeIconFix
{
    [HarmonyPatch(typeof(ResourceBank), nameof(ResourceBank.Awake))]
    private class ResourceBank_Awake
    {
        [HarmonyPostfix]
        private static void Postfix(ResourceBank __instance)
        {
            Texture2D baseTexture = TextureHelper.GetImageAsTexture("tribeicon_squirrel.png", typeof(MissingSquirrelTribeIconFix).Assembly);
            __instance.resources.Add(new ResourceBank.Resource()
            {
                path = "Art/Cards/TribeIcons/tribeicon_squirrel",
                asset = baseTexture.ConvertTexture()
            });
        }
    }
}
