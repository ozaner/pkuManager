using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Alerts.Alert;
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
    public void ExportGigantamax_Factor()
    {
        AlertType at = ExportBooleanTag(pku.Gigantamax_Factor,
            Gigantamax_Factor_Field.Gigantamax_Factor, false);
        if (at is not AlertType.UNSPECIFIED)
            GetBooleanAlert("Gigantamax Factor", at, false);
    }
}