using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Met_Location_O
{
    public IField<BigInteger> Met_Location { get; }
}

public interface Met_Location_E : Tag
{
    public Met_Location_O Met_Location_Field { get; }
    public string Origin_Game_Name { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Origin_Game_E.ExportOrigin_Game))]
    public void ExportMet_Location()
    {
        //override game for met location
        string checkedGameName = pku.Catch_Info.Met_Game_Override.Value ?? Origin_Game_Name;

        //get default location
        string defaultLoc() => GAME_DEX.ReadDataDex<string>(checkedGameName, "Locations", "0") ?? "None";

        //null check
        if (pku.Catch_Info.Met_Location.IsNull())
        {
            Met_Location_Field.Met_Location.Value = 0;
            Warnings.Add(GetMetLocationAlert(AlertType.UNSPECIFIED, defaultLoc()));
            return;
        }

        //try get location id
        bool succ = int.TryParse(GAME_DEX.SearchDataDex(pku.Catch_Info.Met_Location.Value, checkedGameName, "Locations", "$x"), out int temp);
        int? locID = succ ? temp : null;

        //location failure
        if (locID is null)
        {
            Met_Location_Field.Met_Location.Value = 0;
            Warnings.Add(GetMetLocationAlert(AlertType.INVALID, defaultLoc(), pku.Catch_Info.Met_Location.Value));
        }
        else //location success
            Met_Location_Field.Met_Location.Value = locID.Value;
    }

    protected static Alert GetMetLocationAlert(AlertType at, string defaultLoc, string invalidLoc = null) => new("Met Location", at switch
    {
        AlertType.INVALID => $"The location \"{invalidLoc}\" doesn't exist in specified origin game.",
        AlertType.UNSPECIFIED => $"The met location was unspecified.",
        _ => throw InvalidAlertType(at)
    } + $" Using the default location: { defaultLoc ?? "None"}.");
}