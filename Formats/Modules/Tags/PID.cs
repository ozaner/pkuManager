using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface PID_O
{
    public IField<BigInteger> PID { get; }
    public bool PID_HasDependencies => false;
}

public interface PID_E : Tag
{
    public ChoiceAlert PID_DependencyError { get => null; set { } }
    public Dictionary<string, object> PID_DependencyDigest { get => null; set { } }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportPID()
    {
        PID_O pidObj = Data as PID_O;

        //check if in-bounds
        (BigInteger? max, BigInteger? min) = pidObj.PID is IBoundable<BigInteger> boundable ?
            (boundable.Max, boundable.Min) : (null, null);
        (pidObj.PID.Value, AlertType at) = pku.PID.Value switch
        {
            null => (0, AlertType.UNSPECIFIED),
            var x when x > max => (max.Value, AlertType.OVERFLOW),
            var x when x < min => (min.Value, AlertType.UNDERFLOW),
            _ => (pku.PID.Value.Value, AlertType.NONE)
        };

        if (pidObj.PID_HasDependencies)
        {
            PID_DependencyDigest = new();
            PID_DependencyDigest.Add("PID", at);
            if (at is AlertType.NONE) //create pid dep error, in case it's needed
            {
                ChoiceAlert alert = GetPIDDepAlert();
                PID_DependencyError = alert;
                Errors.Add(alert);
                return;
            }
        }

        //not a potential mismatch
        Warnings.Add(GetPIDAlert(at, 0, max, min, pidObj.PID_HasDependencies));
    }

    [PorterDirective(ProcessingPhase.FirstPassStage2)]
    public void CleanupPIDDepError()
    {
        PID_O pidObj = Data as PID_O;

        if (pidObj.PID_HasDependencies)
        {
            bool pidOutOfBounds = PID_DependencyDigest["PID"] is not AlertType.NONE;
            bool pidMismatch = PID_DependencyError is not null
                            && PID_DependencyError.Choices[0].Message != ""
                            && PID_DependencyError.Choices[1].Message != "";

            //generate new PID using digest
            PID_DependencyDigest.TryGetValue("Shiny", out object tempA);
            (bool, uint)? shinyDigest = ((bool, uint)?)tempA;
            PID_DependencyDigest.TryGetValue("Gender", out object tempB);
            (Gender, GenderRatio)? genderDigest = ((Gender, GenderRatio)?)tempB;
            PID_DependencyDigest.TryGetValue("Nature", out object tempC);
            Nature? nature = (Nature?)tempC;
            PID_DependencyDigest.TryGetValue("Unown Form", out object tempD);
            int? unownForm = (int?)tempD;

            uint newPID = TagUtil.GenerateRandomPID(shinyDigest, genderDigest, nature, unownForm);

            if (pidOutOfBounds) //pid was out of bounds
                pidObj.PID.Value = newPID;
            else if (!pidMismatch) //pid matches
                Errors.Remove(PID_DependencyError);
            else //pid doesn't match
                PID_Resolver = new(PID_DependencyError, pidObj.PID, new[] { pidObj.PID.Value, newPID });
        }
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger> PID_Resolver { get => null; set { } }

    protected static ChoiceAlert GetPIDDepAlert()
    {
        // PID-Mismatch Alert
        ChoiceAlert.SingleChoice[] choices =
        {
            new("Use original PID", ""),
            new("Generate new PID", "")
        };
        return new ChoiceAlert("PID-Mismatch", "This pku's PID is incompatible with some of its other " +
            "tags (in this format). Choose whether to keep the PID or generate a compatible one.", choices, true, 1);
    }

    protected static Alert GetPIDAlert(AlertType at, BigInteger defaultVal, BigInteger? max, BigInteger? min, bool hasDeps = false)
    {
        if (!hasDeps) //independent pid (silent unspecified)
            return at is AlertType.UNSPECIFIED ? null : NumericTagUtil.GetNumericAlert("PID", at, defaultVal, max, min);
        else //dependent pid
        {
            return at is AlertType.NONE ? null : new("PID", at switch
            {
                AlertType.UNSPECIFIED => "PID not specified",
                AlertType.OVERFLOW => "This pku's PID is higher than the maximum",
                AlertType.UNDERFLOW => "This pku's PID is lower than the minimum",
                _ => throw InvalidAlertType(at)
            } + ", generating one that matches this pku's other tags.");
        }
    }
}

public interface PID_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportPID()
        => NumericTagUtil.ImportNumericTag(pku.PID, (Data as PID_O).PID);
}