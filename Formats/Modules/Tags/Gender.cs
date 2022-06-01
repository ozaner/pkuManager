using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Fields.LambdaFields;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Gender_O
{
    public OneOf<IIntField, IField<Gender>, IField<Gender?>> Gender { get; }
    public bool Gender_DisallowImpossibleGenders => false;
    public bool Gender_PIDDependent => false;

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

public interface Gender_E : Tag
{
    public ChoiceAlert PID_DependencyError { get => null; set { } }
    public Dictionary<string, object> PID_DependencyDigest { get => null; set { } }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(PID_E.ExportPID),
                                                nameof(SFA_E.ExportSFA))] //need for gender field
    public void ExportGender()
    {
        Gender_O Gender_Field = Data as Gender_O;

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
        Gender? defaultGender = Gender_Field.Gender_PIDDependent || Gender_Field.Gender.IsT2 ? null : mandatoryGender ?? DEFAULT_GENDER;
        Gender? exportedGender;

        if (pku.Gender.IsNull())
        {
            exportedGender = defaultGender;
            if (mandatoryGender is null)
                at = AlertType.UNSPECIFIED;
        }
        else
        {
            if (Gender_Field.Gender_DisallowImpossibleGenders &&
                mandatoryGender is not null &&
                readGender is not null && mandatoryGender != readGender)
            {
                exportedGender = mandatoryGender;
                at = AlertType.MISMATCH;
            }
            else
            {
                exportedGender = readGender ?? defaultGender;
                if (readGender is null)
                    at = AlertType.INVALID;
            }
        }


        if (Gender_Field.Gender_PIDDependent)
        {
            if (at is not AlertType.UNSPECIFIED) //when unspecified, pidDep gender alert is silent
                Warnings.Add(GetGenderPIDAlert(at, pku.Gender.Value));

            PID_DependencyDigest["Gender"] = exportedGender.HasValue ? (exportedGender.Value, gr) : null;

            //add to pid dep error if necessary
            if (PID_DependencyError is not null && exportedGender.HasValue && Gender_Field.Value != exportedGender)
            {
                var x = PID_DependencyError.Choices;
                x[0].Message = x[0].Message.AddNewLine($"Gender: {Gender_Field.Value.Value.ToFormattedString()}");
                x[1].Message = x[1].Message.AddNewLine($"Gender: {exportedGender.Value.ToFormattedString()}");
            }
        }
        else
        {
            Gender_Field.Value = exportedGender;
            Warnings.Add(GetGenderAlert(at, pku.Gender.Value, defaultGender));
        }
    }

    public Alert GetGenderAlert(AlertType at, string val, OneOf<Gender?, string> defaultVal)
        => GetGenderAlertBase(at, val, defaultVal);

    public Alert GetGenderPIDAlert(AlertType at, string val)
        => GetGenderAlertBase(at, val, "using the gender decided by the PID");

    public Alert GetGenderAlertBase(AlertType at, string val, OneOf<Gender?, string> defaultVal)
    {
        if (at is AlertType.NONE)
            return null;

        string msg = at switch
        {
            AlertType.MISMATCH => $"This species cannot be {val}, ",
            AlertType.INVALID => $"'{val}' is not a valid gender, ",
            AlertType.UNSPECIFIED => "No gender was specified, ",
            _ => throw InvalidAlertType(at)
        };
        msg += defaultVal.Match(
            x => $" using the default: {x?.ToFormattedString() ?? "none"}",
            x => x) + ".";
        return new("Gender", msg);
    }
}

public interface Gender_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportGender()
    {
        if((Data as Gender_O).Gender.TryPickT2(out var nullableField, out var usableGenderField))
        {
            if (nullableField.IsNull())
                return;
            else
                usableGenderField = new LambdaField<Gender>(() => nullableField.Value.Value, x => nullableField.Value = x);
        }
        AlertType at = EnumTagUtil<Gender>.ImportEnumTag(pku.Gender, usableGenderField);
        if (at is AlertType.INVALID)
            ByteOverrideUtil.TryAddByteOverrideCMD("Gender", usableGenderField.Value as IByteOverridable, pku, FormatName);
    }
}