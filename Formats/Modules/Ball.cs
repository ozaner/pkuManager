using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Ball_O
{
    public OneOf<IntegralField, Field<string>> Ball { get; }
}

public interface Ball_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Ball_O Data { get; }

    public string Ball_Default => "Poké Ball";
    public bool Ball_AlertIfUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessBall()
        => ProcessIndexTag("Ball", pku.Catch_Info.Ball, Ball_Default, Data.Ball, Ball_AlertIfUnspecified,
            x => BALL_DEX.ExistsIn(FormatName, x), x => BALL_DEX.GetIndexedValue<int?>(FormatName, x, "Indices") ?? 0);
}