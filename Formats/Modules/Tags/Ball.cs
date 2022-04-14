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
    public string FormatName { get; }

    public bool IsValid(string ball) => BALL_DEX.ExistsIn(FormatName, ball);
    public bool IsValid() => IsValid(AsString);

    public string AsString
    {
        get => Ball.Match(
            x => BALL_DEX.SearchIndexedValue<int?>(x.GetAs<int>(), FormatName, "Indices", "$x"),
            x => x.Value);
        set => Ball.Switch(
            x => x.Value = BALL_DEX.GetIndexedValue<int?>(FormatName, value, "Indices") ?? 0,
            x => x.Value = value);
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