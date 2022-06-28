using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Utilities;
using static pkuManager.WinForms.Alerts.Alert;

namespace pkuManager.WinForms.Formats.Modules.Templates;

public static class LocationTagUtil
{
    //public string Origin_Game_Name { get; }

    public static (AlertType, string defaultLoc) ExportLocation(string originGameName, IField<string> pkuField, IIntField formatField)
    {
        AlertType at = AlertType.NONE;
        string defaultLoc = GAME_DEX.ReadDataDex<string>(originGameName, "Locations", "0") ?? "None";

        if (pkuField.IsNull()) //unspecified
            (at, formatField.Value) = (AlertType.UNSPECIFIED, 0);
        else //specified
        {
            //try get location id
            bool succ = int.TryParse(GAME_DEX.SearchDataDex(pkuField.Value, originGameName, "Locations", "$x"), out int temp);
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