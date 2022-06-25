using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Gigantamax_Factor_O
{
    public IField<bool> Gigantamax_Factor { get; }
}

public interface Gigantamax_Factor_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportGigantamax_Factor()
    {
        BooleanTagUtil.ExportBooleanTag(pku.Gigantamax_Factor,
            (Data as Gigantamax_Factor_O).Gigantamax_Factor, false);
    }
}