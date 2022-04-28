using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface OT_Gender_O
{
    public OneOf<IField<BigInteger>, IField<Gender>> OT_Gender { get; }
}

public interface OT_Gender_E : EnumTag_E
{
    public OT_Gender_O OT_Gender_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportOT_Gender()
    {
        AlertType at = ExportEnumTag(pku.Game_Info.Gender, DEFAULT_GENDER, OT_Gender_Field.OT_Gender,
            x => x is Gender.Male or Gender.Female);
        if (at is not AlertType.UNSPECIFIED) //silent OT gender unspecified alert
            Warnings.Add(GetOT_GenderAlert(at, pku.Game_Info.Gender.Value, DEFAULT_GENDER));
    }

    public Alert GetOT_GenderAlert(AlertType at, string val, OneOf<Gender, string> defaultVal)
        => GetOT_GenderAlertBase(at, val, defaultVal);

    public Alert GetOT_GenderAlertBase(AlertType at, string val, OneOf<Gender, string> defaultVal)
        => GetEnumAlert("OT Gender", at, val, defaultVal);
}