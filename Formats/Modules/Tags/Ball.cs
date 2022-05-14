using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ball_O
{
    public OneOf<IField<BigInteger>, IField<string>> Ball { get; }
}

public interface Ball_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportBall()
    {
        AlertType at = IndexTagUtil.ExportIndexTag(pku.Catch_Info.Ball, (Data as Ball_O).Ball, "Poké Ball", BALL_DEX, FormatName);
        Warnings.Add(IndexTagUtil.GetIndexAlert("Ball", at, pku.Catch_Info.Ball.Value, "Poké Ball"));
    }
}