using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Utilities;
using static pkuManager.WinForms.Alerts.Alert;

namespace pkuManager.WinForms.Formats.Modules.Templates;

public static class LocationTagUtil
{
    public static (AlertType, string defaultLoc) ExportLocation(IField<string> pkuGameField,
        IField<string> pkuLocationField, IIntField formatField)
    {
        string game = pkuGameField.Value;
        string defaultLoc = GAME_DEX.ReadDataDex<string>(game, "Locations", "0") ?? "None";
        AlertType at = AlertType.NONE;

        if (pkuLocationField.IsNull()) //unspecified
            (at, formatField.Value) = (AlertType.UNSPECIFIED, 0);
        else //specified
        {
            //try get location id
            bool succ = int.TryParse(GAME_DEX.SearchDataDex(pkuLocationField.Value, game, "Locations", "$x"), out int temp);
            int? locID = succ ? temp : null;

            if (locID is null) //invalid
                (at, formatField.Value) = (AlertType.INVALID, 0);
            else //valid
                formatField.Value = locID.Value;
        }
        
        return (at, defaultLoc);
    }

    public static Alert GetLocationAlert(string tagName, AlertType at, string defaultLoc, string pkuValue)
    {
        if (at is AlertType.NONE)
            return null;

        string msg = at switch
        {
            AlertType.INVALID => $"The location \"{pkuValue}\" doesn't exist in specified origin game.",
            AlertType.UNSPECIFIED => $"The met location was unspecified.",
            _ => throw InvalidAlertType(at)
        } + $" Using the default location: {defaultLoc ?? "None"}.";

        return new(tagName, msg);
    }
}