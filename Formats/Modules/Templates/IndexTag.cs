﻿using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using System;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface IndexTag_E
{
    public List<Alert> Warnings { get; }

    protected void ExportIndexTag(string tagName, IField<string> tag, string defaultVal,
        bool alertIfUnspecified, Predicate<string> isValid, Action<string> setIndexField)
    {
        AlertType at = AlertType.NONE;
        string finalVal = defaultVal;

        if (!tag.IsNull() && isValid(tag.Value)) //tag specified & exists
            finalVal = tag.Value;
        else if (!tag.IsNull()) //tag specified & DNE
            at = AlertType.INVALID;
        else //tag unspecified
            at = alertIfUnspecified ? AlertType.UNSPECIFIED : AlertType.NONE;

        setIndexField(finalVal);
        Warnings.Add(GetIndexAlert(tagName, at, tag.Value, defaultVal));
    }

    protected static Alert GetIndexAlert(string tagName, AlertType at, string val, string defaultVal) => at switch
    {
        AlertType.NONE => null,
        AlertType.UNSPECIFIED => new(tagName, $"No {tagName.ToLowerInvariant()} was specified, using the default: {defaultVal ?? "None"}."),
        AlertType.INVALID => new(tagName, $"The {tagName.ToLowerInvariant()} \"{val}\" is not supported by this format, using the default: {defaultVal ?? "None"}."),
        _ => throw InvalidAlertType(at)
    };
}