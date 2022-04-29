using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Nature_O
{
    public OneOf<IField<BigInteger>, IField<Nature>> Nature { get; }
    public Nature Nature_Default => DEFAULT_NATURE;
    public bool Nature_PIDDependent => false;
}

public interface Nature_E : EnumTag_E
{
    public Nature_O Nature_Field { get; }

    public ChoiceAlert PID_DependencyError { get => null; set { } }
    public Dictionary<string, object> PID_DependencyDigest { get => null; set { } }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(PID_E.ExportPID))]
    public void ExportNature()
    {
        if (Nature_Field.Nature_PIDDependent)
        {
            BackedField<Nature> dummyField = new();
            AlertType at = ExportEnumTag(pku.Nature, Nature_Field.Nature_Default, dummyField);
            Nature? exportedNat = dummyField.Value;

            Warnings.Add(GetNaturePIDAlert(at, pku.Nature.Value));
            PID_DependencyDigest["Nature"] = exportedNat;

            //add to pid dep error if necessary
            if (PID_DependencyError is not null && exportedNat.HasValue && Nature_Field.Nature.AsT1.Value != exportedNat)
            {
                var x = PID_DependencyError.Choices;
                x[0].Message = x[0].Message.AddNewLine($"Nature: {Nature_Field.Nature.AsT1.Value.ToFormattedString()}");
                x[1].Message = x[1].Message.AddNewLine($"Nature: {exportedNat.ToFormattedString()}");
            }
        }
        else //pid independent
        {
            AlertType at = ExportEnumTag(pku.Nature, Nature_Field.Nature_Default, Nature_Field.Nature);
            Warnings.Add(GetNatureAlert(at, pku.Nature.Value, Nature_Field.Nature_Default));
        }
    }

    public Alert GetNatureAlert(AlertType at, string val, OneOf<Nature, string> defaultVal)
        => GetNatureAlertBase(at, val, defaultVal);

    public Alert GetNaturePIDAlert(AlertType at, string val)
        => GetNatureAlertBase(at, val, "using the nature decided by the PID.");

    public Alert GetNatureAlertBase(AlertType at, string val, OneOf<Nature, string> defaultVal)
        => GetEnumAlert("Nature", at, val, defaultVal);
}