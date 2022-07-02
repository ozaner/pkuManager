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
    public bool UseOfficialValues => false;

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Origin_Game_E.ExportOrigin_Game))]
    public void ExportMet_Location()
    {
        (AlertType at, string defaultLoc) = LocationTagUtil.ExportLocation(pku.GameField(UseOfficialValues),
            pku.Catch_Info.Met_Location, (Data as Met_Location_O).Met_Location);
        Warnings.Add(LocationTagUtil.GetLocationAlert("Met Location", at, defaultLoc, pku.Catch_Info.Met_Location.Value));
    }
}