using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface PID_O
{
    public IField<BigInteger> PID { get; }
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
    [PorterDirective(ProcessingPhase.FirstPass, nameof(TID_E.ExportTID), nameof(Gender_E.ExportGender),
                                                nameof(Form_E.ExportForm), nameof(Nature_E.ExportNature))]
    public void ExportPID()
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
                x => TagUtil.GetUnownFormIDFromName(x.Value)) : null;
        }

        // Deal with null PID
        (uint pid, bool pidInBounds, AlertType at) = pku.PID.Value switch
        {
            null => ((uint)0, false, AlertType.UNSPECIFIED),
            var x when x > uint.MaxValue => (uint.MaxValue, false, AlertType.OVERFLOW),
            var x when x < uint.MinValue => ((uint)0, false, AlertType.UNDERFLOW),
            _ => ((uint)pku.PID.Value, true, AlertType.NONE)
        };
        Alert alert = GetPIDAlert(at);

        // Check if any value has a pid-mismatch
        bool genderMismatch = false, natureMismatch = false, unownMismatch = false, shinyMismatch;
        int oldunownform = 0;
        Nature oldnature = DEFAULT_NATURE;
        Gender oldgender = DEFAULT_GENDER;

        if (checkedGender is not null) //gender mismatch check
        {
            oldgender = TagUtil.GetPIDGender(TagUtil.GetGenderRatio(pku), pid);
            genderMismatch = checkedGender != oldgender;
        }
        if (checkedNature is not null) //nature mismatch check
        {
            oldnature = (Nature)(pid % 25);
            natureMismatch = checkedNature != oldnature;
        }
        if (checkedUnownForm is not null) //unown form mismatch check
        {
            oldunownform = TagUtil.GetUnownFormIDFromPID(pid);
            unownMismatch = checkedUnownForm is not null && checkedUnownForm != oldunownform;
        }
        //always check shiny
        bool oldshiny = TagUtil.IsPIDShiny(pid, checkedTID, PID_Gen6ShinyOdds);
        shinyMismatch = pku.IsShiny() != oldshiny;

        // Deal with pid-mismatches
        BigInteger[] pids;
        if (unownMismatch || genderMismatch || natureMismatch || shinyMismatch)
        {
            uint newPID = TagUtil.GenerateRandomPID(pku.IsShiny(), checkedTID, checkedGender,
                TagUtil.GetGenderRatio(pku), checkedNature, checkedUnownForm);

            if (pidInBounds) //two options: old & new, need error
            {
                List<(string, object, object)> tags = new();
                if (unownMismatch)
                    tags.Add(("Unown Form", TagUtil.GetUnownFormName(oldunownform),
                        TagUtil.GetUnownFormName(checkedUnownForm.Value)));
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

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger> PID_Resolver { get; set; }

    protected static Alert GetPIDAlert(AlertType at, List<(string, object, object)> tags = null)
    {
        if (at is AlertType.NONE)
            return null;

        // PID-Mismatch Alert
        if (at is AlertType.MISMATCH)
        {
            if (tags?.Count is not > 0)
                throw new ArgumentException($"If {nameof(at)} is MISMATCH, {nameof(tags)} must be non-empty.", nameof(tags));
            string choice1msg = "";
            string choice2msg = "";
            foreach ((string name, object a, object b) in tags)
            {
                choice1msg = choice1msg.AddNewLine($"{name}: {a}");
                choice2msg = choice2msg.AddNewLine($"{name}: {b}");
            }
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