using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Language_O
{
    public OneOf<IField<BigInteger>, IField<Language>, IField<Language?>> Language { get; }

    public Language? Value => Language.Match(
        x => x.GetAs<Language>(),
        x => x.Value,
        x => x.Value
    );
}

public interface Language_E : EnumTag_E
{
    public pkuObject pku { get; }
    public Language_O Language_Data { get; }

    public Language? Language_Default => Language.English;
    public bool Language_AlertIfUnspecified => true;
    public Func<AlertType, string, string, Alert> Language_Alert_Func => null;
    public Predicate<Language> Language_IsValid { get; }

    public void ProcessLanguageBase()
        => ProcessEnumTag("Language", pku.Game_Info.Language, Language_Default, Language_Data.Language,
            Language_AlertIfUnspecified, Language_Alert_Func, Language_IsValid);

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessLanguage() => ProcessLanguageBase();
}