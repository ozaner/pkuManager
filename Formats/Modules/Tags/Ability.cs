﻿using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ability_O
{
    public OneOf<IField<BigInteger>, IField<string>> Ability { get; }
}

public interface Ability_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Ability_O Ability_Field { get; }
    public string Ability_Default => "None";

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessAbility()
        => ProcessIndexTag("Ability", pku.Ability, Ability_Default, Ability_Field.Ability, true,
            x => ABILITY_DEX.ExistsIn(FormatName, x), Ability_Field.Ability.Match<OneOf<Func<string, int>, Func<string, string>>>(
                _ => (Func<string, int>)(x => ABILITY_DEX.GetIndexedValue<int?>(FormatName, x, "Indices") ?? 0),
                _ => (Func<string, string>)(x => ABILITY_DEX.GetIndexedValue<string>(FormatName, x, "Indices"))
            ));
}