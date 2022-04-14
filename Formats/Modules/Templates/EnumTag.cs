using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface EnumTag_E
{
    public List<Alert> Warnings { get; }

    protected void ExportEnumTag<T>(string tagName, IField<string> tag, T? defaultVal,
        OneOf<IField<BigInteger>, IField<T>, IField<T?>> formatVal, bool alertIfUnspecified,
        Func<AlertType, string, string, Alert> alertFunc, Predicate<T> isValid = null) where T : struct, Enum
    {
        AlertType at = AlertType.NONE;
        T? finalVal = defaultVal;

        T? tagEnum = tag.ToEnum<T>();
        if (tagEnum.HasValue && (isValid is null || isValid(tagEnum.Value))) //tag specified & exists
            finalVal = tagEnum.Value;
        else if (!tag.IsNull()) //tag specified & DNE
            at = AlertType.INVALID;
        else //tag unspecified
            at = alertIfUnspecified ? AlertType.UNSPECIFIED : AlertType.NONE;

        formatVal.Switch(x => x.Value = Convert.ToInt32(finalVal),
                         x => x.Value = finalVal.Value, //exception if default is null, but formatVal is non-nullable 
                         x => x.Value = finalVal);
        if (alertFunc is not null)
            Warnings.Add(alertFunc(at, tag.Value, defaultVal?.ToFormattedString()));
    }

    protected static Alert GetEnumAlert(string tagName, AlertType at, string val, string defaultVal) => at switch
    {
        AlertType.NONE => null,
        AlertType.UNSPECIFIED => new(tagName, $"No {tagName.ToLowerInvariant()} was specified, using the default: {defaultVal ?? "none"}."),
        AlertType.INVALID => new(tagName, $"The {tagName.ToLowerInvariant()} \"{val}\" is not supported by this format, using the default: {defaultVal ?? "none"}."),
        _ => throw InvalidAlertType(at)
    };
}