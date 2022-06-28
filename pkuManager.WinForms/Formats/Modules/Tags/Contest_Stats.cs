using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Contest_Stats_O
{
    public IIntArrayField Contest_Stats { get; }
}

public interface Contest_Stats_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportContest_Stats()
    {
        var contestStats = (Data as Contest_Stats_O).Contest_Stats;
        AlertType[] ats = NumericTagUtil.ExportNumericArrayTag(pku.Contest_Stats_Array, contestStats, 0);
        Alert a = NumericTagUtil.GetNumericArrayAlert("Contest Stats", TagUtil.CONTEST_STAT_NAMES, ats, contestStats, 0, true);
        Warnings.Add(a);
    }
}