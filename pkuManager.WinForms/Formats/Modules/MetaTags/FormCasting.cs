using pkuManager.Data;
using pkuManager.Data.Dexes;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.MetaTags;

public static class FormCastingUtil
{
    public enum FormCastStatus
    {
        DNE,
        No_Cast_Needed,
        Casted,
        Casted_to_Default
    }

    public static (FormCastStatus fcs, string form) GetCastedForm(SFAM sfam, string format, bool allowDefault)
    {
        if (DDM.SFAMExists(sfam, format)) //form exists, no need to cast
            return (FormCastStatus.No_Cast_Needed, sfam.Form);

        //try castable forms
        SFAM testSFAM = new(sfam.Species, sfam.Form, sfam.Appearance, sfam.Modifiers);
        foreach (var form in DDM.GetCastableForms(sfam))
        {
            testSFAM.Form = form;
            if (DDM.SFAMExists(testSFAM, format))
                return (FormCastStatus.Casted, form);
        }

        //try default form if allowed
        if (allowDefault)
        {
            testSFAM.Form = DDM.GetDefaultForm(sfam.Species);
            if (DDM.SFAMExists(testSFAM, format))
                return (FormCastStatus.Casted_to_Default, testSFAM.Form);
        }

        return (FormCastStatus.DNE, null); //no form exists
    }
}

public interface FormCasting_E : Tag
{
    [PorterDirective(ProcessingPhase.PreProcessing)]
    public void ApplyFormCasting()
    {
        (var fcs, string form) = FormCastingUtil.GetCastedForm(pku, FormatName, GlobalFlags.Default_Form_Override);
        
        if (fcs is FormCastingUtil.FormCastStatus.DNE) //no form valid (shouldn't have gotten here then)
            throw new("FormCasting module should not have been run if this pku has no existant forms in this format.");

        if (fcs is not FormCastingUtil.FormCastStatus.No_Cast_Needed) //cast occured
        {
            Notes.Add(GetFormCastingAlert(fcs is FormCastingUtil.FormCastStatus.Casted_to_Default,
                pku.Forms.Value.ToFormattedString(), form.SplitLexical().ToFormattedString()));

            pku.Forms.Value = form.SplitLexical(); //cast form
        }
    }

    protected static Alert GetFormCastingAlert(bool usingDefaultOverride, string originalForm, string castedForm)
    {
        string msg = $"The pku's form \'{originalForm}\' doesn't exist in this format, but ";
        if (usingDefaultOverride)
            msg += $"because \'Default Form Override\' is active, casting to the default form \'{castedForm}\'.";
        else
            msg += $"a similar form \'{castedForm}\' does. Using that instead.";
        return new("Form Casting", msg);
    }
}