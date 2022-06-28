using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Met_Location_O
{
    public IIntField Met_Location { get; }
}

public interface Met_Location_E : Tag
{
    public string Origin_Game_Name { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Origin_Game_E.ExportOrigin_Game))]
    public void ExportMet_Location()
    {
        (AlertType at, string defaultLoc) = LocationTagUtil.ExportLocation(Origin_Game_Name,
            pku.Catch_Info.Met_Location, (Data as Met_Location_O).Met_Location);
        Warnings.Add(LocationTagUtil.GetLocationAlert("Met Location", at, defaultLoc, pku.Catch_Info.Met_Location.Value));
    }
}