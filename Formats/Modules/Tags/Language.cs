using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Language_O
{
    public OneOf<IField<BigInteger>, IField<string>> Language { get; }
    public string FormatName { get; }

    public bool IsValid => LANGUAGE_DEX.ExistsIn(FormatName, AsString);

    public string AsString
    {
        get => Language.Match(
            x => LANGUAGE_DEX.SearchIndexedValue<int?>(x.GetAs<int>(), FormatName, "Indices", "$x"),
            x => x.Value);
        set => Language.Switch(
            x => x.Value = LANGUAGE_DEX.GetIndexedValue<int?>(FormatName, value, "Indices") ?? 0,
            x => x.Value = value);
    }
}

public interface Language_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Language_O Language_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessLanguage() => ProcessLanguageBase();

    public void ProcessLanguageBase()
    {
        ProcessIndexTag("Language", pku.Game_Info.Language, "None", Language_Field.Language, true,
            x => LANGUAGE_DEX.ExistsIn(FormatName, x),
            x => LANGUAGE_DEX.GetIndexedValue<int?>(FormatName, x, "Indices") ?? 0,
            x => LANGUAGE_DEX.GetIndexedValue<string>(FormatName, x, "Indices")
        );
    }
}