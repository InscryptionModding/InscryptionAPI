## Localisation
---
While Inscryption already provides translations for a number of languages, these only apply to the base game's text, meaning any modded content is stuck in the language it was written in.

To alleviate this problem, the API provides support for adding new translations and even new languages.

### Adding new Translations
If you want to add your own translations to Inscryption you can use the API's localisation system.
```csharp
LocalizationManager.Translate("MyModGUID", null, "Hello", "안녕하세요", Language.Korean);
```

### Adding new Languages
If you want to add your own translations to Inscryption you can use the API's localisation system.
```csharp
LocalizationManager.NewLanguage("MyModGUID", "Polish", "PL", "Reset With Polish", pathToCSV);
```

The csv for your language should be in the following format so it can be imported when needed:
```
Column1,Column10,PL
TALKING_STOAT_DIALOGUE_STOATSACRIFICE_REPEAT_#2_852_M,Again...,Ponownie...
_OPPONENTSKIPTURN_REPEAT_#1_558_M,Pass.,Przechodzić.
```

## Default languages
---
The default supported languages are listed in the table below.

| Suffix | Language    |
|--------|-------------|
| fr     | French      |
| it     | Italian     |
| de     | German      |
| es     | Spanish     |
| pt     | Portuguese  |
| tr     | Turkish     |
| ru     | Russian     |
| ja     | Japanese    |
| ko     | Korean      |
| zhcn   | Chinese (Simplified) |
| zhtw   | Chinese (Traditional)|