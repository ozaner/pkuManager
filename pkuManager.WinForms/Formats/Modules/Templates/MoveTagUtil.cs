using OneOf;
using pkuManager.Data.Dexes;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using System.Collections;
using System.Collections.Generic;
using static pkuManager.WinForms.Alerts.Alert;

namespace pkuManager.WinForms.Formats.Modules.Templates;

public class MoveTagUtil
{
    public static string GetTrueMove(string move)
    {
        int dupLoc = move.IndexOf('$');
        return dupLoc > -1 ? move[0..dupLoc] : move;
    }

    public static (AlertType, string[]) ExportMoveSet(string format, List<string> pkuMoves,
        OneOf<IIntArrayField, IField<string[]>> moveField)
    {
        bool getMoveID(string move, out OneOf<int, string> id)
        {
            bool exists;
            if (moveField.IsT0)
            {
                exists = DDM.TryGetMoveID(format, move, out int idN);
                id = idN;
            }
            else
            {
                exists = DDM.TryGetMoveID(format, move, out string idN);
                id = idN;
            }
            return exists;
        }

        int? maxMoves = moveField.Match(x => x.Value?.Length, x => x.Value?.Length);
        List<string> moveIndices = new(); //(canonical) names of all chosen moves
        IList moveIDs = moveField.IsT0 ? new List<int>() : new List<string>();
        AlertType at = AlertType.NONE;

        int confirmedMoves = 0;
        if (pkuMoves.Count > 0)
        {
            bool invalid = false;
            foreach (string move in pkuMoves)
            {
                string trueMove = GetTrueMove(move);
                if (getMoveID(trueMove, out OneOf<int, string> ID)) //move exists in format
                {
                    if (confirmedMoves < maxMoves || maxMoves is null)
                    {
                        moveIDs.Add(ID.Value);
                        moveIndices.Add(move);
                        confirmedMoves++;
                    }
                }
                else //move DNE in format
                    invalid = true;
            }

            if (invalid) //at least one move is invalid
                at = AlertType.INVALID;
            else if (maxMoves.HasValue && confirmedMoves != pkuMoves.Count)
                at = AlertType.TOO_LONG;
        }
        else
            at = AlertType.UNSPECIFIED;

        while (moveIDs.Count < maxMoves) //fill up empty slots with None
        {
            if (getMoveID("None", out OneOf<int, string> noneID))
                moveIDs.Add(noneID.Value);
            else
                moveIDs.Add(moveField.IsT0 ? 0 : null);
        }
        moveField.Switch(
            x => x.SetAs((moveIDs as List<int>).ToArray()),
            x => x.Value = (moveIDs as List<string>).ToArray()
        );
        return (at, moveIndices.ToArray());
    }

    public static Alert GetMoveSetAlert(string tagName, AlertType at, int? moveCount, int movesUsed)
    {
        if (at is AlertType.NONE)
            return null;

        string tagNameL = tagName.ToLowerInvariant();
        string msg = "";
        if (at.HasFlag(AlertType.UNSPECIFIED))
            msg = $"This pku has no {tagNameL}, leaving them empty.";
        else if (at.HasFlag(AlertType.INVALID))
        {
            if (movesUsed is 0)
                msg = $"None of the pku's {tagNameL} are valid in this format, leaving them empty.";
            else
            {
                msg = $"Some of the pku's {tagNameL} are invalid in this format, ";
                msg += at.HasFlag(AlertType.TOO_LONG) ? $"using the first {movesUsed} valid moves." : "ignoring them.";
            }
        }
        else if (at.HasFlag(AlertType.TOO_LONG))
            msg = $"This pku has more than {moveCount} valid {tagNameL}, using the first {movesUsed} valid moves.";

        return new($"{tagName}", msg);
    }
}
