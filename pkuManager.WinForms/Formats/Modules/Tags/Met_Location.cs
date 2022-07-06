using pkuManager.WinForms.Alerts;
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
        (AlertType at, string defaultLoc, string invalidLoc) = LocationTagUtil.ExportIntLocation(pku.Catch_Info.Met_Location,
            pku.Catch_Info.Official_Met_Location, Origin_Game_Name, (Data as Met_Location_O).Met_Location);
        Warnings.Add(GetMet_LocationAlert(at, defaultLoc, invalidLoc));
    }

    public Alert GetMet_LocationAlert(AlertType at, string defaultLoc, string invalidLoc)
        => GetMet_LocationAlertBase(at, defaultLoc, invalidLoc);

    public Alert GetMet_LocationAlertBase(AlertType at, string defaultLoc, string invalidLoc)
        => LocationTagUtil.GetLocationAlert("Met Location", at, defaultLoc, invalidLoc);
}