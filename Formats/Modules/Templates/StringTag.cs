using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface StringTag_E
{
    public string FormatName { get; }
    public List<Alert> Warnings { get; }

    public Language_O Language_Field => null;
    public RadioButtonAlert Language_DependencyError => null;

    public ErrorResolver<BigInteger[]> ProcessString(string tagName, string str,
        OneOf<BAMStringField, IField<string>> field) => ProcessString(tagName, x => str, field);

    public ErrorResolver<BigInteger[]> ProcessString(string tagName, Func<string, string> valueFromLang,
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
            BAMStringField encField = field.AsT0; //errors only occur with encoded strings
            var alertChoices = Language_DependencyError.Choices;

            string[] langs = LANGUAGE_DEX.AllExistsIn(FormatName);
            BigInteger[][] choices = new BigInteger[langs.Length + 1][];
            for (int i = 0; i < langs.Length; i++)
            {
                (choices[i], _, _) = DexUtil.CharEncoding.Encode(valueFromLang(langs[i]), encField.Length, FormatName, langs[i]);
                string decodedStr = DexUtil.CharEncoding.Decode(choices[i], FormatName, langs[i]).decodedStr;
                alertChoices[i].Message = alertChoices[i].Message.AddNewLine($"{tagName}: {decodedStr}");
            }
            alertChoices[langs.Length].Message = alertChoices[langs.Length].Message.AddNewLine($"{tagName}: "); // "None" lang option
            return new(Language_DependencyError, encField, choices);
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
        return msg is not "" ? new("Nickname", msg) : throw InvalidAlertType();
    }
}