using Newtonsoft.Json.Linq;
using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface IndexTag_O
{
    public string FormatName { get; }

    protected bool IsValid(JObject dex, string val) => dex.ExistsIn(FormatName, val);

    protected string AsStringGet(JObject dex, OneOf<IField<BigInteger>, IField<string>> field)
        => field.Match(
            x => dex.SearchIndexedValue<int?>(x.GetAs<int>(), FormatName, "Indices", "$x"),
            x => dex.SearchIndexedValue(x.Value, FormatName, "Indices", "$x"));

    protected void AsStringSet(JObject dex, OneOf<IField<BigInteger>, IField<string>> field, string value)
        => field.Switch(
            x => x.Value = dex.GetIndexedValue<int?>(FormatName, value, "Indices") ?? 0,
            x => x.Value = dex.GetIndexedValue<string>(FormatName, value, "Indices"));
}

public interface IndexTag_E : Tag
{
    protected void ExportIndexTag(string tagName, IField<string> pkuTag, string defaultVal,
        bool alertIfUnspecified, Predicate<string> isValid, Action<string> setIndexField)
    {
        AlertType at = AlertType.NONE;
        string finalVal = defaultVal;

        if (!pkuTag.IsNull() && isValid(pkuTag.Value)) //tag specified & exists
            finalVal = pkuTag.Value;
        else if (!pkuTag.IsNull()) //tag specified & DNE
            at = AlertType.INVALID;
        else //tag unspecified
            at = alertIfUnspecified ? AlertType.UNSPECIFIED : AlertType.NONE;

        setIndexField(finalVal);
        Warnings.Add(GetIndexAlert(tagName, at, pkuTag.Value, defaultVal));
    }

    protected static Alert GetIndexAlert(string tagName, AlertType at, string val, string defaultVal) => at switch
    {
        AlertType.NONE => null,
        AlertType.UNSPECIFIED => new(tagName, $"No {tagName.ToLowerInvariant()} was specified, using the default: {defaultVal ?? "None"}."),
        AlertType.INVALID => new(tagName, $"The {tagName.ToLowerInvariant()} \"{val}\" is not supported by this format, using the default: {defaultVal ?? "None"}."),
        _ => throw InvalidAlertType(at)
    };
}

public interface IndexTag_I : ByteOverride_I
{
    protected void ImportIndexTag(string tagName, IField<string> pkuTag, bool isValid, string asString, IField<BigInteger> encodedField)
    {
        if (isValid) //valid
            pkuTag.Value = asString;
        else if (encodedField is IByteOverridable bf) //invalid encoded language, add to byte override
            Warnings.Add((this as ByteOverride_I).AddByteOverrideCMD(tagName, bf.GetOverride()));
        //first two cases didn't work. invalid string value? no string override yet...
    }
}