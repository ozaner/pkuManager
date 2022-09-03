using OneOf;
using pkuManager.Data.Dexes;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Ball_O
{
    public OneOf<IIntField, IField<string>> Ball { get; }
}

public interface Ball_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportBall()
    {
        AlertType at = IndexTagUtil.ExportIndexTag(pku.Catch_Info.Ball, (Data as Ball_O).Ball, "Poké Ball",
            (v) =>
            {
                bool a = DDM.TryGetBallID(FormatName, v, out int ID);
                return (a, ID);
            },
            (v) =>
            {
                bool a = DDM.TryGetBallID(FormatName, v, out string ID);
                return (a, ID);
            });
        Warnings.Add(IndexTagUtil.GetIndexAlert("Ball", at, pku.Catch_Info.Ball.Value, "Poké Ball"));
    }
}