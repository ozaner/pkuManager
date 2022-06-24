using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface LocationTag_E : Tag
{
    public string Origin_Game_Name { get; }

    protected void ExportLocation(string tagName, IField<string> pkuField, IIntField formatField)
    {
        //get default location
        string defaultLoc() => GAME_DEX.ReadDataDex<string>(Origin_Game_Name, "Locations", "0") ?? "None";

        //null check
        if (pkuField.IsNull())
        {
            formatField.Value = 0;
            Warnings.Add(GetLocationAlert(tagName, AlertType.UNSPECIFIED, defaultLoc()));
            return;
        }

        //try get location id
        bool succ = int.TryParse(GAME_DEX.SearchDataDex(pkuField.Value, Origin_Game_Name, "Locations", "$x"), out int temp);
        int? locID = succ ? temp : null;

        //location failure
        if (locID is null)
        {
            formatField.Value = 0;
            Warnings.Add(GetLocationAlert(tagName, AlertType.INVALID, defaultLoc(), pkuField.Value));
        }
        else //location success
            formatField.Value = locID.Value;
    }

    protected static Alert GetLocationAlert(string tagName, AlertType at, string defaultLoc, string invalidLoc = null) => new(tagName, at switch
    {
        AlertType.INVALID => $"The location \"{invalidLoc}\" doesn't exist in specified origin game.",
        AlertType.UNSPECIFIED => $"The met location was unspecified.",
        _ => throw InvalidAlertType(at)
    } + $" Using the default location: {defaultLoc ?? "None"}.");
}