using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.pku;
using System;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface OT_Gender_O
{
    public OneOf<IIntegralField, IField<Gender>, IField<Gender?>, IField<bool>> OT_Gender { get; }
}

public interface OT_Gender_E : EnumTag_E
{
    public pkuObject pku { get; }
    public OT_Gender_O Data { get; }

    public Gender? OT_Gender_Default => Gender.Male;
    public bool OT_Gender_AlertIfUnspecified => true;
    public Func<AlertType, string, string, Alert> OT_Gender_Alert_Func => null;

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessOT_Gender()
    {
        //converts bool field to integralfield
        if (Data.OT_Gender.TryPickT3(out IField<bool> boolField, out OneOf<IIntegralField, IField<Gender>, IField<Gender?>> field))
            field = new BackedIntegralField(1, 0);

        ProcessEnumTag("OT Gender", pku.Game_Info.Gender, OT_Gender_Default, field, OT_Gender_AlertIfUnspecified,
            OT_Gender_Alert_Func, x => x is Gender.Male or Gender.Female);

        //if original field was bool, convert back from integralfield
        if (Data.OT_Gender.IsT3)
            Data.OT_Gender.AsT3.Value = field.AsT0.Value > 0;
    }
}