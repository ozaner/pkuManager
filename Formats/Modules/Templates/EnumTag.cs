using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface EnumTag_E : Tag
{
    protected AlertType ExportEnumTag<T>(IField<string> tag, T defaultVal,
        OneOf<IField<BigInteger>, IField<T>> formatVal, Predicate<T> isValid = null) where T : struct, Enum
    {
        AlertType at = AlertType.NONE;
        T finalVal = defaultVal;

        T? tagEnum = tag.ToEnum<T>();
        if (tagEnum.HasValue && (isValid is null || isValid(tagEnum.Value))) //tag specified & exists
            finalVal = tagEnum.Value;
        else if (!tag.IsNull()) //tag specified & DNE
            at = AlertType.INVALID;
        else //tag unspecified
            at = AlertType.UNSPECIFIED;

        formatVal.Switch(x => x.Value = Convert.ToInt32(finalVal),
                         x => x.Value = finalVal);
        return at;
    }

    protected static Alert GetEnumAlert<T>(string tagName, AlertType at, string val, OneOf<T, string> defaultVal) where T : struct, Enum
    {
        string defaultValString = defaultVal.Match(
            x => $"using the default: {x.ToFormattedString()}",
            x => x);
        return at switch
        {
            AlertType.NONE => null,
            AlertType.UNSPECIFIED => new(tagName, $"No {tagName.ToLowerInvariant()} was specified, {defaultValString}."),
            AlertType.INVALID => new(tagName, $"The {tagName.ToLowerInvariant()} \"{val}\" is not supported by this format, {defaultValString}"),
            _ => throw InvalidAlertType(at)
        };
    }
}