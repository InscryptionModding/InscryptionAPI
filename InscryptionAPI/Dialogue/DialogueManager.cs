using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Pelts;
using System.Collections;
using UnityEngine;
using static DiskCardGame.TextDisplayer;

namespace InscryptionAPI.Dialogue;

/// <summary>
/// Manager class for handling custom dialogue.
/// </summary>
public static class DialogueManager
{
    /// <summary>
    /// Utility class that associates a custom <see cref="DialogueEvent"/> with the GUID of the mod that added it.
    /// </summary>
    public class Dialogue
    {
        /// <summary>
        /// The actual dialogue data.
        /// </summary>
        public DialogueEvent DialogueEvent;
        /// <summary>
        /// GUID of the mod that added this dialogue.
        /// </summary>
        public string PluginGUID;
    }

    /// <summary>
    /// Utility class for custom colour codes. Used in dialogue to colour text.
    /// </summary>
    public class DialogueColor {
        /// <summary>
        /// The custom colour.
        /// </summary>
        public Color Color;
        /// <summary>
        /// The dialogue code used to colour text.
        /// </summary>
        public string ColorCode;
        /// <summary>
        /// GUID of the mod that added this colour.
        /// </summary>
        public string PluginGUID;
    }

    /// <summary>
    /// All custom dialogue added through the API.
    /// </summary>
    public static List<Dialogue> CustomDialogue = new();
    /// <summary>
    /// All custom dialogue colours added through the API.
    /// </summary>
    public static List<DialogueColor> CustomDialogueColor = new();
    /// <summary>
    /// Dictionary that matches custom colour codes to the correct colour.
    /// </summary>
    public static Dictionary<string, Color> ColorLookup = new();

    /// <summary>
    /// Adds a new <see cref="DialogueEvent"/> to the API.
    /// </summary>
    /// <param name="pluginGUID">GUID of the mod adding this event.</param>
    /// <param name="dialogueEvent">The <see cref="DialogueEvent"/> to add.</param>
    /// <returns>A <see cref="Dialogue"/> object containing the event and associated GUID.</returns>
    public static Dialogue Add(string pluginGUID, DialogueEvent dialogueEvent)
    {
        Dialogue dialogue = new()
        {
            PluginGUID = pluginGUID,
            DialogueEvent = dialogueEvent
        };

        CustomDialogue.RemoveAll(d => d.DialogueEvent.id == dialogueEvent.id);
        CustomDialogue.Add(dialogue);

        DialogueDataUtil.Data?.events?.RemoveAll(d => d.id == dialogueEvent.id);
        DialogueDataUtil.Data?.events?.Add(dialogueEvent);

        return dialogue;
    }

    /// <summary>
    /// <list type="table">
    ///     <item><term>B</term><description>blue</description></item>
    ///     <item><term>bB</term><description>bright blue</description></item>
    ///     <item><term>bG</term><description>bright gold</description></item>
    ///     <item><term>blGr</term><description>bright lime green</description></item>
    ///     <item><term>bR</term><description>bright red</description></item>
    ///     <item><term>brnO</term><description>brown orange</description></item>
    ///     <item><term>bSG</term><description>glow seafoam green (default Act 1 emission colour)</description></item>
    ///     <item><term>dB</term><description>dark blue</description></item>
    ///     <item><term>dlGr</term><description>dark lime green</description></item>
    ///     <item><term>dSG</term><description>dark seafoam green</description></item>
    ///     <item><term>G</term><description>gold</description></item>
    ///     <item><term>g1</term><description>bright lime green</description></item>
    ///     <item><term>g2</term><description>gold</description></item>
    ///     <item><term>g3</term><description>blue</description></item>
    ///     <item><term>gray</term><description>40% grey</description></item>
    ///     <item><term>lGr</term><description>lime green</description></item>
    ///     <item><term>O</term><description>orange</description></item>
    /// </list>
    /// </summary>
    private static int iff;

    /// <summary>
    /// Adds a new colour code that can be used in dialogue events to colour text.
    /// <br/><br/>
    /// Colour codes are in the following format:<br/>
    /// <c>[c:{code}]{dialogue}[c:]</c>
    /// <list type="table">
    ///     <listheader>Vanilla colour codes are:</listheader>
    ///     <item><term>B</term><description>blue</description></item>
    ///     <item><term>bB</term><description>bright blue</description></item>
    ///     <item><term>bG</term><description>bright gold</description></item>
    ///     <item><term>blGr</term><description>bright lime green</description></item>
    ///     <item><term>bR</term><description>bright red</description></item>
    ///     <item><term>brnO</term><description>brown orange</description></item>
    ///     <item><term>bSG</term><description>glow seafoam green (default Act 1 emission colour)</description></item>
    ///     <item><term>dB</term><description>dark blue</description></item>
    ///     <item><term>dlGr</term><description>dark lime green</description></item>
    ///     <item><term>dSG</term><description>dark seafoam green</description></item>
    ///     <item><term>G</term><description>gold</description></item>
    ///     <item><term>g1</term><description>bright lime green</description></item>
    ///     <item><term>g2</term><description>gold</description></item>
    ///     <item><term>g3</term><description>blue</description></item>
    ///     <item><term>gray</term><description>40% grey</description></item>
    ///     <item><term>lGr</term><description>lime green</description></item>
    ///     <item><term>O</term><description>orange</description></item>
    /// </list>
    /// </summary>
    /// <param name="pluginGUID">The GUID of the mod adding this colour.</param>
    /// <param name="code">The dialogue code for using this colour in text.</param>
    /// <param name="color">The colour associated with the code.</param>
    /// <returns>The new colour's data object.</returns>
    public static DialogueColor AddColor(string pluginGUID, string code, Color color)
    {
        DialogueColor data = new()
        {
            PluginGUID = pluginGUID,
            ColorCode = code,
            Color = color
        };
        ColorLookup[code] = color;
        CustomDialogueColor.Add(data);
        return data;
    }

    /// <summary>
    /// Creates a custom <see cref="DialogueEvent"/> and registers it with the API so it can be used in-game.
    /// </summary>
    /// <param name="pluginGUID">GUID of the mod adding this event.</param>
    /// <param name="name">Name of this event.</param>
    /// <param name="mainLines">The dialogue to play when this event plays.</param>
    /// <param name="repeatLines">The dialogue to play when entering an already-introduced region.</param>
    /// <param name="afterMaxRepeats">How this event should handle repeat plays after all repeat dialogue has been exhausted.</param>
    /// <param name="defaultSpeaker">The speaker ID for this event. Controls the font and voice used.</param>
    /// <returns>The new <see cref="DialogueEvent"/>.</returns>
    public static DialogueEvent GenerateEvent(string pluginGUID, string name, List<CustomLine> mainLines, List<List<CustomLine>> repeatLines = null, DialogueEvent.MaxRepeatsBehaviour afterMaxRepeats =
        DialogueEvent.MaxRepeatsBehaviour.RandomDefinedRepeat, DialogueEvent.Speaker defaultSpeaker = DialogueEvent.Speaker.Single)
    {
        DialogueEvent ev = new();
        List<DialogueEvent.Speaker> speakers = new() { DialogueEvent.Speaker.Single };
        ev.id = name;
        ev.mainLines = new(mainLines != null ? mainLines.ConvertAll((x) => x.ToLine(speakers, defaultSpeaker)) : new());
        ev.repeatLines = repeatLines != null ? repeatLines.ConvertAll((x) => new DialogueEvent.LineSet(x.ConvertAll((x2) => x2.ToLine(speakers, defaultSpeaker)))) : new();
        ev.maxRepeatsBehaviour = afterMaxRepeats;
        ev.speakers = new(speakers);

        Add(pluginGUID, ev);
        return ev;
    }

    /// <summary>
    /// Creates a new <see cref="DialogueEvent"/> with the correct prefix needed to make it play when trading the given pelt type.
    /// </summary>
    /// <param name="pluginGUID">GUID of the mod adding this event.</param>
    /// <param name="peltData"><see cref="PeltManager.PeltData"/> of the pelt this event plays for.</param>
    /// <param name="lines">The dialogue to play when this event plays.</param>
    /// <param name="repeatLines">The dialogue to play when entering an already-introduced region.</param>
    /// <returns>A <see cref="DialogueEvent"/> for the given pelt's trade dialogue.</returns>
    public static DialogueEvent GenerateTraderPeltsEvent(string pluginGUID, PeltManager.PeltData peltData, List<CustomLine> lines, List<List<CustomLine>> repeatLines = null)
    {
        return GenerateTraderPeltsEvent(pluginGUID, peltData.peltTierName ?? PeltManager.GetTierNameFromData(peltData), lines, repeatLines);
    }

    /// <summary>
    /// Creates a new <see cref="DialogueEvent"/> with the correct prefix needed to make it play when trading the given pelt type.
    /// </summary>
    /// <param name="pluginGUID">GUID of the mod adding this event.</param>
    /// <param name="peltTierName">Name of the pelt this event plays for.</param>
    /// <param name="lines">The dialogue to play when this event plays.</param>
    /// <param name="repeatLines">The dialogue to play when entering an already-introduced region.</param>
    /// <returns>A <see cref="DialogueEvent"/> for the given pelt's trade dialogue.</returns>
    public static DialogueEvent GenerateTraderPeltsEvent(string pluginGUID, string peltTierName, List<CustomLine> lines, List<List<CustomLine>> repeatLines = null)
    {
        return GenerateEvent(pluginGUID, "TraderPelts" + peltTierName, lines, repeatLines);
    }

    /// <summary>
    /// Creates a new <see cref="DialogueEvent"/> with the correct prefix needed to make it play when entering a new region.
    /// </summary>
    /// <param name="pluginGUID">GUID of the mod adding this event.</param>
    /// <param name="regionData"><see cref="RegionData"/> of the region this event is for.</param>
    /// <param name="lines">The dialogue to play when this event plays.</param>
    /// <param name="repeatLines">The dialogue to play when entering an already-introduced region.</param>
    /// <returns>A <see cref="DialogueEvent"/> for the given region introduction.</returns>
    public static DialogueEvent GenerateRegionIntroductionEvent(string pluginGUID, RegionData regionData, List<CustomLine> lines, List<List<CustomLine>> repeatLines = null)
    {
        return GenerateRegionIntroductionEvent(pluginGUID, regionData.name, lines, repeatLines);
    }

    /// <summary>
    /// Creates a new <see cref="DialogueEvent"/> with the correct prefix needed to make it play when entering a new region.
    /// </summary>
    /// <param name="pluginGUID">GUID of the mod adding this event.</param>
    /// <param name="regionName">Name of the region this event is introducing.</param>
    /// <param name="lines">The dialogue to play when this event plays.</param>
    /// <param name="repeatLines">The dialogue to play when entering an already-introduced region.</param>
    /// <returns>A <see cref="DialogueEvent"/> for the given region introduction.</returns>
    public static DialogueEvent GenerateRegionIntroductionEvent(string pluginGUID, string regionName, List<CustomLine> lines, List<List<CustomLine>> repeatLines = null)
    {
        return GenerateEvent(pluginGUID, "Region" + regionName, lines, repeatLines);
    }

    /// <summary>
    /// A version of PlayDialogueEvent that can be used in the 3D Acts as well as Act 2.
    /// Effectively just adds a check for Act 2 and then runs the correct PlayDialogueEvent method.
    /// </summary>
    public static IEnumerator PlayDialogueEventSafe(string eventId,
        MessageAdvanceMode advanceMode = MessageAdvanceMode.Auto,
        EventIntersectMode intersectMode = EventIntersectMode.Wait,
        DialogueSpeaker speaker = null, TextBox.Style style = TextBox.Style.Neutral,
        TextBox.ScreenPosition screenPosition = TextBox.ScreenPosition.OppositeOfPlayer,
        string[] variableStrings = null,
        Action<DialogueEvent.Line> newLineCallback = null,
        bool adjustAudioVolume = true)
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            yield return DialogueHandler.Instance.PlayDialogueEvent(eventId, style, speaker, variableStrings, screenPosition, adjustAudioVolume);
        }
        else
        {
            yield return TextDisplayer.Instance.PlayDialogueEvent(eventId, advanceMode, intersectMode, variableStrings, newLineCallback);
        }
    }

    /// <summary>
    /// A quick method to convert a card's CardTemple into the respective TextBox.Style.
    /// </summary>
    /// <param name="temple">The CardTemple we want to use.</param>
    /// <returns>The corresponding TextBox.Style.</returns>
    public static TextBox.Style GetStyleFromTemple(CardTemple temple) => (TextBox.Style)(int)temple;

    /// <summary>
    /// A method to grab the correct TextBox.Style based off the player's chosen ambition.
    /// </summary>
    /// <returns>The corresponding TextBox.Style.</returns>
    public static TextBox.Style GetStyleFromAmbition()
    {
        if (StoryEventsData.EventCompleted(StoryEvent.GBCUndeadAmbition))
        {
            return TextBox.Style.Undead;
        }
        else if (StoryEventsData.EventCompleted(StoryEvent.GBCNatureAmbition))
        {
            return TextBox.Style.Nature;
        }
        else if (StoryEventsData.EventCompleted(StoryEvent.GBCTechAmbition))
        {
            return TextBox.Style.Tech;
        }
        else
        {
            return TextBox.Style.Magic;
        }
    }

    #region Patches
    [HarmonyPatch(typeof(DialogueDataUtil), nameof(DialogueDataUtil.ReadDialogueData), new System.Type[] { })]
    private class DialogueDataUtil_ReadDialogueData
    {
        private static void Postfix()
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
        }
    }

    [HarmonyPatch(typeof(DialogueParser), nameof(DialogueParser.GetColorFromCode), new System.Type[] { typeof(string), typeof(Color) })]
    private class DialogueParser_GetColorFromCode
    {
        private static bool Prefix(DialogueParser __instance, string code, Color defaultColor, ref Color __result)
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
