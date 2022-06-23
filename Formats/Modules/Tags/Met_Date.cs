using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Met_Date_O
{
    OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> Met_Date { get; }
}

public interface Met_Date_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMet_Date()
        => Warnings.Add(DateTagUtil.ExportDate("Met Date", pku.Catch_Info.Met_Date, (Data as Met_Date_O).Met_Date));
}