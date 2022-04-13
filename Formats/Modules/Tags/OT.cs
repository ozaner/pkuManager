using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface OT_O
{
    public OneOf<BAMStringField, IField<string>> OT { get; }
}

public interface OT_E : StringTag_E
{
    public pkuObject pku { get; }
    public OT_O OT_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ProcessLanguage))]
    public void ProcessOT() => ProcessOTBase();

    public void ProcessOTBase()
        => OT_Resolver = ProcessString("OT", pku.Game_Info.OT.Value, OT_Field.OT);

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger[]> OT_Resolver { get => null; set { } }
}