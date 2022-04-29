using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
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
    public Ball_O Ball_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportBall()
    {
        AlertType at = ExportIndexTag(pku.Catch_Info.Ball, "Poké Ball", Ball_Field.IsValid, x => Ball_Field.AsString = x);
        Warnings.Add(GetIndexAlert("Ball", at, pku.Catch_Info.Ball.Value, "Poké Ball"));
    }
}