using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using System;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules;

public interface IndexTag_E
{
    public List<Alert> Warnings { get; }

    protected void ProcessIndexTag(string tagName, Field<string> tag, string defaultVal,
        OneOf<IntegralField, Field<string>> formatVal, bool alertIfUnspecified, Predicate<string> isValid, Func<string, int> getIndex)
    {
        AlertType at = AlertType.NONE;
        string finalVal = defaultVal;

        if (!tag.IsNull && isValid(tag)) //tag specified & exists
            finalVal = tag;
        else if (!tag.IsNull) //tag specified & DNE
            at = AlertType.INVALID;
        else //tag unspecified
            at = alertIfUnspecified ? AlertType.UNSPECIFIED : AlertType.NONE;

        formatVal.Switch(x => x.Set(getIndex(finalVal)),
                         x => x.Set(finalVal));
        Warnings.Add(GetIndexAlert(tagName, at, tag, defaultVal));
    }

    protected Alert GetIndexAlert(string tagName, AlertType at, string val, string defaultVal) => at switch
    {
        AlertType.NONE => null,
        AlertType.UNSPECIFIED => new(tagName, $"No {tagName.ToLowerInvariant()} was specified, using the default: {defaultVal ?? "none"}."),
        AlertType.INVALID => new(tagName, $"The {tagName.ToLowerInvariant()} \"{val}\" is not supported by this format, using the default: {defaultVal ?? "none"}."),
        _ => throw InvalidAlertType(at)
    };
}