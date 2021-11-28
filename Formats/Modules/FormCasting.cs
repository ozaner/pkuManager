using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface FormCasting_E
{
    public pkuObject pku { get; }
    public GlobalFlags GlobalFlags { get; }
    public string FormatName { get; }
    public List<Alert> Notes { get; }

    // Alert strings
    private const string AlertTitle = "Form Casting";
    private static string CastingMsg(string originalForm, string castedForm)
        => $"The pku's form \'{originalForm}\' doesn't exist in this format, but a similar form \'{castedForm}\' does. Using that instead.";
    private static string DefaultMsg(string originalForm)
        => $"The pku's form \'{originalForm}\' doesn't exist in this format but, because \'Default Form Override\' is active, casting to the default form.";

    [PorterDirective(ProcessingPhase.PreProcessing)]
    public void ProcessFormCasting()
    {
        string form = pku.FirstFormInFormat(FormatName, true, GlobalFlags.Default_Form_Override);
        if (form is null)
            throw new("FormCasting module should not have been run if this pku has no existant forms in this format.");

        string originalForm = pku.GetSearchableForm();
        if (form.EqualsCaseInsensitive(originalForm)) //no need to cast
            return;

        //using a casted/default form
        Alert a;
        var castableForms = pku.GetCastableForms();
        string defaultForm = DexUtil.GetDefaultForm(pku.Species);
        if (castableForms.Any(x => x.EqualsCaseInsensitive(form)))
            a = new(AlertTitle, CastingMsg(originalForm.SplitLexical().ToFormattedString(),
                                           form.SplitLexical().ToFormattedString()));
        else if (GlobalFlags.Default_Form_Override && form.EqualsCaseInsensitive(defaultForm))
            a = new(AlertTitle, DefaultMsg(originalForm.SplitLexical().ToFormattedString()));
        else //this is impossible
            throw new("There's something wrong with the this method or FirstFormInFormat if we got here...");

        pku.Forms.Set(form.SplitLexical()); //cast form
        Notes.Add(a);
    }
}