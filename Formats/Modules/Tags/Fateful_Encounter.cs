using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Fateful_Encounter_O
{
    public IField<bool> Fateful_Encounter { get; }
}

public interface Fateful_Encounter_E : BooleanTag_E
{
    public Fateful_Encounter_O Fateful_Encounter_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportFateful_Encounter() => ExportFateful_EncounterBase();

    public void ExportFateful_EncounterBase()
        => ExportBooleanTag("Fateful Encounter", pku.Catch_Info.Fateful_Encounter,
            Fateful_Encounter_Field.Fateful_Encounter, false, false);
}