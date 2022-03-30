using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Gender_O
{
    public OneOf<IField<BigInteger>, IField<Gender>, IField<Gender?>> Gender { get; }

    public sealed Gender? Value
    {
        get => Gender.Match(
            x => x.Value.ToEnum<Gender>(),
            x => x.Value,
            x => x.Value
        );
        set => Gender.Switch(
            x => x.Value = (int)value,
            x => x.Value = value ?? DEFAULT_GENDER,
            x => x.Value = value);
    }
}

public interface Gender_E
{
    public pkuObject pku { get; }
    List<Alert> Warnings { get; }

    public Gender_O Gender_Field { get; }
    public bool Gender_DisallowImpossibleGenders => false;
    public bool Gender_PIDDependent => false;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessGender() => ProcessGenderBase();

    public void ProcessGenderBase()
    {
        AlertType at = AlertType.NONE;
        Gender? readGender = pku.Gender.ToEnum<Gender>();
        GenderRatio gr = TagUtil.GetGenderRatio(pku);
        Gender? mandatoryGender = gr switch
        {
            GenderRatio.All_Genderless => Gender.Genderless,
            GenderRatio.All_Female => Gender.Female,
            GenderRatio.All_Male => Gender.Male,
            _ => null
        };
        Gender? defaultGender = Gender_Field.Gender.IsT2 ? null : mandatoryGender ?? DEFAULT_GENDER;

        if (pku.Gender.IsNull())
        {
            Gender_Field.Value = defaultGender;
            if (mandatoryGender is null)
                at = AlertType.UNSPECIFIED;
        }
        else
        {
            if (Gender_DisallowImpossibleGenders &&
                mandatoryGender is not null &&
                readGender is not null && mandatoryGender != readGender)
            {
                Gender_Field.Value = mandatoryGender;
                at = AlertType.MISMATCH;
            }
            else
            {
                Gender_Field.Value = readGender ?? defaultGender;
                if (readGender is null)
                    at = AlertType.INVALID;
            }
        }
        Warnings.Add(GetGenderAlert(at, Gender_Field.Value, pku.Gender.Value));
    }

    protected Alert GetGenderAlert(AlertType at, Gender? defaultGender, string invalidGender)
    {
        if (at is AlertType.NONE)
            return null;

        string msg = at switch
        {
            AlertType.MISMATCH => $"This species cannot be {invalidGender}.",
            AlertType.INVALID => $"'{invalidGender}' is not a valid gender.",
            AlertType.UNSPECIFIED => "No gender was specified.",
            _ => throw InvalidAlertType(at)
        };
        if (defaultGender is null && Gender_PIDDependent)
            msg += " Using the gender decided by the PID.";
        else
            msg += $" Setting gender to {(defaultGender.HasValue ? defaultGender : "nothing")}.";
        return new("Gender", msg);
    }
}