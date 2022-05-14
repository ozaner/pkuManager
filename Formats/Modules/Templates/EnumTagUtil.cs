using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public static class EnumTagUtil<T> where T : struct, Enum
{
    /* ------------------------------------
     * Exporting
     * ------------------------------------
    */
    public static AlertType ExportEnumTag(IField<string> pkuTag, OneOf<IField<BigInteger>, IField<T>> formatVal,
        T defaultVal, Predicate<T> isValid = null)
    {
        AlertType at = AlertType.UNSPECIFIED;
        T finalVal = defaultVal;

        if (!pkuTag.IsNull()) //specified
        {
            T? tagEnum = pkuTag.ToEnum<T>();
            if (tagEnum.HasValue && (isValid is null || isValid(tagEnum.Value))) //valid
            {
                finalVal = tagEnum.Value;
                at = AlertType.NONE;
            }
            else //invalid
                at = AlertType.INVALID;
        }

        formatVal.Switch(x => x.Value = Convert.ToInt32(finalVal),
                         x => x.Value = finalVal);
        return at;
    }

    public static void ExportMultiEnumTag(IDictionary<T, IField<bool>> enumToFieldMap, HashSet<T> enums)
    {
        foreach ((T e, IField<bool> field) in enumToFieldMap)
            if (field is not null)
                field.Value = enums.Contains(e);
        //no need to alert, so no need to keep track of alerttypes
    }


    /* ------------------------------------
     * Importing
     * ------------------------------------
    */
    public static AlertType ImportEnumTag(IField<string> pkuTag, OneOf<IField<BigInteger>, IField<T>> encodedField)
    {
        AlertType at = AlertType.NONE;
        pkuTag.Value = encodedField.Match(
            x =>
            {
                T? attemptedVal = x.Value.ToEnum<T>();
                if (attemptedVal is null)
                    at = AlertType.INVALID;
                return attemptedVal.ToFormattedString();
            },
            x => x.Value.ToFormattedString());
        return at;
    }


    /* ------------------------------------
     * Alerting
     * ------------------------------------
    */
    public static Alert GetEnumAlert(string tagName, AlertType at, string val, OneOf<T, string> defaultVal)
    {
        string defaultValString = defaultVal.Match(
            x => $"using the default: {x.ToFormattedString()}",
            x => x);
        return at switch
        {
            AlertType.NONE => null,
            AlertType.UNSPECIFIED => new(tagName, $"No {tagName.ToLowerInvariant()} was specified, {defaultValString}"),
            AlertType.INVALID => new(tagName, $"The {tagName.ToLowerInvariant()} \"{val}\" is not supported by this format, {defaultValString}"),
            _ => throw InvalidAlertType(at)
        };
    }
}
