using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;
using System.Linq;
using static pkuManager.WinForms.Alerts.Alert;

namespace pkuManager.WinForms.Formats.Modules.Templates;

public static class LocationTagUtil
{
    public static (AlertType at, string defaultLoc, string invalidLoc) ExportIntLocation(IField<string> pkuLocationField,
        IField<string> pkuOfficialLocationField, string formatGame, IIntField formatField)
    {
        AlertType at = AlertType.NONE;
        formatField.Value = 0; //init location to 0

        //split location from game override
        string location = pkuObject.ChooseField(true, pkuLocationField, pkuOfficialLocationField).Value;
        if (location is null) //unspecified
            at = AlertType.UNSPECIFIED;
        else //specified
        {
            var temp = location.Split("//", 2);
            if (temp.Length is 2) //has game override
            {
                string gameOverride = temp[0];
                if (gameOverride != formatGame)
                    at = AlertType.MISMATCH; //game override must match the format's game
                location = temp[1];
            }
            else //no game override
                location = temp[0];
        }

        if (at is AlertType.NONE) //specified and no mismatch
        {
            //try get location id
            bool succ = int.TryParse(GAME_DEX.SearchDataDex(location, formatGame, "Locations", "$x"), out int temp2);
            int? locID = succ ? temp2 : null;

            if (locID is null) //location invalid
                at = AlertType.INVALID;
            else //location valid
                formatField.Value = locID.Value;
        }

        string defaultLoc = GAME_DEX.ReadDataDex<string>(formatGame, "Locations", "0") ?? "None";
        return (at, defaultLoc, location);
    }

    public static void ExportStringLocation(IField<string> pkuLocationField, IField<string> formatField)
    {
        if (pkuLocationField.IsNull())
        {
            formatField.Value = null;
            return;
        }
        string locationAndMeta = pkuLocationField.Value.Split("//", 2).Last(); //remove "game//" in "game//location/meta"
        string location = locationAndMeta.Split("/", 2).First(); //remove "/meta" in "location/meta"
        formatField.Value = location;
    }

    public static Alert GetLocationAlert(string tagName, AlertType at, string defaultLoc, string invalidLoc)
    {
        if (at is AlertType.NONE)
            return null;

        string msg = at switch
        {
            AlertType.MISMATCH => "The location's game override doesn't match the specified origin game.",
            AlertType.INVALID => $"The location \"{invalidLoc}\" doesn't exist in the specified origin game.",
            AlertType.UNSPECIFIED => $"The met location was unspecified.",
            _ => throw InvalidAlertType(at)
        } + $" Using the default location: {defaultLoc}.";

        return new(tagName, msg);
    }
}