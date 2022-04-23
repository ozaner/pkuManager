using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ability_O : IndexTag_O
{
    public OneOf<IField<BigInteger>, IField<string>> Ability { get; }

    public bool IsValid(string abil) => IsValid(ABILITY_DEX, abil);
    public bool IsValid() => IsValid(AsString);

    public string AsString
    {
        get => AsStringGet(ABILITY_DEX, Ability);
        set => AsStringSet(ABILITY_DEX, Ability, value);
    }
}

public interface Ability_E : IndexTag_E
{
    public Ability_O Ability_Field { get; }
    public string Ability_Default => "None";

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportAbility()
        => ExportIndexTag("Ability", pku.Ability, Ability_Default, true,
            Ability_Field.IsValid, x => Ability_Field.AsString = x);
}