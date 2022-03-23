using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.Modules.Language_Util;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Language_O
{
    public OneOf<IField<BigInteger>, IField<Language>, IField<Language?>> Language { get; }

    public Language? Value
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

public static class Language_Util
{
    public const Language DEFAULT_LANGUAGE = Language.English;

    /// <summary>
    /// An official language a Pokémon can have.
    /// Index numbers correspond to those used in the official games.
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// Unset language ID.<br/>
        /// Note that Gen 5 Japanese in-game trades use this value. Great...
        /// </summary>
        //None = 0,

        /// <summary>
        /// Japanese (日本語)
        /// </summary>
        Japanese = 1,

        /// <summary>
        /// English (US/UK/AU)
        /// </summary>
        English = 2,

        /// <summary>
        /// French (Français)
        /// </summary>
        French = 3,

        /// <summary>
        /// Italian (Italiano)
        /// </summary>
        Italian = 4,

        /// <summary>
        /// German (Deutsch)
        /// </summary>
        German = 5,

        /// <summary>
        /// Unused language ID reserved for Korean in Gen 3 but never used.
        /// </summary>
        //Korean_Gen_3 = 6,

        /// <summary>
        /// Spanish (Español)
        /// </summary>
        Spanish = 7,

        /// <summary>
        /// Korean (한국어)
        /// </summary>
        Korean = 8,

        /// <summary>
        /// Chinese Simplified (简体中文)
        /// </summary>
        Chinese_Simplified = 9,

        /// <summary>
        /// Chinese Traditional (繁體中文)
        /// </summary>
        Chinese_Traditional = 10
    }
}

public interface Language_E : EnumTag_E
{
    public pkuObject pku { get; }
    public Language_O Language_Field { get; }

    public Language? Language_Default => Language.English;
    public bool Language_AlertIfUnspecified => true;
    public Func<AlertType, string, string, Alert> Language_Alert_Func => null;
    public Predicate<Language> Language_IsValid { get; }

    public void ProcessLanguageBase()
        => ProcessEnumTag("Language", pku.Game_Info.Language, Language_Default, Language_Field.Language,
            Language_AlertIfUnspecified, Language_Alert_Func, Language_IsValid);

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessLanguage() => ProcessLanguageBase();
}