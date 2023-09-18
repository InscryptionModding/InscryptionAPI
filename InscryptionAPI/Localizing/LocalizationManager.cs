using System.Text;
using HarmonyLib;
using InscryptionAPI.Guid;
using UnityEngine;

namespace InscryptionAPI.Localizing;

public static partial class LocalizationManager
{
    public class CustomLanguage
    {
        public string PluginGUID;
        public string LanguageName;
        public string LanguageCode; // For translating
        public string PathToStringTable;
        public Language Language;
    }
    
    public class CustomTranslation
    {
        public string PluginGUID;
        public Localization.Translation Translation;
    }

    public static List<string> AllLanguageNames = new List<string>();
    public static List<string> AllLanguageButtonText = new List<string>();
    
    public static List<CustomLanguage> AllLanguages = new List<CustomLanguage>();
    public static List<CustomLanguage> NewLanguages = new List<CustomLanguage>();
    public static List<CustomTranslation> CustomTranslations = new List<CustomTranslation>();
    public static Action<Language> OnLanguageLoaded = null;

    private static List<Language> AlreadyLoadedLanguages = new List<Language>();

    static LocalizationManager()
    {
        AllLanguageNames = LocalizedLanguageNames.NAMES.ToList();
        AllLanguageButtonText = LocalizedLanguageNames.SET_LANGUAGE_BUTTON_TEXT.ToList();
        foreach (Language language in Enum.GetValues(typeof(Language)).Cast<Language>().Where((a)=>a < Language.NUM_LANGUAGES))
        {
            CustomLanguage customLanguage = new CustomLanguage()
            {
                PluginGUID = InscryptionAPIPlugin.ModGUID,
                LanguageName = language.ToString(),
                Language = language,
            };
            
            switch (language)
            {
                case Language.English:
                    customLanguage.LanguageCode = "en";
                    break;
                case Language.French:
                    customLanguage.LanguageCode = "fr";
                    break;
                case Language.Italian:
                    customLanguage.LanguageCode = "it";
                    break;
                case Language.German:
                    customLanguage.LanguageCode = "de";
                    break;
                case Language.Spanish:
                    customLanguage.LanguageCode = "es";
                    break;
                case Language.BrazilianPortuguese:
                    customLanguage.LanguageCode = "pt";
                    break;
                case Language.Turkish:
                    customLanguage.LanguageCode = "tr";
                    break;
                case Language.Russian:
                    customLanguage.LanguageCode = "ru";
                    break;
                case Language.Japanese:
                    customLanguage.LanguageCode = "ja";
                    break;
                case Language.Korean:
                    customLanguage.LanguageCode = "ko";
                    break;
                case Language.ChineseSimplified:
                    customLanguage.LanguageCode = "zhcn";
                    break;
                case Language.ChineseTraditional:
                    customLanguage.LanguageCode = "zhtw";
                    break;
                default:
                    customLanguage.LanguageCode = "UNKNOWN";
                    break;
            }
            AllLanguages.Add(customLanguage);
        }
    }

    public static Language NewLanguage(string pluginGUID, string languageName, string code, string resetButtonText, string stringTablePath=null)
    {
        Language language = GuidManager.GetEnumValue<Language>(pluginGUID, languageName);
        CustomLanguage customLanguage = new CustomLanguage()
        {
            PluginGUID = pluginGUID,
            LanguageName = languageName,
            Language = language,
            LanguageCode = code,
            PathToStringTable = stringTablePath,
        };
        AllLanguages.Add(customLanguage);
        NewLanguages.Add(customLanguage);
        AllLanguageNames.Add(languageName);
        AllLanguageButtonText.Add(resetButtonText);

        /*if (!string.IsNullOrEmpty(stringTablePath))
        {
            ImportStringTable(stringTablePath, language);
        }*/
        
        return language;
    }
    
    private static void ImportStringTable(string stringTablePath, Language language)
    {
        string lines = File.ReadAllText(stringTablePath);
        try
        {
            using StringReader aReader = new StringReader(lines);
            CSVParser cSVParser = new CSVParser(aReader, ',');
            List<string> list = new List<string>();
            int num = 0;
            while (cSVParser.NextLine(list))
            {
                if (list.Count < 3)
                {
                    continue;
                }
                string text = list[0];
                bool num2 = text.EndsWith("_F");
                Localization.Translation translation;
                if (num2)
                {
                    translation = Localization.translations[num - 1];
                }
                else if (num < Localization.translations.Count)
                {
                    translation = Localization.translations[num];
                }
                else
                {
                    translation = new Localization.Translation();
                    Localization.translations.Add(translation);
                }
                if (string.IsNullOrEmpty(translation.id))
                {
                    translation.id = text;
                    translation.englishString = list[1];
                    translation.englishStringFormatted = Localization.FormatString(list[1]);
                }
                if (num2)
                {
                    if (!string.IsNullOrEmpty(list[2]) && translation.values[language] != list[2])
                    {
                        translation.femaleGenderValues.Add(language, list[2]);
                    }
                    continue;
                }
                if (text != translation.id)
                {
                    InscryptionAPIPlugin.Logger.LogInfo("Mismatched Translation! Starting at: " + translation.id);
                }
                translation.values.Add(language, list[2]);
                num++;
            }
        }
        catch
        {
            InscryptionAPIPlugin.Logger.LogInfo("No Translation File for " + language);
        }
    }

    public static CustomTranslation Translate(string pluginGUID, string id, string englishString, string translatedString, Language language)
    {
        CustomTranslation customTranslation = Get(englishString, id);

        bool newTranslation = customTranslation == null;
        if (newTranslation)
        {
            customTranslation = new CustomTranslation();
            customTranslation.PluginGUID = pluginGUID;
            customTranslation.Translation = new Localization.Translation();
            customTranslation.Translation.id = id;
            customTranslation.Translation.englishString = id;
            customTranslation.Translation.englishStringFormatted = !string.IsNullOrEmpty(englishString) ? Localization.FormatString(englishString) : null;
            customTranslation.Translation.values = new Dictionary<Language, string>();
            customTranslation.Translation.femaleGenderValues = new Dictionary<Language, string>();
        }

        customTranslation.Translation.values[language] = translatedString;

        if (newTranslation)
        {
            return Add(customTranslation);
        }

        if (AlreadyLoadedLanguages.Count > 0)
        {
            InsertTranslation(customTranslation);
        }
        return customTranslation;
    }

    public static CustomTranslation Add(CustomTranslation translation)
    {
        CustomTranslations.Add(translation);
        if (AlreadyLoadedLanguages.Count > 0)
        {
            InsertTranslation(translation);
        }
        return translation;
    }

    public static CustomTranslation Get(string englishText, string id)
    {
        CustomTranslation translation = null;
        if (!string.IsNullOrEmpty(englishText))
        {
            translation = CustomTranslations.Find((a) => a.Translation.englishString == englishText);
        }

        if (translation == null && !string.IsNullOrEmpty(id))
        {
            translation = CustomTranslations.Find((a) => a.Translation.id == id);
        }

        return translation;
    }

    private static void InsertTranslation(CustomTranslation customTranslation)
    {
        Localization.Translation translation = null;
        if (!string.IsNullOrEmpty(customTranslation.Translation.englishStringFormatted))
        {
            translation = Localization.Translations.Find((a) => a.englishStringFormatted == customTranslation.Translation.englishStringFormatted);
        }
        else
        {
            translation = Localization.Translations.Find((a) => a.id == customTranslation.Translation.id);
        }

        if (translation == null)
        {
            translation = new Localization.Translation();
            translation.id = customTranslation.Translation.id;
            translation.englishString = customTranslation.Translation.englishString;
            translation.englishStringFormatted = customTranslation.Translation.englishStringFormatted;
            translation.values = new Dictionary<Language, string>();
            translation.femaleGenderValues = new Dictionary<Language, string>();
            Localization.Translations.Add(translation);
        }

        // Update translations
        foreach (Language language in AlreadyLoadedLanguages)
        {
            if (customTranslation.Translation.values.TryGetValue(language, out string word))
            {
                translation.values[language] = word;
            }
            if (customTranslation.Translation.femaleGenderValues.TryGetValue(language, out string femaleWord))
            {
                translation.femaleGenderValues[language] = femaleWord;
            }
        }
    }
    
    public static string LanguageToCode(Language language)
    {
        return AllLanguages.Find((a) => a.Language == language).LanguageCode;
    }

    public static Language CodeToLanguage(string code)
    {
        return AllLanguages.Find((a) => a.LanguageCode == code).Language;
    }

    public static void ExportAllToCSV()
    {
        // Exports all localisation to a spreadsheet in the APIs directory
        StringBuilder stringBuilder = new StringBuilder("id");
        for (int i = 0; i < AllLanguages.Count; i++)
        {
            var language = AllLanguages[i];
            stringBuilder.Append("," + language);
        }

        int keys = Localization.Translations.Count;
        for (var i = 0; i < keys; i++)
        {
            Localization.Translation translation = Localization.Translations[i];
            stringBuilder.Append($"\n{translation.id}");

            for (var j = 0; j < AllLanguages.Count; j++)
            {
                Language language = AllLanguages[j].Language;
                stringBuilder.Append(",");

                if (language == Language.English)
                {
                    stringBuilder.Append(translation.englishString);
                }
                else if (translation.values.TryGetValue(language, out string word))
                {
                    stringBuilder.Append(word);
                }
            }
        }

        string path = Path.Combine(InscryptionAPIPlugin.Directory, "localisation_table.csv");
        File.WriteAllText(path, stringBuilder.ToString());
        InscryptionAPIPlugin.Logger.LogInfo($"Exported .csv file to {path}");
    }
    
    [Obsolete("Use Translate() instead")]
    public static CustomTranslation New(string pluginGUID, string id, string englishString, string translatedString, Language language)
    {
        return Translate(pluginGUID, id, englishString, translatedString, language);
    }
}
