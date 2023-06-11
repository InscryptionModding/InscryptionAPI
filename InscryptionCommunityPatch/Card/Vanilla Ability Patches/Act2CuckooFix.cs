using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
internal class Act2CuckooFix
{
    private static MethodBase TargetMethod()
    {
        MethodBase baseMethod = AccessTools.Method(typeof(CreateEgg), nameof(CreateEgg.OnResolveOnBoard));
        return AccessTools.EnumeratorMoveNext(baseMethod);
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> FixDialogueSoftlock(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "DiskCardGame.TextDisplayer get_Instance()")
            {
                codes.RemoveRange(i, 7);
                MethodInfo customMethod = AccessTools.Method(typeof(Act2CuckooFix), nameof(Act2CuckooFix.PixelDialogue),
                    new Type[] { typeof(CreateEgg) });

                codes.Insert(i, new(OpCodes.Ldarg_0));
                codes.Insert(i + 1, new(OpCodes.Callvirt, customMethod));
                break;
            }
        }

        return codes;
    }
    private static IEnumerator PixelDialogue(CreateEgg instance)
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            DialogueSpeaker speaker = Singleton<InBattleDialogueSpeakers>.Instance.GetSpeaker(instance.Card.Info.temple switch
            {
                CardTemple.Undead => DialogueSpeaker.Character.Grimora,
                CardTemple.Wizard => DialogueSpeaker.Character.Magnificus,
                CardTemple.Tech => DialogueSpeaker.Character.P03,
                _ => DialogueSpeaker.Character.Leshy
            });
            yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent("CuckooSpawnRavenEgg", (TextBox.Style)instance.Card.Info.temple, speaker);
        }
        else
        {
            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("CuckooSpawnRavenEgg", TextDisplayer.MessageAdvanceMode.Input);
        }
    }
}