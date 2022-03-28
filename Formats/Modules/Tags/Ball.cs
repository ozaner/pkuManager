using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ball_O
{
    public OneOf<IField<BigInteger>, IField<string>> Ball { get; }
}

public interface Ball_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Ball_O Ball_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessBall()
        => ProcessIndexTag("Ball", pku.Catch_Info.Ball, "Poké Ball", Ball_Field.Ball, true,
            x => BALL_DEX.ExistsIn(FormatName, x), x => BALL_DEX.GetIndexedValue<int?>(FormatName, x, "Indices") ?? 0);
}