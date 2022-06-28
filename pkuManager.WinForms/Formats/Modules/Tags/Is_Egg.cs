using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Is_Egg_O
{
    public IField<bool> Is_Egg { get; }
}

public interface Is_Egg_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportIs_Egg() => ExportIs_EggBase();

    public void ExportIs_EggBase()
        => BooleanTagUtil.ExportBooleanTag(pku.Egg_Info.Is_Egg, (Data as Is_Egg_O).Is_Egg, false);
}

public interface Is_Egg_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportIs_Egg() => ImportIs_EggBase();

    public void ImportIs_EggBase()
        => BooleanTagUtil.ImportBooleanTag(pku.Egg_Info.Is_Egg, (Data as Is_Egg_O).Is_Egg, false);
}