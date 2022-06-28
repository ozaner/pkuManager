using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;
using System.Linq;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.MetaTags;

public interface FormCasting_E : Tag
{
    [PorterDirective(ProcessingPhase.PreProcessing)]
    public void ApplyFormCasting()
    {
        string form = DexUtil.FirstFormInFormat(pku, FormatName, true, GlobalFlags.Default_Form_Override);
        if (form is null)
            throw new("FormCasting module should not have been run if this pku has no existant forms in this format.");

        string originalForm = ((DexUtil.SFA)pku).Form;
        if (form == originalForm) //no need to cast
            return;

        //using a casted/default form
        bool usingDefaultOverride = false;
        var castableForms = DexUtil.GetCastableForms(pku);
        string defaultForm = DexUtil.GetDefaultForm(pku.Species.Value);
        if (castableForms.Any(x => x == form))
            usingDefaultOverride = false;
        else if (GlobalFlags.Default_Form_Override && form == defaultForm)
            usingDefaultOverride = true;
        else //this is impossible
            throw new("There's something wrong with the this method or FirstFormInFormat if we got here...");

        pku.Forms.Value = form.SplitLexical(); //cast form
        Notes.Add(GetFormCastingAlert(usingDefaultOverride, originalForm.SplitLexical().ToFormattedString(),
                form.SplitLexical().ToFormattedString()));
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