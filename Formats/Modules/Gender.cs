using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.Modules.Gender_Util;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Gender_O
{
    public OneOf<IField<BigInteger>, IField<Gender>, IField<Gender?>> Gender { get; }

    public Gender? Value
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

public static class Gender_Util
{
    /// <summary>
    /// A gender a Pokémon, or trainer, can have.<br/>
    /// Note that OT genders can only be male or female, not genderless.<br/>
    /// Index numbers correspond to those used in the official games.
    /// </summary>
    public enum Gender
    {
        Male,
        Female,
        Genderless
    }

    /// <summary>
    /// A gender ratio a Pokémon species can have.<br/>
    /// Index numbers correspond to the gender threshold use to determine a Pokémon's gender.
    /// </summary>
    public enum GenderRatio
    {
        All_Male = 0,
        Male_7_Female_1 = 31,
        Male_3_Female_1 = 63,
        Male_1_Female_1 = 127,
        Male_1_Female_3 = 191,
        Male_1_Female_7 = 225,
        All_Female = 254,
        All_Genderless = 255
    }

    /// <summary>
    /// The default gender for Pokémon and trainers used in pkx formats.
    /// </summary>
    public const Gender DEFAULT_GENDER = Gender.Male;

    /// <summary>
    /// Gets the gender ratio of the given official <paramref name="species"/>.
    /// </summary>
    /// <param name="species">An official species. Will throw an exception otherwise.</param>
    /// <returns>The gender ratio of <paramref name="species"/>.</returns>
    public static GenderRatio GetGenderRatio(DexUtil.SFA sfa)
        => SPECIES_DEX.ReadSpeciesDex<string>(sfa, "Gender Ratio").ToEnum<GenderRatio>().Value;

    /// <summary>
    /// Returns the gender of a Pokémon with the given <paramref name="pid"/> as determined by Gens 3-5. 
    /// </summary>
    /// <param name="gr">The gender ratio of the Pokémon.</param>
    /// <param name="pid">The PID of the Pokémon.</param>
    /// <returns>The gender of a Pokémon with the given gender ratio
    ///          and <paramref name="pid"/> in Gens 3-5.</returns>
    public static Gender GetPIDGender(GenderRatio gr, uint pid) => gr switch
    {
        GenderRatio.All_Female => Gender.Female,
        GenderRatio.All_Male => Gender.Male,
        GenderRatio.All_Genderless => Gender.Genderless,
        GenderRatio x when (int)x > pid % 256 => Gender.Female,
        _ => Gender.Male
    };
}

public interface Gender_E
{
    public pkuObject pku { get; }
    List<Alert> Warnings { get; }

    public Gender_O Gender_Field { get; }
    public bool Gender_DisallowImpossibleGenders => false;
    public bool Gender_PIDDependent => false;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public virtual void ProcessGender()
        => ProcessGenderBase();

    public void ProcessGenderBase()
    {
        AlertType at = AlertType.NONE;
        Gender? readGender = pku.Gender.ToEnum<Gender>();
        GenderRatio gr = GetGenderRatio(pku);
        Gender? mandatoryGender = gr switch
        {
            GenderRatio.All_Genderless => Gender.Genderless,
            GenderRatio.All_Female => Gender.Female,
            GenderRatio.All_Male => Gender.Male,
            _ => null
        };
        Gender? defaultGender = Gender_Field.Gender.IsT2 ? null : (mandatoryGender ?? DEFAULT_GENDER);

        if (pku.Gender is null)
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
        Warnings.Add(GetGenderAlertBase(at, Gender_Field.Value, pku.Gender));
    }

    public Alert GetGenderAlert(AlertType at, Gender? defaultGender, string invalidGender)
        => GetGenderAlertBase(at, defaultGender, invalidGender);

    public virtual Alert GetGenderAlertBase(AlertType at, Gender? defaultGender, string invalidGender)
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