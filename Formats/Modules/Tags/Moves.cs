using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Moves_O
{
    public OneOf<IField<BigInteger[]>, IField<string[]>> Moves { get; }
}

public interface Moves_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Moves_O Moves_Field { get; }
    public int[] Moves_Indices { set; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessMoves()
    {
        List<int> moveIndices = new(); //indices in pku
        int[] moveIDsInt = new int[4]; //index numbers for format
        string[] moveIDsStr = new string[4]; //index names for format
        Alert alert = null;
        if (pku.Moves is not null)
        {
            int confirmedMoves = 0;
            bool invalid = false; //at least one move is invalid

            for (int i = 0; i < pku.Moves.Length; i++)
            {
                if (MOVE_DEX.ExistsIn(FormatName, pku.Moves[i].Name.Value)) //move exists in format
                {
                    if (confirmedMoves < 4)
                    {
                        Moves_Field.Moves.Switch(
                            _ => moveIDsInt[confirmedMoves] = MOVE_DEX.GetIndexedValue<int?>(FormatName, pku.Moves[i].Name.Value, "Indices") ?? 0,
                            _ => moveIDsStr[confirmedMoves] = MOVE_DEX.GetIndexedValue<string>(FormatName, pku.Moves[i].Name.Value, "Indices"));
                        moveIndices.Add(i);
                        confirmedMoves++;
                    }
                }
                else //move DNE in format
                    invalid = true;
            }

            if (confirmedMoves != pku.Moves.Length) //not a perfect match
                alert = invalid ? GetMoveAlert(AlertType.INVALID, confirmedMoves) : GetMoveAlert(AlertType.TOO_LONG);
        }
        else
            alert = GetMoveAlert(AlertType.UNSPECIFIED);

        Moves_Field.Moves.Switch(
            x => x.SetAs(moveIDsInt),
            x => x.Value = moveIDsStr
        );
        Moves_Indices = moveIndices.ToArray();
        Warnings.Add(alert);
    }

    protected static Alert GetMoveAlert(AlertType at, int? movesUsed = null)
    {
        if (at is AlertType.INVALID && movesUsed is null)
            throw new ArgumentNullException(nameof(movesUsed), $"Must specify how many moves used for INVALID alerts.");
        return new("Moves", at switch
        {
            AlertType.UNSPECIFIED => "This pku has no moves, the Pokemon's moveset will be empty.",
            AlertType.INVALID => movesUsed is 0 ? "None of the pku's moves are valid in this format, the Pokemon's moveset will be empty." :
                    $"Some of the pku's moves are invalid in this format, using the first {movesUsed} valid moves.",
            AlertType.TOO_LONG => "This pku has more than 4 valid moves, using the first 4.",
            _ => throw InvalidAlertType(at)
        });
    }
}