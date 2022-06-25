using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public static class BooleanTagUtil
{
    /* ------------------------------------
     * Exporting
     * ------------------------------------
    */
    public static AlertType ExportBooleanTag(IField<bool?> pkuVal, IField<bool> formatVal, bool defaultVal)
    {
        (bool checkedVal, AlertType at) = pkuVal.Value switch
        {
            null => (defaultVal, AlertType.UNSPECIFIED),
            _ => (pkuVal.Value.Value, AlertType.NONE)
        };
        formatVal.Value = checkedVal;
        return at;
    }


    /* ------------------------------------
     * Importing
     * ------------------------------------
    */
    public static void ImportBooleanTag(IField<bool?> pkuVal, IField<bool> formatVal, bool explicitFalse)
    {
        if (!explicitFalse && !formatVal.Value)
            pkuVal.Value = null;
        else
            pkuVal.Value = formatVal.Value;
    }


    /* ------------------------------------
     * Alerting
     * ------------------------------------
    */
    public static Alert GetBooleanAlert(string name, AlertType at, bool defaultVal)
    {
        if (at is AlertType.NONE)
            return null;
        else if (at is AlertType.UNSPECIFIED)
            return new(name, $"{name} tag not specified, using the default of {defaultVal}.");
        else
            throw InvalidAlertType(at);
    }
}