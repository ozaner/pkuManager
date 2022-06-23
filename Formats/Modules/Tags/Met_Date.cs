using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Met_Date_O
{
    public IIntField Met_Date_Year { get; }
    public IIntField Met_Date_Month { get; }
    public IIntField Met_Date_Day { get; }
}

public interface Met_Date_Unix_O
{
    public IIntField Met_Date { get; }
}

public interface Met_Date_E : Tag
{

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMet_Date()
    {
        OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> formatVal = null;

        //Unix formats
        if (Data is Met_Date_Unix_O mdu)
            formatVal = OneOf<(IIntField Y, IIntField M, IIntField D), IIntField>.FromT1(mdu.Met_Date);

        //Y-M-D formats
        else if (Data is Met_Date_O md)
            formatVal = OneOf<(IIntField Y, IIntField M, IIntField D), IIntField>
                .FromT0((md.Met_Date_Year, md.Met_Date_Month, md.Met_Date_Day));

        Alert a2 = DateTagUtil.ExportAnyDate("Met Date", pku.Catch_Info.Met_Date, pku.Catch_Info.Timezone, formatVal);
        Warnings.Add(a2);
    }
}