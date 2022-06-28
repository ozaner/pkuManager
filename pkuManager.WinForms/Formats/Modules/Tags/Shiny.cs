using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Fields.BackedFields;
using pkuManager.WinForms.Formats.Modules.Templates;
using pkuManager.WinForms.Utilities;
using System.Collections.Generic;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Shiny_O
{
    public IField<bool> Shiny { get; }
    public bool Shiny_Gen6Odds => false;
    public bool Shiny_PIDDependent => false;
}

public interface Shiny_E : Tag
{
    public ChoiceAlert PID_DependencyError { get => null; set { } }
    public Dictionary<string, object> PID_DependencyDigest { get => null; set { } }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(PID_E.ExportPID), nameof(TID_E.ExportTID))]
    public void ExportShiny()
    {
        Shiny_O shinyObj = Data as Shiny_O;

        if (shinyObj.Shiny_PIDDependent)
        {
            BackedField<bool> dummyField = new();
            BooleanTagUtil.ExportBooleanTag(pku.Shiny, dummyField, false);
            PID_DependencyDigest["Shiny"] = (dummyField.Value, (Data as TID_O).TID.GetAs<uint>());

            //add to pid dep error if necessary
            if (PID_DependencyError is not null && dummyField.Value != shinyObj.Shiny.Value)
            {
                var x = PID_DependencyError.Choices;
                x[0].Message = x[0].Message.AddNewLine($"Shiny: {shinyObj.Shiny.Value}");
                x[1].Message = x[1].Message.AddNewLine($"Shiny: {dummyField.Value}");
            }
        }
        else //pid independent
            BooleanTagUtil.ExportBooleanTag(pku.Shiny, shinyObj.Shiny, false);

        //no alerting needed
    }
}

public interface Shiny_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportShiny() //PID and TID should be settled for pid dep
        => BooleanTagUtil.ImportBooleanTag(pku.Shiny, (Data as Shiny_O).Shiny, false);
}