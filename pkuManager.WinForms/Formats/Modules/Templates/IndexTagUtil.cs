using Newtonsoft.Json.Linq;
using OneOf;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Utilities;
using System;
using static pkuManager.WinForms.Alerts.Alert;

namespace pkuManager.WinForms.Formats.Modules.Templates;

public static class IndexTagUtil
{
    /* ------------------------------------
     * Helper Methods
     * ------------------------------------
    */
    public static void EncodeFormatField(string value, OneOf<IIntField, IField<string>> encodedField, JObject dex, string formatName)
    {
        encodedField.Switch(
            x => x.Value = dex.GetIndexedValue<int?>(formatName, value, "Indices") ?? 0,
            x => x.Value = dex.GetIndexedValue<string>(formatName, value, "Indices"));
    }

    public static string DecodeFormatField(OneOf<IIntField, IField<string>> encodedField, JObject dex, string formatName)
        => encodedField.Match(
            x => dex.SearchIndexedValue<int?>(x.GetAs<int>(), formatName, "Indices", "$x"),
            x => dex.SearchIndexedValue(x.Value, formatName, "Indices", "$x"));


    /* ------------------------------------
     * Porting
     * ------------------------------------
    */
    public static AlertType ExportIndexTagNew(IField<string> pkuTag, OneOf<IIntField, IField<string>> formatVal,
        string defaultVal, Func<string, (bool, int)> tryGetIntID, Func<string, (bool, string)> tryGetStringID)
    {
        bool helper(OneOf<IIntField, IField<string>> fv, string val)
        {
            bool a = false;
            fv.Switch(
                x =>
                {
                    (a, int b) = tryGetIntID(val);
                    if (!a)
                        (_, b) = tryGetIntID(defaultVal);
                    x.Value = b;
                },
                x =>
                {
                    (a, string b) = tryGetStringID(val);
                    if (!a)
                        (_, b) = tryGetStringID(defaultVal);
                    x.Value = b;
                });
            return a;
        }

        AlertType at = AlertType.UNSPECIFIED;

        if (!pkuTag.IsNull()) //specified
        {
            bool valid = helper(formatVal, pkuTag.Value);
            if (!valid)
                at = AlertType.INVALID;
            else
                at = AlertType.NONE;
        }
        return at;
    }

    public static AlertType ExportIndexTag(IField<string> pkuTag, OneOf<IIntField, IField<string>> formatVal,
        string defaultVal, JObject dex, string formatName)
    {
        AlertType at = AlertType.UNSPECIFIED;
        string finalVal = defaultVal;

        if (!pkuTag.IsNull()) //specified
        {
            if (dex.ExistsIn(formatName, pkuTag.Value)) //valid
            {
                finalVal = pkuTag.Value;
                at = AlertType.NONE;
            }
            else //invalid
                at = AlertType.INVALID;
        }

        EncodeFormatField(finalVal, formatVal, dex, formatName);
        return at;
    }

    public static AlertType ImportIndexTag(IField<string> pkuTag,
        OneOf<IIntField, IField<string>> encodedField, JObject dex, string formatName)
    {
        AlertType at = AlertType.NONE;
        string decodedVal = DecodeFormatField(encodedField, dex, formatName);

        if (dex.ExistsIn(formatName, decodedVal)) //valid
            pkuTag.Value = decodedVal;
        else //invalid
        {
            pkuTag.Value = null;
            at = AlertType.INVALID;
        }
        return at;
    }


    /* ------------------------------------
     * Alerting
     * ------------------------------------
    */
    public static Alert GetIndexAlert(string tagName, AlertType at, string val, string defaultVal, bool ignoreUnspecified = false) => at switch
    {
        AlertType.NONE => null,
        AlertType.UNSPECIFIED => ignoreUnspecified ? null : new(tagName, $"No {tagName.ToLowerInvariant()} was specified, using the default: {defaultVal ?? "None"}."),
        AlertType.INVALID => new(tagName, $"The {tagName.ToLowerInvariant()} \"{val}\" is not supported by this format, using the default: {defaultVal ?? "None"}."),
        _ => throw InvalidAlertType(at)
    };
}
