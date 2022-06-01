using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System.Collections;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Moves_O
{
    public OneOf<IIntArrayField, IField<string[]>> Moves { get; }
}

public interface Moves_E : Tag
{
    public int[] Moves_Indices { set; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMoves()
    {
        Moves_O movesObj = Data as Moves_O;
        int? moveCount = movesObj.Moves.Match(x => x.Value?.Length, x => x.Value?.Length);

        List<int> moveIndices = new(); //indices in pku
        IList moveIDs = movesObj.Moves.IsT0 ? new List<int>() : new List<string>();
        AlertType at = AlertType.NONE;

        int confirmedMoves = 0;
        if (pku.Moves is not null)
        {
            bool invalid = false; //at least one move is invalid
            for (int i = 0; i < pku.Moves.Length; i++)
            {
                if (MOVE_DEX.ExistsIn(FormatName, pku.Moves[i].Name.Value)) //move exists in format
                {
                    if (confirmedMoves < moveCount || moveCount is null)
                    {
                        movesObj.Moves.Switch(
                            _ => moveIDs.Add(MOVE_DEX.GetIndexedValue<int?>(FormatName, pku.Moves[i].Name.Value, "Indices") ?? 0),
                            _ => moveIDs.Add(MOVE_DEX.GetIndexedValue<string>(FormatName, pku.Moves[i].Name.Value, "Indices")));
                        moveIndices.Add(i);
                        confirmedMoves++;
                    }
                }
                else //move DNE in format
                    invalid = true;
            }

            if (invalid)
                at = AlertType.INVALID;
            else if (moveCount.HasValue && confirmedMoves != pku.Moves.Length)
                at = AlertType.TOO_LONG;
        }
        else
            at = AlertType.UNSPECIFIED;

        while (moveIDs.Count < moveCount) //fill up empty slots with None
            movesObj.Moves.Switch(
                _ => moveIDs.Add(MOVE_DEX.GetIndexedValue<int?>(FormatName, "None", "Indices") ?? 0),
                _ => moveIDs.Add(MOVE_DEX.GetIndexedValue<string>(FormatName, "None", "Indices")));

        Alert alert = GetMoveAlert(at, moveCount, confirmedMoves);
        movesObj.Moves.Switch(
            x => x.SetAs((moveIDs as List<int>).ToArray()),
            x => x.Value = (moveIDs as List<string>).ToArray()
        );
        Moves_Indices = moveIndices.ToArray();
        Warnings.Add(alert);
    }

    protected static Alert GetMoveAlert(AlertType at, int? moveCount, int movesUsed)
    {
        if (at is AlertType.NONE)
            return null;

        string msg = "";
        if (at.HasFlag(AlertType.UNSPECIFIED))
            msg = "This pku has no moves, the Pokemon's moveset will be empty.";
        else if (at.HasFlag(AlertType.INVALID))
        {
            if (movesUsed is 0)
                msg = "None of the pku's moves are valid in this format, the Pokemon's moveset will be empty.";
            else
            {
                msg = $"Some of the pku's moves are invalid in this format, ";
                msg += at.HasFlag(AlertType.TOO_LONG) ? $"using the first {movesUsed} valid moves." : "ignoring them.";
            }
        }
        else if (at.HasFlag(AlertType.TOO_LONG))
            msg = $"This pku has more than {moveCount} valid moves, using the first {movesUsed} valid moves.";

        return new("Moves", msg);
    }
}