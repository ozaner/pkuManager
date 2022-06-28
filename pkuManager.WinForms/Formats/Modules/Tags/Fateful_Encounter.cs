using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Fateful_Encounter_O
{
    public IField<bool> Fateful_Encounter { get; }
}

public interface Fateful_Encounter_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportFateful_Encounter() => ExportFateful_EncounterBase();

    public void ExportFateful_EncounterBase()
    {
        BooleanTagUtil.ExportBooleanTag(pku.Catch_Info.Fateful_Encounter,
            (Data as Fateful_Encounter_O).Fateful_Encounter, false);
    }
}