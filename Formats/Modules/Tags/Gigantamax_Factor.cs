using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Gigantamax_Factor_O
{
    public IField<bool> Gigantamax_Factor { get; }
}

public interface Gigantamax_Factor_E : BooleanTag_E
{
    public Gigantamax_Factor_O Gigantamax_Factor_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessGigantamax_Factor()
        => ProcessBooleanTag("Gigantamax Factor", pku.Gigantamax_Factor,
            Gigantamax_Factor_Field.Gigantamax_Factor, false, false);
}