using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Egg_Date_O
{
    OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> Egg_Date { get; }
}

public interface Egg_Date_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportEgg_Date()
    {
        AlertType at = DateTagUtil.ExportDate(pku.Egg_Info.Met_Date, (Data as Egg_Date_O).Egg_Date);
        Warnings.Add(DateTagUtil.GetDateAlert("Egg Received Date", at));
    }
}