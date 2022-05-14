using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ability_O
{
    public OneOf<IField<BigInteger>, IField<string>> Ability { get; }
}

public interface Ability_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportAbility()
    {
        AlertType at = IndexTagUtil.ExportIndexTag(pku.Ability, (Data as Ability_O).Ability, "None", ABILITY_DEX, FormatName);
        Warnings.Add(GetAbilityAlert(at));
    }

    public Alert GetAbilityAlert(AlertType at)
        => GetAbilityAlertBase(at);

    public Alert GetAbilityAlertBase(AlertType at)
        => IndexTagUtil.GetIndexAlert("Abilities", at, pku.Ability.Value, "None");
}