using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ball_O : IndexTag_O
{
    public OneOf<IField<BigInteger>, IField<string>> Ball { get; }

    public bool IsValid(string ball) => IsValid(BALL_DEX, ball);
    public bool IsValid() => IsValid(AsString);

    public string AsString
    {
        get => AsStringGet(BALL_DEX, Ball);
        set => AsStringSet(BALL_DEX, Ball, value);
    }
}

public interface Ball_E : IndexTag_E
{
    public pkuObject pku { get; }
    public Ball_O Ball_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportBall()
        => ExportIndexTag("Ball", pku.Catch_Info.Ball, "Poké Ball", true,
            Ball_Field.IsValid, x => Ball_Field.AsString = x);
}