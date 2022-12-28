using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Dialogue;

public static class DialogueManager
{
    public class Dialogue
    {
        public DialogueEvent DialogueEvent;
        public string PluginGUID;
    }
    
    public class DialogueColor
    {
        public Color Color;
        public string ColorCode;
        public string PluginGUID;
    }

    public static List<Dialogue> CustomDialogue = new List<Dialogue>();
    public static List<DialogueColor> CustomDialogueColor = new List<DialogueColor>();
    public static Dictionary<string, Color> ColorLookup = new Dictionary<string, Color>();
    
    public static Dialogue Add(string pluginGUID, DialogueEvent dialogueEvent)
    {
        Dialogue dialogue = new Dialogue()
        {
            PluginGUID = pluginGUID,
            DialogueEvent = dialogueEvent
        };
        CustomDialogue.Add(dialogue);
        return dialogue;
    }
    
    public static DialogueColor AddColor(string pluginGUID, string code, Color color)
    {
        DialogueColor data = new DialogueColor()
        {
            PluginGUID = pluginGUID,
            ColorCode = code,
            Color = color
        };
        ColorLookup[code] = color;
        CustomDialogueColor.Add(data);
        return data;
    }

    #region Pathces
    [HarmonyPatch(typeof(DialogueDataUtil), nameof(DialogueDataUtil.ReadDialogueData), new System.Type[] { })]
    public class DialogueDataUtil_ReadDialogueData
    {
        public static void Postfix()
        {
            List<string> ids = DialogueDataUtil.data.events.Select((a) => a.id).ToList();
            
            foreach (Dialogue dialogue in CustomDialogue)
            {
                if (ids.Contains(dialogue.DialogueEvent.id))
                {
                    InscryptionAPIPlugin.Logger.LogWarning("Attempting to add dialogue with id that already exists: " + dialogue.DialogueEvent.id);
                }
                DialogueDataUtil.data.events.Add(dialogue.DialogueEvent);
            }
            InscryptionAPIPlugin.Logger.LogWarning($"Inserted {CustomDialogue.Count} dialogue!");
        }
    }
    
    [HarmonyPatch(typeof(DialogueParser), nameof(DialogueParser.GetColorFromCode), new System.Type[] {typeof (string), typeof(Color)})]
    public class DialogueParser_GetColorFromCode
    {
        public static bool Prefix(DialogueParser __instance, string code, Color defaultColor, ref Color __result)
        {
            string stringValue = DialogueParser.GetStringValue(code, "c");
            if (ColorLookup.TryGetValue(stringValue, out Color value))
            {
                __result = value;
                return false;
            }

            return true;
        }
    }
    #endregion
}
