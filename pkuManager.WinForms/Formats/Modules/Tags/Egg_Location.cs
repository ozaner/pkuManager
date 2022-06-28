using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Egg_Location_O
{
    public IIntField Egg_Location { get; }
}

public interface Egg_Location_E : Tag
{
    public string Origin_Game_Name { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Origin_Game_E.ExportOrigin_Game))]
    public void ExportEgg_Location()
    {
        (AlertType at, string defaultLoc) = LocationTagUtil.ExportLocation(Origin_Game_Name,
            pku.Egg_Info.Received_Location, (Data as Egg_Location_O).Egg_Location);
        Warnings.Add(LocationTagUtil.GetLocationAlert("Egg Received Location", at, defaultLoc, pku.Egg_Info.Received_Location.Value));
    }
}