using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface OT_Gender_O
{
    public OneOf<IIntField, IField<Gender>> OT_Gender { get; }
}

public interface OT_Gender_E : Tag
{
    private static readonly Dictionary<Gender, int> MF_ONLY_DICT = new() { { Gender.Male, 0 }, { Gender.Female, 1 }, };

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportOT_Gender()
    {
        AlertType at = EnumTagUtil<Gender>.ExportEnumTag(pku.Game_Info.Gender,
            (Data as OT_Gender_O).OT_Gender, DEFAULT_GENDER, MF_ONLY_DICT);
        if (at is not AlertType.UNSPECIFIED) //silent OT gender unspecified alert
            Warnings.Add(GetOT_GenderAlert(at, pku.Game_Info.Gender.Value, DEFAULT_GENDER));
    }

    public Alert GetOT_GenderAlert(AlertType at, string val, OneOf<Gender, string> defaultVal)
        => EnumTagUtil<Gender>.GetEnumAlert("OT Gender", at, val, defaultVal);
}