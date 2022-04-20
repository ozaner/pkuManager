﻿using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Is_Egg_O
{
    public IField<bool> Is_Egg { get; }
}

public interface Is_Egg_E : BooleanTag_E
{
    public Is_Egg_O Is_Egg_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportIs_Egg() => ExportIs_EggBase();

    public void ExportIs_EggBase()
        => ExportBooleanTag("Is Egg", pku.Egg_Info.Is_Egg, Is_Egg_Field.Is_Egg, false, false);
}

public interface Is_Egg_I : BooleanTag_I
{
    public pkuObject pku { get; }
    public Is_Egg_O Is_Egg_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportIs_Egg() => ImportIs_EggBase();

    public void ImportIs_EggBase()
        => ImportBooleanTag("Is Egg", pku.Egg_Info.Is_Egg, Is_Egg_Field.Is_Egg, false);
}