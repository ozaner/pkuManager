using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.Modules.Nature_Util;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

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

public static class Nature_Util
{
    public const Nature DEFAULT_NATURE = Nature.Hardy;

    /// <summary>
    /// An official nature a Pokémon can have.<br/>
    /// Index numbers correspond to those used in the official games.
    /// </summary>
    public enum Nature
    {
        Hardy,
        Lonely,
        Brave,
        Adamant,
        Naughty,
        Bold,
        Docile,
        Relaxed,
        Impish,
        Lax,
        Timid,
        Hasty,
        Serious,
        Jolly,
        Naive,
        Modest,
        Mild,
        Quiet,
        Bashful,
        Rash,
        Calm,
        Gentle,
        Sassy,
        Careful,
        Quirky
    }
}

public interface Nature_E : EnumTag_E
{
    public pkuObject pku { get; }
    public Nature_O Nature_Field { get; }

    public Nature? Nature_Default => DEFAULT_NATURE;
    public bool Nature_AlertIfUnspecified => true;
    public Func<AlertType, string, string, Alert> Nature_Alert_Func => null;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessNature()
        => ProcessEnumTag("Nature", pku.Nature, Nature_Default, Nature_Field.Nature, Nature_AlertIfUnspecified, Nature_Alert_Func);
}