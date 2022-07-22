using OneOf;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Fields.BAMFields;
using pkuManager.WinForms.Formats.Modules.MetaTags;
using pkuManager.WinForms.Formats.Modules.Templates;
using pkuManager.WinForms.Utilities;
using System;
using System.Linq;
using System.Numerics;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Language_O
{
    public OneOf<IIntField, IField<string>> Language { get; }
}

public interface Language_E : Language_P
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportLanguage() => ExportLanguageBase();

    public void ExportLanguageBase()
    {
        var languageObj = (Data as Language_O).Language;
        bool langDep = DDM.FormatDex.IsLangDependent(FormatName);
        AlertType at = IndexTagUtil.ExportIndexTag(pku.Game_Info.Language, languageObj, "None", LANGUAGE_DEX, FormatName);

        if (langDep && at is not AlertType.NONE) //lang dep & invalid/unspecified
        {
            string[] langs = InitLangDepError(at);
            BigInteger[] langsEnc = new BigInteger[langs.Length];
            for (int i = 0; i < langs.Length; i++)
                langsEnc[i] = LANGUAGE_DEX.GetIndexedValue<int?>(FormatName, langs[i], "Indices") ?? 0;

            //Assume all lang dep formats are encoded.
            Language_Resolver = new(Language_DependencyError, languageObj.AsT0, langsEnc);
        }
        else //lang indepdendent
            Warnings.Add(IndexTagUtil.GetIndexAlert("Language", at, pku.Game_Info.Language.Value, "None", true));
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger> Language_Resolver { get => null; set { } }
}

public interface Language_I : Language_P
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportLanguage() => ImportLanguageBase();
    
    public void ImportLanguageBase()
    {
        Language_O Language_Field = Data as Language_O;
        bool langDep = DDM.FormatDex.IsLangDependent(FormatName);
        AlertType at = IndexTagUtil.ImportIndexTag(pku.Game_Info.Language, Language_Field.Language, LANGUAGE_DEX, FormatName);

        if (at is AlertType.INVALID)
        {
            if (langDep) //invalid lang w/ dependency
            {
                string[] langs = InitLangDepError(AlertType.INVALID);
                Language_Resolver = new(Language_DependencyError, pku.Game_Info.Language, langs);
            }
            else //invalid lang
                ByteOverrideUtil.TryAddByteOverrideCMD("Language", Language_Field.Language.Value as IByteOverridable, pku, FormatName);
        }
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<string> Language_Resolver { get => null; set { } }
}

public interface Language_P : Tag
{
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