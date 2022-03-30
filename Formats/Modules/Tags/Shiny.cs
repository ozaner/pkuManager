using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Shiny_O
{
    public IField<bool> Shiny { get; }
}

public interface Shiny_E : BooleanTag_E
{
    public Shiny_O Shiny_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessShiny()
        => ProcessBooleanTag("Shiny", pku.Shiny, Shiny_Field.Shiny, false, false);
}