using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface StringTag_E : Tag
{
    public Language_O Language_Field => null;
    public ChoiceAlert Language_DependencyError => null;

    public ErrorResolver<BigInteger[]> ExportString(string tagName, string str,
        OneOf<BAMStringField, IField<string>> field) => ExportString(tagName, x => str, field);

    public ErrorResolver<BigInteger[]> ExportString(string tagName, Func<string, string> valueFromLang,
        OneOf<BAMStringField, IField<string>> field)
    {
        if (Language_DependencyError is null) //valid lang/not lang dep
        {
            bool langDep = DexUtil.CharEncoding.IsLangDependent(FormatName);
            string lang = langDep ? Language_Field.AsString : null;
            field.Switch(
                x => {
                    int maxLength = x.Length;
                    AlertType at = AlertType.NONE;

                    (BigInteger[] encStr, bool truncated, bool invalid)
                        = DexUtil.CharEncoding.Encode(valueFromLang(lang), maxLength, FormatName, lang);
                    if (truncated)
                        at |= AlertType.TOO_LONG;
                    if (invalid)
                        at |= AlertType.INVALID;
                    x.Value = encStr;
                    Warnings.Add(GetStringAlert(tagName, at, langDep, maxLength));
                },
                x => x.Value = valueFromLang(lang));
        }
        else //invalid dependent lang
        {
            string[] langs = LANGUAGE_DEX.AllExistsIn(FormatName);
            string[] decodedStrs = new string[langs.Length + 1];
            BigInteger[][] encodedStrs = new BigInteger[langs.Length + 1][];
            var alertChoices = Language_DependencyError.Choices;

            for (int i = 0; i < langs.Length; i++)
            {
                (encodedStrs[i], _, _) = DexUtil.CharEncoding.Encode(valueFromLang(langs[i]), field.AsT0.Length, FormatName, langs[i]);
                decodedStrs[i] = DexUtil.CharEncoding.Decode(encodedStrs[i], FormatName, langs[i]).decodedStr;
                alertChoices[i].Message = alertChoices[i].Message.AddNewLine($"{tagName}: {decodedStrs[i]}");
            }

            // "None" lang option
            (encodedStrs[langs.Length], _, _) = DexUtil.CharEncoding.Encode(null, field.AsT0.Length, FormatName);
            decodedStrs[langs.Length] = "";
            alertChoices[langs.Length].Message = alertChoices[langs.Length].Message.AddNewLine($"{tagName}: ");
            
            return new(Language_DependencyError, field.AsT0, encodedStrs);
        }
        return null;
    }

    public Alert GetStringAlert(string tagName, AlertType at, bool langDep, int? maxChars = null)
    {
        if (at == AlertType.NONE)
            return null;

        string msg = "";
        if (at.HasFlag(AlertType.INVALID)) //some characters invalid, removing them
            msg += $"Some of the characters in the {tagName} are invalid in this format{(langDep ? "/language" : "")}, removing them.";
        if (at.HasFlag(AlertType.TOO_LONG)) //too many characters, truncating
        {
            if (msg is not "")
                msg += DataUtil.Newline();
            msg += $"{tagName} can only have {maxChars} characters in this format, truncating it.";
        }
        return msg is not "" ? new(tagName, msg) : throw InvalidAlertType();
    }
}

public interface StringTag_I : ByteOverride_I
{
    public Language_O Language_Field => null;
    public ChoiceAlert Language_DependencyError => null;

    public ErrorResolver<string> ImportString(string tagName, IField<string> pkuField, OneOf<BAMStringField, IField<string>> field)
    {
        if (Language_DependencyError is null) //valid lang/not lang dep
        {
            bool langDep = DexUtil.CharEncoding.IsLangDependent(FormatName);
            string lang = langDep ? Language_Field.AsString : null;
            field.Switch(
                x => {
                    (pkuField.Value, bool invalid) = DexUtil.CharEncoding.Decode(x.Value, FormatName, lang);
                    if (invalid)
                        Warnings.Add(GetStringAlert(tagName, AlertType.INVALID) //string invalid
                                   + AddByteOverrideCMD(tagName, x.GetOverride())); //adding byte override
                },
                x => pkuField.Value = x.Value);
        }
        else //invalid dependent lang
        {
            string[] langs = LANGUAGE_DEX.AllExistsIn(FormatName);
            string[] decodedStrs = new string[langs.Length + 1];
            var alertChoices = Language_DependencyError.Choices;

            for (int i = 0; i < langs.Length; i++)
            {
                decodedStrs[i] = DexUtil.CharEncoding.Decode(field.AsT0.Value, FormatName, langs[i]).decodedStr;
                alertChoices[i].Message = alertChoices[i].Message.AddNewLine($"{tagName}: {decodedStrs[i]}");
            }
            decodedStrs[langs.Length] = ""; // "None" lang option
            alertChoices[langs.Length].Message = alertChoices[langs.Length].Message.AddNewLine($"{tagName}: ");
            
            return new(Language_DependencyError, pkuField, decodedStrs);
        }
        return null;
    }

    public Alert GetStringAlert(string tagName, AlertType at)
    {
        if (at == AlertType.NONE)
            return null;
        string msg = "";
        if (at.HasFlag(AlertType.INVALID)) //some characters invalid, removing them
            msg += $"Some of the characters in the {tagName} are invalid, removing them.";
        return msg is not "" ? new(tagName, msg) : throw InvalidAlertType();
    }
}