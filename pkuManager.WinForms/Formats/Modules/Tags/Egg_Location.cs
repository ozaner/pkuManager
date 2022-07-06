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
        (AlertType at, string defaultLoc, string invalidLoc) = LocationTagUtil.ExportIntLocation(pku.Egg_Info.Received_Location,
            pku.Egg_Info.Official_Received_Location, Origin_Game_Name, (Data as Egg_Location_O).Egg_Location);
        if (at is not AlertType.UNSPECIFIED) //silent unspecified
            Warnings.Add(LocationTagUtil.GetLocationAlert("Egg Received Location", at, defaultLoc, invalidLoc));
    }
}