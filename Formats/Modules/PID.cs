using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Formats.pkx.pk3;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.Modules.Gender_Util;
using static pkuManager.Formats.Modules.Nature_Util;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface PID_O
{
    public IField<BigInteger> PID { get; }
}

public static class PID_Util
{
    /// <summary>
    /// Generates a PID that satisfies the given constraints in all generations.<br/>
    /// If a constraint is null, it will be ignored.
    /// </summary>
    /// <param name="shiny">The desired shinyness.</param>
    /// <param name="tid">The Pokémon's TID.</param>
    /// <param name="gender">The desired gender.</param>
    /// <param name="gr">The species' gender ratio.</param>
    /// <param name="nature">The desired nature.</param>
    /// <param name="unownForm">The desired form, if the species is Unown.</param>
    /// <returns>A random PID satisfying all the given constraints.</returns>
    public static uint GenerateRandomPID(bool shiny, uint tid, Gender? gender = null,
        GenderRatio? gr = null, Nature? nature = null, int? unownForm = null)
    {
        //Notice no option for ability slot.
        //Getting legality is the user's problem. Preserving legality is pku's problem.
        while (true)
        {
            uint pid = DataUtil.GetRandomUInt(); //Generate new PID candidate

            // Unown Form Check
            if (unownForm is not null)
            {
                if (unownForm != pk3Object.GetUnownFormIDFromPID(pid))
                    continue;
            }

            // Gender Check
            if (gender is not null && gr is null)
                throw new ArgumentException($"If {nameof(gender)} is specified, a gender ratio must also be specified.", nameof(gr));
            else if (gender is not null) //gr is not null
            {
                if (gr is not (GenderRatio.All_Male or GenderRatio.All_Female or GenderRatio.All_Genderless))
                {
                    Gender pidGender = GetPIDGender(gr.Value, pid);

                    //Male but pid is Female
                    if ((gender, pidGender) is (Gender.Male, not Gender.Male))
                        continue;

                    //Female but pid is Male
                    if ((gender, pidGender) is (Gender.Female, not Gender.Female))
                        continue;
                }
            }

            // Nature Check
            if (nature is not null)
            {
                if ((int)nature != pid % 25)
                    continue;
            }

            // Shiny Check
            if ((pid / 65536 ^ pid % 65536 ^ tid / 65536 ^ tid % 65536) < 8 != shiny) //In gen 6+ that 8->16.
                continue;                                                             //No harm keeping it 8 for backwards compat.

            return pid; // everything checks out
        }
    }

    public static bool IsPIDShiny(uint pid, uint tid, bool gen6Plus)
        => (tid / 65536 ^ tid % 65536 ^ pid / 65536 ^ pid % 65536) < (gen6Plus ? 16 : 8);
}

public interface PID_E
{
    public pkuObject pku { get; }
    List<Alert> Warnings { get; }
    List<Alert> Errors { get; }

    public PID_O PID_Field { get; }
    public TID_O TID_Field { get; } //deals with shiny
    public Species_O Species_Field { get; }
    public Form_O Form_Field { get; } //unown form
    public Gender_O Gender_Field { get; }
    public Nature_O Nature_Field { get; }

    public bool PID_Gen6ShinyOdds => true;

    public bool PID_GenderDependent => false;
    public bool PID_UnownFormDependent => false;
    public bool PID_NatureDependent => false;

    // PID [Requires: Gender, Form, Nature, TID] [ErrorResolver]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(TID_E.ProcessTID), nameof(Gender_E.ProcessGender),
                                                nameof(Form_E.ProcessForm), nameof(Nature_E.ProcessNature))]
    public virtual void ProcessPID()
        => ProcessPIDBase();

    // PID ErrorResolver
    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger> PID_Resolver { get; set; }


    public void ProcessPIDBase()
    {
        uint checkedTID = TID_Field.TID.GetAs<uint>();
        Gender? checkedGender = PID_GenderDependent ? Gender_Field.Value : null;
        Nature? checkedNature = PID_NatureDependent ? Nature_Field.Value : null;
        int? checkedUnownForm = null;
        if (PID_UnownFormDependent)
        {
            bool isUnown = Species_Field.Species.Match(
                x => x.Value == 201,
                x => x.Value == "Unown");
            checkedUnownForm = isUnown ? Form_Field.Form.Match(
                x => x.GetAs<int>(),
                x => pk3Object.GetUnownFormIDFromName(x.Value)) : null;
        }

        // Deal with null PID. (Always in-bounds since it is a uint)
        (uint pid, bool pidInBounds, Alert alert) = pku.PID is null ?
            (uint.MinValue, false, GetPIDAlert(AlertType.UNSPECIFIED)) : (pku.PID.Value, true, null);

        // Check if any value has a pid-mismatch
        bool genderMismatch = false, natureMismatch = false, unownMismatch = false, shinyMismatch;
        int oldunownform = 0;
        Nature oldnature = DEFAULT_NATURE;
        Gender oldgender = DEFAULT_GENDER;

        if (checkedGender is not null) //gender mismatch check
        {
            oldgender = GetPIDGender(GetGenderRatio(pku), pid);
            genderMismatch = checkedGender != oldgender;
        }
        if (checkedNature is not null) //nature mismatch check
        {
            oldnature = (Nature)(pid % 25);
            natureMismatch = checkedNature != oldnature;
        }
        if (checkedUnownForm is not null) //unown form mismatch check
        {
            oldunownform = pk3Object.GetUnownFormIDFromPID(pid);
            unownMismatch = checkedUnownForm is not null && checkedUnownForm != oldunownform;
        }
        //always check shiny
        bool oldshiny = PID_Util.IsPIDShiny(pid, checkedTID, PID_Gen6ShinyOdds);
        shinyMismatch = pku.IsShiny() != oldshiny;

        // Deal with pid-mismatches
        BigInteger[] pids;
        if (unownMismatch || genderMismatch || natureMismatch || shinyMismatch)
        {
            uint newPID = PID_Util.GenerateRandomPID(pku.IsShiny(), checkedTID, checkedGender,
                GetGenderRatio(pku), checkedNature, checkedUnownForm);

            if (pidInBounds) //two options: old & new, need error
            {
                List<(string, object, object)> tags = new();
                if (unownMismatch)
                    tags.Add(("Unown Form", pk3Object.GetUnownFormName(oldunownform),
                        pk3Object.GetUnownFormName(checkedUnownForm.Value)));
                if (genderMismatch)
                    tags.Add(("Gender", oldgender, checkedGender));
                if (natureMismatch)
                    tags.Add(("Nature", oldnature, checkedNature));
                if (shinyMismatch)
                    tags.Add(("Shiny", oldshiny, pku.Shiny));
                alert = GetPIDAlert(AlertType.MISMATCH, tags); //RadioButtonAlert
                pids = new BigInteger[] { pid, newPID }; //error: pid mismatched, choose old or new.
            }
            else
                pids = new BigInteger[] { newPID }; //warning: pid out of bounds, generating new one that deals with mismatches.
        }
        else
            pids = new BigInteger[] { pid }; //either:
                                             //   warning: pid unspecified or out of bounds, rounding it.
                                             //no warning: pid is in bounds w/ no mismatches.

        //set values
        PID_Resolver = new(alert, PID_Field.PID, pids);
        if (alert is RadioButtonAlert)
            Errors.Add(alert);
        else
            Warnings.Add(alert);
    }

    public static Alert GetPIDAlert(AlertType at, List<(string, object, object)> tags = null)
    {
        // PID-Mismatch Alert
        if (at is AlertType.MISMATCH)
        {
            if (tags?.Count is not > 0)
                throw new ArgumentException($"If {nameof(at)} is MISMATCH, {nameof(tags)} must be non-empty.", nameof(tags));
            string choice1msg = "";
            string choice2msg = "";
            foreach ((string name, object a, object b) in tags)
            {
                choice1msg += $"{name}: {a}{DataUtil.Newline()}";
                choice2msg += $"{name}: {b}{DataUtil.Newline()}";
            }
            choice1msg = choice1msg[0..^1];
            choice2msg = choice2msg[0..^1];
            RadioButtonAlert.RBAChoice[] choices =
            {
                    new("Use original PID", choice1msg),
                    new("Generate new PID", choice2msg)
                };

            return new RadioButtonAlert("PID-Mismatch", "This pku's PID is incompatible with some of its other " +
                "tags (in this format). Choose whether to keep the PID or generate a compatible one.", choices);
        }

        // PID Alert
        return new("PID", at switch
        {
            AlertType.UNSPECIFIED => "PID not specified",
            AlertType.OVERFLOW => "This pku's PID is higher than the maximum",
            AlertType.UNDERFLOW => "This pku's PID is lower than the minimum",
            _ => throw InvalidAlertType(at)
        } + ", generating one that matches this pku's other tags.");
    }
}