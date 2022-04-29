using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System;
using System.Linq;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Language_O : IndexTag_O
{
    public OneOf<IField<BigInteger>, IField<string>> Language { get; }

    public bool IsValid(string lang) => IsValid(LANGUAGE_DEX, lang);
    public bool IsValid() => IsValid(AsString);

    public string AsString
    {
        get => AsStringGet(LANGUAGE_DEX, Language);
        set => AsStringSet(LANGUAGE_DEX, Language, value);
    }
}

public interface Language_E : Language_P, IndexTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportLanguage() => ExportLanguageBase();

    public void ExportLanguageBase()
    {
        string lang = pku.Game_Info.Language.Value;
        bool langDep = DexUtil.CharEncoding.IsLangDependent(FormatName);
        if (langDep && !Language_Field.IsValid(lang)) //invalid lang w/ dependency
        {
            string[] langs = InitLangDepError(lang is null ? AlertType.UNSPECIFIED : AlertType.INVALID);
            BigInteger[] langsEnc = new BigInteger[langs.Length];
            for (int i = 0; i < langs.Length; i++)
                langsEnc[i] = LANGUAGE_DEX.GetIndexedValue<int?>(FormatName, langs[i], "Indices") ?? 0;

            //Assume all lang dep formats are encoded.
            Language_Resolver = new(Language_DependencyError, Language_Field.Language.AsT0, langsEnc);
        }
        else
        {
            AlertType at = ExportIndexTag(pku.Game_Info.Language, "None", Language_Field.IsValid, x => Language_Field.AsString = x);
            if (at is not AlertType.UNSPECIFIED) //ignore unspecified
                Warnings.Add(GetIndexAlert("Language", at, pku.Game_Info.Language.Value, "None"));
        }
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger> Language_Resolver { get => null; set { } }
}

public interface Language_I : Language_P, IndexTag_I
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportLanguage() => ImportLanguageBase();
    
    public void ImportLanguageBase()
    {
        bool langDep = DexUtil.CharEncoding.IsLangDependent(FormatName);
        if (langDep && !Language_Field.IsValid(Language_Field.AsString)) //invalid lang w/ dependency
        {
            string[] langs = InitLangDepError(AlertType.INVALID);
            Language_Resolver = new(Language_DependencyError, pku.Game_Info.Language, langs);
        }
        else
            ImportIndexTag("Language", pku.Game_Info.Language, Language_Field.IsValid(),
                Language_Field.AsString, Language_Field.Language.AsT0);
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<string> Language_Resolver { get => null; set { } }
}

public interface Language_P : Tag
{
    public Language_O Language_Field { get; }
    public ChoiceAlert Language_DependencyError { get => null; set { } }

    protected string[] InitLangDepError(AlertType at)
    {
        string[] langs = LANGUAGE_DEX.AllExistsIn(FormatName).Append(null).ToArray();

        //get default choice
        int defChoice = Array.IndexOf(langs, TagUtil.DEFAULT_SEMANTIC_LANGUAGE);
        if (defChoice == -1)
            defChoice = 0;

        ChoiceAlert alert = GetLanguageDependencyAlert(at, langs, defChoice);
        Language_DependencyError = alert;
        Errors.Add(alert);

        return langs;
    }

    public ChoiceAlert GetLanguageDependencyAlert(AlertType at, string[] langs, int defChoice)
        => GetLanguageDependencyAlertBase(at, langs, defChoice);

    public ChoiceAlert GetLanguageDependencyAlertBase(AlertType at, string[] langs, int defChoice)
    {
        ChoiceAlert.SingleChoice[] choices = new ChoiceAlert.SingleChoice[langs.Length];
        for (int i = 0; i < langs.Length; i++)
            choices[i] = new ChoiceAlert.SingleChoice(langs[i] ?? "None", "");

        string msg = "Some text values in this format require a language, but this Pokémon's language is ";
        if (at.HasFlag(AlertType.INVALID))
            msg += "invalid in this format";
        else if (at.HasFlag(AlertType.UNSPECIFIED))
            msg += "unspecified";
        else
            throw InvalidAlertType(at);
        msg += ". Please choose a language:";

        return new("Language Encoding", msg, choices, false, defChoice);
    }
}