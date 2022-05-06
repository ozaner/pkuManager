using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Species_O
{
    public OneOf<IField<BigInteger>, IField<string>> Species { get; }
}

public interface Form_O
{
    public OneOf<IField<BigInteger>, IField<string>> Form { get; }
}

public interface SFA_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportSFA() => ExportSFABase();
    
    public void ExportSFABase()
    {
        //Set Species field
        (Data as Species_O).Species.Switch(
            x => x.SetAs(DexUtil.GetSpeciesIndexedValue<int?>(pku, FormatName, "Indices").Value), //int field
            x => x.Value = DexUtil.GetSpeciesIndexedValue<string>(pku, FormatName, "Indices") //string field
        );

        //Set Form field (if it exists)
        if (Data is Form_O formObj)
        {
            formObj.Form.Switch(
                x => x.SetAs(DexUtil.GetSpeciesIndexedValue<int?>(pku, FormatName, "Form Indices").Value), //int field
                x => x.Value = DexUtil.GetSpeciesIndexedValue<string>(pku, FormatName, "Form Indices") //string field
            );
        }

        Warnings.Add(GetSFAAlert(pku)); //run alert
    }

    public Alert GetSFAAlert(DexUtil.SFA sfa) => null;
}

public interface SFA_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportSFA()
    {
        bool isString = (Data as Species_O).Species.IsT1;
        bool isFemale = (Data as Gender_O)?.Value is Gender.Female;
        DexUtil.SFA sfa;
        if (isString) //string indices
        {
            string speciesID = (Data as Species_O).Species.AsT1.Value;
            string formID = Data is Form_O formObj ? formObj.Form.AsT1.Value : null;
            sfa = DexUtil.GetSFAFromIndices(FormatName, speciesID, formID, isFemale);
        }
        else //int indices
        {
            int? speciesID = (Data as Species_O).Species.AsT0.GetAs<int>();
            int? formID = Data is Form_O formObj ? formObj.Form.AsT0.GetAs<int>() : null;
            sfa = DexUtil.GetSFAFromIndices(FormatName, speciesID, formID, isFemale);
        }

        pku.Species.Value = sfa.Species;
        pku.Forms.Value = sfa.Form is "" ? null : sfa.Form.SplitLexical();
        pku.Appearance.Value = sfa.Appearance is "" ? null : sfa.Appearance.SplitLexical();
    }
}