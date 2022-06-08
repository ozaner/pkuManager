using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Collections.Generic;
using System.Linq;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Relearn_Moves_O
{
    public OneOf<IIntArrayField, IField<string[]>> Relearn_Moves { get; }
}

public interface Relearn_Moves_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportRelearn_Moves()
    {
        //Exporting
        var relearnMovesField = (Data as Relearn_Moves_O).Relearn_Moves;
        List<string> pkuRelearnMoves = pku.Movepool.Keys.Where(move => pku.Movepool[move].Relearn.Value == true).ToList();
        (AlertType at, var chosen) = MoveTagUtil.ExportMoveSet(FormatName, pkuRelearnMoves, relearnMovesField);

        //Alerting
        if (at is AlertType.UNSPECIFIED)
            return; //ignore unspecified
        int? maxMoves = relearnMovesField.Match(x => x.Value?.Length, x => x.Value?.Length);
        Alert alert = MoveTagUtil.GetMoveSetAlert("Moves", at, maxMoves, chosen.Length);
        Warnings.Add(alert);
    }
}