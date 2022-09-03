using OneOf;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using System;
using static pkuManager.WinForms.Alerts.Alert;

namespace pkuManager.WinForms.Formats.Modules.Templates;

public static class IndexTagUtil
{
    /* ------------------------------------
     * Porting
     * ------------------------------------
    */
    public static AlertType ExportIndexTag(IField<string> pkuTag, OneOf<IIntField, IField<string>> formatVal,
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

    public static AlertType ImportIndexTag(IField<string> pkuTag, OneOf<IIntField, IField<string>> encodedField,
        Func<int, (bool, string)> tryGetFromIntID, Func<string, (bool, string)> tryGetFromStringID)
    {
        (bool valid, string val) = encodedField.Match(
                x => tryGetFromIntID(x.GetAs<int>()),
                x => tryGetFromStringID(x.Value));

        AlertType at = AlertType.NONE;
        if (valid)
            pkuTag.Value = val;
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
