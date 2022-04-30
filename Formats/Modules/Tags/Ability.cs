using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
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
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportAbility()
    {
        Ability_O abilityObj = Data as Ability_O;
        AlertType at = ExportIndexTag(pku.Ability, "None", abilityObj.IsValid, x => abilityObj.AsString = x);
        Warnings.Add(GetAbilityAlert(at));
    }

    public Alert GetAbilityAlert(AlertType at)
        => GetAbilityAlertBase(at);

    public Alert GetAbilityAlertBase(AlertType at)
        => GetIndexAlert("Abilities", at, pku.Ability.Value, "None");
}