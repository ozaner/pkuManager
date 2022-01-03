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
    public OneOf<IntegralField, Field<Gender>, Field<Gender?>, Field<bool>> OT_Gender { get; }
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
        IntegralField boolWrapper = null;
        if (Data.OT_Gender.TryPickT3(out Field<bool> boolField, out OneOf<IntegralField, Field<Gender>, Field<Gender?>> field))
            field = boolWrapper = new BackedIntegralField(1, 0);

        ProcessEnumTag("OT Gender", pku.Game_Info.Gender, OT_Gender_Default, field, OT_Gender_AlertIfUnspecified,
            OT_Gender_Alert_Func, x => x is Gender.Male or Gender.Female);

        if (boolWrapper is not null)
            Data.OT_Gender.AsT3.Set(boolWrapper.Get() > 0);
    }
}