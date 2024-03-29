﻿using OneOf;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using System.Collections.Generic;
using System.Linq;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Moves_O
{
    public OneOf<IIntArrayField, IField<string[]>> Moves { get; }
}

public interface Moves_E : Tag
{
    public string[] Moves_Indices { set; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMoves()
    {
        //Exporting
        var movesField = (Data as Moves_O).Moves;
        List<string> pkuMoves = pku.Moves.Keys.ToList();
        (AlertType at, var chosen) = MoveTagUtil.ExportMoveSet(FormatName, pkuMoves, movesField);

        //Alerting
        int? maxMoves = movesField.Match(x => x.Value?.Length, x => x.Value?.Length);
        Alert alert = MoveTagUtil.GetMoveSetAlert("Moves", at, maxMoves, chosen.Length);
        Warnings.Add(alert);
        
        //Set move indices
        Moves_Indices = chosen;
    }
}