using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Egg_Steps_O
{
    public IIntField Egg_Steps { get; }
    public bool Egg_Steps_StoredInFriendship => false;
    public bool Egg_Steps_ImplicitIs_Egg => false;
}

public interface Egg_Steps_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportEgg_Steps()
        => ExportEgg_StepsBase();

    public void ExportEgg_StepsBase()
    {
        var eggStepsObj = Data as Egg_Steps_O;

        //if egg steps field is stored in friendship, dont process it unless pku is egg
        if (eggStepsObj.Egg_Steps_StoredInFriendship && !pku.IsEgg())
            return;

        //if Is_Egg depends on egg steps, handle a mismatch
        if (eggStepsObj.Egg_Steps_ImplicitIs_Egg)
        {
            bool validEgg = pku.IsEgg() && pku.Egg_Info.Steps_to_Hatch.Value > 0;
            bool validNonEgg = !(pku.IsEgg() || pku.Egg_Info.Steps_to_Hatch.Value > 0);

            if (!validEgg && !validNonEgg)
            {
                eggStepsObj.Egg_Steps.Value = pku.IsEgg() ? 1 : 0;
                Warnings.Add(GetEgg_StepMismatchAlert(pku.IsEgg()));
                return;
            }
        }

        //normal case
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Egg_Info.Steps_to_Hatch, eggStepsObj.Egg_Steps, 0);
        if (at is not AlertType.UNSPECIFIED) //ignore unspecified
            Warnings.Add(NumericTagUtil.GetNumericAlert("Steps to Hatch", at, 0, eggStepsObj.Egg_Steps));
    }

    public Alert GetEgg_StepMismatchAlert(bool isEgg)
        => new("Steps to Hatch", isEgg ? "Eggs can't have less than 1 step left to hatch, setting steps to 1."
                                       : "Non-eggs must have 0 steps left to hatch. Setting steps to 0.");
}