using OneOf;
using pkuManager.Data;
using pkuManager.Data.Dexes;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Utilities;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Species_O
{
    public OneOf<IIntField, IField<string>> Species { get; }
}

public interface Form_O
{
    public OneOf<IIntField, IField<string>> Form { get; }
}

public interface SFA_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportSFA() => ExportSFABase();
    
    public void ExportSFABase()
    {
        //Set Species field
        (Data as Species_O).Species.Switch(
            x => {
                DDM.TryGetSpeciesID(pku, FormatName, out int ID);
                x.SetAs(ID);
            }, //int field
            x => {
                DDM.TryGetSpeciesID(pku, FormatName, out string ID);
                x.Value = ID;
            } //string field
        );

        //Set Form field (if it exists)
        if (Data is Form_O formObj)
        {
            formObj.Form.Switch(
                x => {
                    DDM.TryGetFormID(pku, FormatName, out int ID);
                    x.SetAs(ID);
                }, //int field
                x => {
                    DDM.TryGetFormID(pku, FormatName, out string ID);
                    x.Value = ID;
                } //string field
            );
        }

        Warnings.Add(GetSFAMAlert(pku)); //run alert
    }

    public Alert GetSFAMAlert(SFAM sfam) => null;
}

public interface SFA_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportSFA()
    {
        bool isString = (Data as Species_O).Species.IsT1;
        SFAM sfam;
        if (isString) //string indices
        {
            string speciesID = (Data as Species_O).Species.AsT1.Value;
            string formID = Data is Form_O formObj ? formObj.Form.AsT1.Value : null;
            DDM.TryGetSFAMFromIDs(out sfam, FormatName, speciesID, formID, null, Data as IModifiers);
        }
        else //int indices
        {
            int speciesID = (Data as Species_O).Species.AsT0.GetAs<int>();
            int? formID = Data is Form_O formObj ? formObj.Form.AsT0.GetAs<int>() : null;
            DDM.TryGetSFAMFromIDs(out sfam, FormatName, speciesID, formID, null, Data as IModifiers);
        }

        pku.Species.Value = sfam.Species;
        pku.Forms.Value = sfam.Form is "" ? null : sfam.Form.SplitLexical();
        pku.Appearance.Value = sfam.Appearance is "" ? null : sfam.Appearance.SplitLexical();
    }
}