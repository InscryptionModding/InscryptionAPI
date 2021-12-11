using HarmonyLib;
using APIPlugin;

namespace API.Patches
{
    [HarmonyPatch(typeof(DialogueDataUtil.DialogueData), "GetEvent", typeof(string))]
    public class DialogueUtilPatch
    {
        public static bool Prefix(ref DialogueEvent __result, string id)
        {
            DialogueEvent dialogueEvent;
            if (id != null && NewDialogue.dialogueEvents.TryGetValue(id, out dialogueEvent))
            {
                __result = dialogueEvent;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

}
