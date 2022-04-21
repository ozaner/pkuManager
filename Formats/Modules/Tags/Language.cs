using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
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

public interface Language_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }
    public List<Alert> Errors { get; }

    public Language_O Language_Field { get; }
    public ChoiceAlert Language_DependencyError { get => null; set { } }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportLanguage() => ExportLanguageBase();

    public void ExportLanguageBase()
    {
        string lang = pku.Game_Info.Language.Value;
        bool langDep = DexUtil.CharEncoding.IsLangDependent(FormatName);
        if (langDep && !Language_Field.IsValid(lang)) //invalid lang w/ dependency
        {
            string[] langs = LANGUAGE_DEX.AllExistsIn(FormatName).Append(null).ToArray();
            BigInteger[] choices = new BigInteger[langs.Length];
            for (int i = 0; i < langs.Length; i++)
                choices[i] = LANGUAGE_DEX.GetIndexedValue<int?>(FormatName, langs[i], "Indices") ?? 0;
            
            //get default choice
            int defChoice = Array.IndexOf(langs, TagUtil.DEFAULT_SEMANTIC_LANGUAGE);
            if (defChoice == -1)
                defChoice = 0;

            ChoiceAlert alert = GetLanguageDependencyAlert(lang is null ? AlertType.UNSPECIFIED
                                                                        : AlertType.INVALID, langs, defChoice);
            Language_Resolver = new(alert, Language_Field.Language.AsT0, choices); //Assume all lang dep formats are encoded.
            Language_DependencyError = alert;
            Errors.Add(alert);
        }
        else //independent lang
        {
            ExportIndexTag("Language", pku.Game_Info.Language, "None", true,
                Language_Field.IsValid, x => Language_Field.AsString = x);
        }
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger> Language_Resolver { get => null; set { } }

    public static ChoiceAlert GetLanguageDependencyAlert(AlertType at, string[] langs, int defChoice)
    {
        ChoiceAlert.SingleChoice[] choices = new ChoiceAlert.SingleChoice[langs.Length];
        for (int i = 0; i < langs.Length; i++)
            choices[i] = new ChoiceAlert.SingleChoice(langs[i] ?? "None", "");

        string msg = "Some text values in this format require a language, but this pku's language is ";
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