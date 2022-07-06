using OneOf;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Fields.BAMFields;
using pkuManager.WinForms.Formats.Modules.Templates;
using pkuManager.WinForms.Formats.pku;
using System.Numerics;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface OT_O
{
    public OneOf<BAMStringField, IField<string>> OT { get; }
}

public interface OT_E : StringTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ExportLanguage))]
    public void ExportOT() => ExportOTBase();

    public void ExportOTBase()
        => OT_Resolver = ExportString("OT", pkuObject.ChooseField(true, pku.Game_Info.OT,
            pku.Game_Info.Official_OT).Value, (Data as OT_O).OT);

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger[]> OT_Resolver { get => null; set { } }
}

public interface OT_I : StringTag_I
{
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_I.ImportLanguage),
                                                nameof(Is_Egg_I.ImportIs_Egg))]
    public void ImportOT()
        => OT_Resolver = ImportString("OT", pku.Game_Info.OT, (Data as OT_O).OT);

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<string> OT_Resolver { get => null; set { } }
}