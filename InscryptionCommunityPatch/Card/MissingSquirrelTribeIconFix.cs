using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

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

    [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.UpdateTribeIcon))]
    private class CardDisplayer3D_UpdateTribeIcon
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // === We want to turn this

            /* for (int i = 0; i < 7; i++)
			    {
			        if (info.IsOfTribe((Tribe)i))
                    {
                        ...
                    }                
                }
            */

            // === Into this

            /* for (int i = 0; i < 7; i++)
			    {
			        if (!ShowTribeOnCard(info, (Tribe)i)
                    {
                        ...
                    }                
                }
            */

            MethodInfo ShowTribeOnCardInfo = SymbolExtensions.GetMethodInfo(() => ShowTribeOnCard(null, Tribe.Squirrel));
            MethodInfo IsOfTribeInfo = AccessTools.Method(typeof(CardInfo), "IsOfTribe", new Type[] { typeof(Tribe) });

            // ===

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == IsOfTribeInfo)
                {
                    codes[i].operand = ShowTribeOnCardInfo;
                    // Just change which method this call points to.
                    // The first parameter is the instance already!
                    break;
                }
            }

            return codes;
        }

        private static bool ShowTribeOnCard(CardInfo info, Tribe tribe)
        {
            if (!info.IsOfTribe(tribe))
            {
                return false;
            }
            if (tribe == Tribe.Squirrel)
            {
                return PatchPlugin.configShowSquirrelTribeOnCards.Value;
            }

            return true;
        }
    }
}
