using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Experience_O
{
    public IField<BigInteger> Experience { get; }
}

public interface Experience_E
{
    public pkuObject pku { get; }
    List<Alert> Warnings { get; }
    List<Alert> Errors { get; }

    public Experience_O Experience_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessExperience()
    {
        int dex = TagUtil.GetNationalDexChecked(pku.Species); //must be valid at this point
        int level100EXP = PokeAPIUtil.GetEXPFromLevel(dex, 100);

        //data collection - Level
        (BigInteger? levelChecked, BigInteger levelAsEXP, AlertType atLevel) = pku.Level.Value switch
        {
            null => (null, -1, AlertType.NONE), //no level specified
            var x when x.Value > 100 => (100, level100EXP, AlertType.OVERFLOW), //level overflow
            var x when x.Value < 1 => (1, 0, AlertType.UNDERFLOW), //level underflow
            _ => ((int?)pku.Level.Value, PokeAPIUtil.GetEXPFromLevel(dex, (int)pku.Level.Value), AlertType.NONE) //valid level
        };

        //data collection - EXP
        (BigInteger? EXPChecked, BigInteger EXPAsLevel, AlertType atEXP) = pku.Experience.Value switch
        {
            null => (null, -1, AlertType.NONE), //no exp specified
            var x when x > level100EXP => (level100EXP, 100, AlertType.OVERFLOW), //exp overflow
            var x when x < 0 => (0, 1, AlertType.UNDERFLOW), //exp underflow
            _ => (pku.Experience.Value, PokeAPIUtil.GetLevelFromEXP(dex, (int)pku.Experience.Value.Value), AlertType.NONE) //valid exp
        };

        //Finalize values + alerts
        (BigInteger exp, BigInteger? expFromLevel, Alert alert) = (EXPChecked, levelChecked) switch
        {
            (null, null) => (0, null, GetLevelExperienceAlert(AlertType.UNSPECIFIED)), //Both Unspecified
            (null, not null) => (levelAsEXP, null, GetLevelAlert(atLevel)), //Only Level Specified
            (not null, null) => (EXPChecked.Value, null, GetExperienceAlert(atEXP, level100EXP)), //Only EXP Specified
            _ => (atEXP, atLevel) switch //Both Specified
            {
                //Both Overflow
                (AlertType.OVERFLOW, AlertType.OVERFLOW) => (level100EXP, null, GetLevelExperienceAlert(AlertType.OVERFLOW)),
                //Both Underflow
                (AlertType.UNDERFLOW, AlertType.UNDERFLOW) => (0, null, GetLevelExperienceAlert(AlertType.UNDERFLOW)),
                //Both in agreement
                var (x, y) when (x, y) is (AlertType.NONE, AlertType.NONE) && EXPAsLevel == levelChecked => (EXPChecked.Value, null, null),
                //Mismatch
                _ => (EXPChecked.Value, (int?)levelAsEXP, GetLevelExperienceAlert(AlertType.MISMATCH, (atLevel, atEXP,
                    levelChecked.Value, EXPChecked.Value, levelAsEXP, EXPAsLevel, level100EXP))),
            }
        };

        BigInteger[] options = expFromLevel is null ? new BigInteger[] { exp }
                                                    : new BigInteger[] { exp, expFromLevel.Value };
        Experience_Resolver = new(alert, Experience_Field.Experience, options);
        if (alert is RadioButtonAlert)
            Errors.Add(alert);
        else
            Warnings.Add(alert);
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger> Experience_Resolver { get; set; }


    public static Alert GetLevelAlert(AlertType at)
        => NumericTag_E.GetNumericalAlert("Level", at, 100, 1, 1, true);

    public static Alert GetExperienceAlert(AlertType at, int? level100exp = null)
    {
        if (at is AlertType.OVERFLOW && level100exp is null)
            throw new ArgumentNullException(nameof(level100exp), "The level 100 exp must be given for OVERFLOW alerts.");

        Alert a = NumericTag_E.GetNumericalAlert("Exp", at, level100exp, 0, 0, true);
        if (at is not AlertType.NONE)
        {
            a.Message = a.Message[..^1] + " (i.e. Level " + at switch
            {
                AlertType.UNSPECIFIED or AlertType.UNDERFLOW => "1",
                AlertType.OVERFLOW => "100",
                _ => throw InvalidAlertType(at)
            } + ").";
        }
        return a;
    }

    public static Alert GetLevelExperienceAlert(AlertType at, (AlertType atLevel, AlertType atEXP,
        BigInteger level, BigInteger exp, BigInteger levelToExp, BigInteger expToLevel, int level100Exp)? mismatchData = null)
    {
        //mismatch alert
        if (at is AlertType.MISMATCH)
        {
            //levelData and expData must be given for MISMATCH alert
            if (mismatchData is null)
                throw new ArgumentNullException(nameof(mismatchData), $"{nameof(mismatchData)} must be given for MISMATCH alerts.");

            RadioButtonAlert.RBAChoice[] choices = new RadioButtonAlert.RBAChoice[2];

            // Deal with phrasing the level option
            choices[0] = new("Use Level Tag", mismatchData.Value.atLevel switch
            {
                AlertType.OVERFLOW => $"Set experience to {mismatchData.Value.level100Exp}, i.e. level 100 (rounded down because level tag was too high).",
                AlertType.UNDERFLOW => "Set experience to 0, i.e. level 1 (rounded up because level tag was too low).",
                AlertType.NONE => $"Set experience to {mismatchData.Value.levelToExp}, i.e. level {mismatchData.Value.level}.",
                _ => throw new ArgumentException("No valid AlertTypes were given to the LEVEL part of the MISMATCH alert.", nameof(mismatchData.Value.atLevel))
            });

            // Deal with phrasing the exp option
            choices[1] = new("Use Experience Tag", mismatchData.Value.atEXP switch
            {
                AlertType.OVERFLOW => $"Set experience to {mismatchData.Value.level100Exp} experience, i.e. level 100 (rounded down because experience tag was too high).",
                AlertType.UNDERFLOW => "Set experience to 0, i.e. level 1 (rounded up because experience tag was too low).",
                AlertType.NONE => $"Set experience to {mismatchData.Value.exp}, i.e. level {mismatchData.Value.expToLevel}.",
                _ => throw new ArgumentException("No valid AlertTypes were given to the EXP part the MISMATCH alert.", nameof(mismatchData.Value.atEXP))
            });

            //put 2 options together
            return new RadioButtonAlert("Level/Experience", "The given level and experience don't match, choose which one to use.", choices);
        }

        //other alerts
        return new("Level/Experience", at switch
        {
            AlertType.UNSPECIFIED => "Neither the Level nor EXP was specified. Defaulting to level 1.",
            AlertType.OVERFLOW => "Both the Level and EXP tags are too high, rounding down to level 100.",
            AlertType.UNDERFLOW => "Both the Level and EXP tags are too low, rounding up to level 1.",
            _ => throw InvalidAlertType(at)
        });
    }
}