using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using HarmonyLib;
using InscryptionAPI.TalkingCards.Create;
using InscryptionAPI.TalkingCards.Animation;

namespace InscryptionAPI.TalkingCards.JSON;

internal static class LoadJSON
{
    private static List<string> GetTalkingJSON()
        => Directory.GetFiles(Paths.PluginPath, "*_talk.json", SearchOption.AllDirectories).ToList();

    public static void LoadJSONCards()
        => GetTalkingJSON().ForEach(x => LoadTalkJSON(x));

    private static void LoadTalkJSON(string file)
    {
        Plugin.LogInfo($"Loading file: {Path.GetFileName(file)}");

        try
        {
            TalkingJSONData? talk = JsonConvert.DeserializeObject<TalkingJSONData>(File.ReadAllText(file));
            if (talk == null) return;
            //FileLog.Log($"Loading card: {talk.cardName}");
            TalkingCardCreator.New(talk.GetFaceData(), GeneratePortrait.DialogueDummy);
            var dialogueEvents = talk.MakeDialogueEvents();
            dialogueEvents.ForEach(x => TalkingCardCreator.AddToDialogueCache(x?.id));
        }
        catch (Exception)
        {
            Plugin.LogError($"Error loading JSON data from file {Path.GetFileName(file)}!");
            // Plugin.LogError(ex.ToString());
            // throw;
        }
    }
}
