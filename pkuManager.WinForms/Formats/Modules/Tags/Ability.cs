using OneOf;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Ability_O
{
    public OneOf<IIntField, IField<string>> Ability { get; }
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