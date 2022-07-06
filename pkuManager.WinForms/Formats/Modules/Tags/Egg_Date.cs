using OneOf;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Egg_Date_O
{
    OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> Egg_Date { get; }
}

public interface Egg_Date_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportEgg_Date()
    {
        AlertType at = DateTagUtil.ExportDate(pku.Egg_Info.Received_Date, (Data as Egg_Date_O).Egg_Date);
        if (at is not AlertType.UNSPECIFIED) //silent unspecified
            Warnings.Add(DateTagUtil.GetDateAlert("Egg Received Date", at));
    }
}