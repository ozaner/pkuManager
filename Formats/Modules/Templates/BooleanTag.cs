using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface BooleanTag_E : Tag
{
    protected void ExportBooleanTag(string tagName, IField<bool?> pkuVal,
        IField<bool> formatVal, bool defaultVal, bool alertIfUnspecified)
    {
        (bool checkedVal, AlertType at) = pkuVal.Value switch
        {
            null => (defaultVal, alertIfUnspecified ? AlertType.UNSPECIFIED : AlertType.NONE),
            _ => (pkuVal.Value.Value, AlertType.NONE)
        };
        formatVal.Value = checkedVal;
        Warnings.Add(GetBooleanAlert(tagName, at, defaultVal));
    }

    protected static Alert GetBooleanAlert(string name, AlertType at, bool defaultVal) => at switch
    {
        AlertType.UNSPECIFIED => new(name, $"{name} tag not specified, using the default of {defaultVal}."),
        AlertType.NONE => null,
        _ => throw InvalidAlertType(at)
    };
}

public interface BooleanTag_I : Tag
{
    protected void ImportBooleanTag(string tagName, IField<bool?> pkuVal, IField<bool> formatVal, bool explicitFalse)
    {
        if (!explicitFalse && !formatVal.Value)
            pkuVal.Value = null;
        else
            pkuVal.Value = formatVal.Value;
    }
}