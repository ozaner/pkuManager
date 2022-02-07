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
}

public interface Language_E : EnumTag_E
{
    public pkuObject pku { get; }
    public Language_O Data { get; }

    public Language? Language_Default => Language.English;
    public bool Language_AlertIfUnspecified => true;
    public Func<AlertType, string, string, Alert> Language_Alert_Func => null;
    public Predicate<Language> Language_IsValid { get; }

    public void ProcessLanguageBase()
        => ProcessEnumTag("Language", pku.Game_Info.Language, Language_Default, Data.Language,
            Language_AlertIfUnspecified, Language_Alert_Func, Language_IsValid);

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessLanguage() => ProcessLanguageBase();
}