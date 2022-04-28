using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Shiny_O
{
    public IField<bool> Shiny { get; }
}

public interface Shiny_E : BooleanTag_E
{
    public Shiny_O Shiny_Field { get; }
    public bool Shiny_Gen6Odds => false;
    public bool Shiny_PIDDependent => false;

    public TID_O TID_Field { get => null; set { } }
    public ChoiceAlert PID_DependencyError { get => null; set { } }
    public Dictionary<string, object> PID_DependencyDigest { get => null; set { } }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(PID_E.ExportPID), nameof(TID_E.ExportTID))]
    public void ExportShiny()
    {
        AlertType at;
        if (Shiny_PIDDependent)
        {
            BackedField<bool> dummyField = new();
            at = ExportBooleanTag(pku.Shiny, dummyField, false);
            PID_DependencyDigest["Shiny"] = (dummyField.Value, TID_Field.TID.GetAs<uint>());

            //add to pid dep error if necessary
            if (PID_DependencyError is not null && dummyField.Value != Shiny_Field.Shiny.Value)
            {
                var x = PID_DependencyError.Choices;
                x[0].Message = x[0].Message.AddNewLine($"Shiny: {Shiny_Field.Shiny.Value}");
                x[1].Message = x[1].Message.AddNewLine($"Shiny: {dummyField.Value}");
            }
        }
        else //pid independent
            at = ExportBooleanTag(pku.Shiny, Shiny_Field.Shiny, false);

        //alerting
        if (at is not AlertType.UNSPECIFIED) //silent when unspecified
            GetBooleanAlert("Shiny", at, false);
    }
}