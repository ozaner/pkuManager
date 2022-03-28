using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Language_O
{
    public OneOf<IField<BigInteger>, IField<Language>, IField<Language?>> Language { get; }

    public sealed Language? Value
    {
        get => Language.Match(
            x => x.Value.ToEnum<Language>(),
            x => x.Value,
            x => x.Value
        );
        set => Language.Switch(
            x => x.Value = (int)value,
            x => x.Value = value ?? DEFAULT_LANGUAGE,
            x => x.Value = value);
    }
}

public interface Language_E : EnumTag_E
{
    public pkuObject pku { get; }

    public Language_O Language_Field { get; }
    public Predicate<Language> Language_IsValid { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessLanguage() => ProcessLanguageBase();

    public void ProcessLanguageBase()
        => ProcessEnumTag("Language", pku.Game_Info.Language, DEFAULT_LANGUAGE,
            Language_Field.Language, true, GetLanguageAlert, Language_IsValid);

    protected Alert GetLanguageAlert(AlertType at, string val, string defaultVal)
        => GetEnumAlert("Language", at, val, defaultVal);
}