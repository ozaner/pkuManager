using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ability_O
{
    public OneOf<IField<BigInteger>, IField<string>> Ability { get; }
    public string FormatName { get; }

    public bool IsValid(string abil) => ABILITY_DEX.ExistsIn(FormatName, abil);
    public bool IsValid() => IsValid(AsString);

    public string AsString
    {
        get => Ability.Match(
            x => ABILITY_DEX.SearchIndexedValue<int?>(x.GetAs<int>(), FormatName, "Indices", "$x"),
            x => x.Value);
        set => Ability.Switch(
            x => x.Value = ABILITY_DEX.GetIndexedValue<int?>(FormatName, value, "Indices") ?? 0,
            x => x.Value = value);
    }
}

public interface Ability_E : IndexTag_E
{
    public pkuObject pku { get; }
    public Ability_O Ability_Field { get; }
    public string Ability_Default => "None";

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessAbility()
        => ProcessIndexTag("Ability", pku.Ability, Ability_Default, true,
            Ability_Field.IsValid, x => Ability_Field.AsString = x);
}