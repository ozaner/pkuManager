using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Nature_O
{
    public OneOf<IIntField, IField<Nature>> Nature { get; }
    public Nature Nature_Default => DEFAULT_NATURE;
    public bool Nature_PIDDependent => false;
}

public interface Nature_E : Tag
{
    public ChoiceAlert PID_DependencyError { get => null; set { } }
    public Dictionary<string, object> PID_DependencyDigest { get => null; set { } }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(PID_E.ExportPID))]
    public void ExportNature()
    {
        Nature_O natureObj = Data as Nature_O;
        if (natureObj.Nature_PIDDependent)
        {
            BackedField<Nature> dummyField = new();
            AlertType at = EnumTagUtil<Nature>.ExportEnumTag(pku.Nature, dummyField, natureObj.Nature_Default);
            Nature? exportedNat = dummyField.Value;

            Warnings.Add(GetNaturePIDAlert(at, pku.Nature.Value));
            PID_DependencyDigest["Nature"] = exportedNat;

            //add to pid dep error if necessary
            if (PID_DependencyError is not null && exportedNat.HasValue && natureObj.Nature.AsT1.Value != exportedNat)
            {
                var x = PID_DependencyError.Choices;
                x[0].Message = x[0].Message.AddNewLine($"Nature: {natureObj.Nature.AsT1.Value.ToFormattedString()}");
                x[1].Message = x[1].Message.AddNewLine($"Nature: {exportedNat.ToFormattedString()}");
            }
        }
        else //pid independent
        {
            AlertType at = EnumTagUtil<Nature>.ExportEnumTag(pku.Nature, natureObj.Nature, natureObj.Nature_Default);
            Warnings.Add(GetNatureAlert(at, pku.Nature.Value, natureObj.Nature_Default));
        }
    }

    public Alert GetNatureAlert(AlertType at, string val, OneOf<Nature, string> defaultVal)
        => GetNatureAlertBase(at, val, defaultVal);

    public Alert GetNaturePIDAlert(AlertType at, string val)
        => GetNatureAlertBase(at, val, "using the nature decided by the PID.");

    public Alert GetNatureAlertBase(AlertType at, string val, OneOf<Nature, string> defaultVal)
        => EnumTagUtil<Nature>.GetEnumAlert("Nature", at, val, defaultVal);
}

public interface Nature_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportNature()
    {
        var natureField = (Data as Nature_O).Nature;
        AlertType at = EnumTagUtil<Nature>.ImportEnumTag(pku.Nature, natureField);
        if (at is AlertType.INVALID)
            ByteOverrideUtil.TryAddByteOverrideCMD("Nature", natureField.Value as IByteOverridable, pku, FormatName);
    }
}