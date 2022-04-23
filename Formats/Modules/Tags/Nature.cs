using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Nature_O
{
    public OneOf<IField<BigInteger>, IField<Nature>, IField<Nature?>> Nature { get; }

    public Nature? Value
    {
        get => Nature.Match(
            x => x.Value.ToEnum<Nature>(),
            x => x.Value,
            x => x.Value
        );
        set => Nature.Switch(
            x => x.Value = (int)value,
            x => x.Value = value ?? DEFAULT_NATURE,
            x => x.Value = value);
    }
}

public interface Nature_E : EnumTag_E
{
    public Nature_O Nature_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportNature()
        => ExportEnumTag("Nature", pku.Nature, Nature_Field.Nature.IsT2 ? null : DEFAULT_NATURE,
            Nature_Field.Nature, true, GetNatureAlert);

    public Alert GetNatureAlert(AlertType at, string val, string defaultVal)
        => GetNatureAlertBase(at, val, defaultVal);

    public Alert GetNatureAlertBase(AlertType at, string val, string defaultVal)
        => GetEnumAlert("Nature", at, val, defaultVal);
}