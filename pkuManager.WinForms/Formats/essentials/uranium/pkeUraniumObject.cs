using OneOf;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Fields.BackedFields;
using pkuManager.WinForms.Formats.Fields.BAMFields;
using pkuManager.WinForms.Formats.Modules.Tags;
using pkuManager.WinForms.Utilities;
using System;
using System.Collections.Generic;

namespace pkuManager.WinForms.Formats.essentials.uranium;

public class pkeUraniumObject : StringFormatObject, Species_O, Nickname_O, Experience_O,
                                Ability_Slot_O, Gender_O, Nature_O, Shiny_O, Form_O,
                                Friendship_O, Moves_O, PP_Ups_O, Relearn_Moves_O, Ball_O,
                                Item_O, EVs_O, IVs_O, Pokerus_O, Markings_O, PID_O, TID_O,
                                OT_O, OT_Gender_O, Met_Level_O, Met_Location_O, Met_Date_O,
                                Egg_Location_O, Egg_Date_O, Egg_Steps_O, Language_O,
                                Fateful_Encounter_O
{
    public override string FormatName => "pkeUranium";


    /* ------------------------------------
     * Attributes
     * ------------------------------------
    */
    public BackedIntField Species { get; } = new();
    public BackedField<string> Nickname { get; } = new();
    public BackedIntField  Experience { get; } = new();
    public BackedIntField  Ability_Slot { get; } = new();
    public BackedIntField  Gender { get; } = new();
    public BackedIntField  Nature { get; } = new();
    public BackedField<bool> Shiny { get; } = new();
    public BackedIntField  Form { get; } = new();
    public BackedIntField  Friendship { get; } = new();
    public BackedIntArrayField Moves { get; } = new(4);
    public BackedIntArrayField PP_Ups { get; } = new(4);
    public BackedIntArrayField Relearn_Moves { get; } = new();
    public BackedIntField Ball { get; } = new();
    public BackedIntField Item { get; } = new();
    public BackedIntArrayField EVs { get; } = new(6);
    public BackedIntArrayField IVs { get; } = new(6);
    public BackedIntField Pokerus_Strain { get; } = new();
    public BackedIntField Pokerus_Days { get; } = new(15, 0);
    public BackedField<bool> Marking_Blue_Circle { get; } = new();
    public BackedField<bool> Marking_Blue_Square { get; } = new();
    public BackedField<bool> Marking_Blue_Triangle { get; } = new();
    public BackedField<bool> Marking_Blue_Heart { get; } = new();
    public BackedIntField PID { get; } = new();
    public BackedIntField TID { get; } = new();
    public BackedField<string> OT { get; } = new();
    public BackedIntField OT_Gender { get; } = new();
    public BackedIntField Met_Level { get; } = new();
    public BackedIntField Met_Location { get; } = new();
    public BackedIntField Met_Date { get; } = new(int.MaxValue, 0);
    public BackedIntField Egg_Location { get; } = new();
    public BackedIntField Egg_Date { get; } = new(int.MaxValue, 0);
    public BackedField<string> Primary_Location_Override { get; } = new();
    public BackedIntField Egg_Steps { get; } = new();
    public BackedIntField Language { get; } = new();
    public BackedField<bool> Fateful_Encounter { get; } = new();
    public BackedField<bool> Feral_Nuclear { get; } = new();


    /* ------------------------------------
     * Module Parameters
     * ------------------------------------
    */
    public Dictionary<AbilitySlot, int> Slot_Mapping => new()
    {
        { AbilitySlot.Slot_1, 0},
        { AbilitySlot.Slot_2, 1},
        { AbilitySlot.Slot_H, 2},
        { AbilitySlot.Slot_H2, 3},
    };
    public bool OT_Gender_CanBeNeutral => true;
    public bool Egg_Steps_ImplicitIs_Egg => true;


    /* ------------------------------------
     * File Conversion
     * ------------------------------------
    */
    protected int GetMarkingValue()
    {
        var map = (this as Markings_O).GetMapping();
        int sum = 0;
        for (Marking i = 0; i <= Marking.Blue_Heart; i++)
            sum += (map[i].Value ? 1 : 0) * (1 << (int)i);
        return sum;
    }

    protected override List<string> CompileLines() => new()
    {
        $"Species: {Species.Value}",
        $"Nickname: {Nickname.Value}",
        $"Exp: {Experience.Value}",
        $"Ability Slot: {Ability_Slot.Value}",
        $"Gender: {Gender.Value}",
        $"Nature: {Nature.Value}",
        $"Shiny: {Shiny.Value.ToString().ToLowerInvariant()}",
        $"Form: {Form.Value}",
        $"Friendship: {Friendship.Value}",
        $"Moves: {Moves.Value.ToFormattedString(true)}",
        $"PP Ups: {PP_Ups.Value.ToFormattedString(true)}",
        $"Relearn Moves: {Relearn_Moves.Value.ToFormattedString(true)}",
        $"Ball: {Ball.Value}",
        $"Item: {Item.Value}",
        $"EVs: {EVs.Value.ToFormattedString(true)}",
        $"IVs: {IVs.Value.ToFormattedString(true)}",
        $"Pokerus Strain: {Pokerus_Strain.Value}",
        $"Pokerus Days: {Pokerus_Days.Value}",
        $"Markings: {GetMarkingValue()}",
        $"PID: {PID.Value}",
        $"TID: {TID.Value}",
        $"OT: {OT.Value}",
        $"OT Gender: {OT_Gender.Value}",
        $"Met Location: {Met_Location.Value}",
        $"Met Location Override: {Primary_Location_Override.Value}",
        $"Met Date: {Met_Date.Value}",
        $"Met Level: {Met_Level.Value}",
        $"Hatched Location: {Egg_Location.Value}",
        $"Hatched Date: {Egg_Date.Value}",
        $"Egg Steps: {Egg_Steps.Value}",
        $"Language: {Language.Value}",
        $"Fateful: {Fateful_Encounter.Value.ToString().ToLowerInvariant()}",
        $"Feral Nuclear: {Feral_Nuclear.Value.ToString().ToLowerInvariant()}",
    };

    protected override string DecompileLines(List<string> lines)
    {
        throw new NotImplementedException();
    }


    /* ------------------------------------
     * Duct Tape
     * ------------------------------------
    */
    OneOf<IIntField, IField<string>> Species_O.Species => Species;
    OneOf<BAMStringField, IField<string>> Nickname_O.Nickname => Nickname;
    IIntField Experience_O.Experience => Experience;
    IIntField Ability_Slot_O.Ability_Slot => Ability_Slot;
    OneOf<IIntField, IField<Gender>, IField<Gender?>> Gender_O.Gender => Gender;
    OneOf<IIntField, IField<Nature>> Nature_O.Nature => Nature;
    IField<bool> Shiny_O.Shiny => Shiny;
    OneOf<IIntField, IField<string>> Form_O.Form => Form;
    IIntField Friendship_O.Friendship => Friendship;
    OneOf<IIntArrayField, IField<string[]>> Moves_O.Moves => Moves;
    IIntArrayField PP_Ups_O.PP_Ups => PP_Ups;
    OneOf<IIntArrayField, IField<string[]>> Relearn_Moves_O.Relearn_Moves => Relearn_Moves;
    OneOf<IIntField, IField<string>> Ball_O.Ball => Ball;
    OneOf<IIntField, IField<string>> Item_O.Item => Item;
    IIntArrayField EVs_O.EVs => EVs;
    IIntArrayField IVs_O.IVs => IVs;
    IIntField Pokerus_O.Pokerus_Strain => Pokerus_Strain;
    IIntField Pokerus_O.Pokerus_Days => Pokerus_Days;

    IField<bool> Markings_O.Marking_Blue_Circle => Marking_Blue_Circle;
    IField<bool> Markings_O.Marking_Blue_Square => Marking_Blue_Square;
    IField<bool> Markings_O.Marking_Blue_Triangle => Marking_Blue_Triangle;
    IField<bool> Markings_O.Marking_Blue_Heart => Marking_Blue_Heart;

    IIntField PID_O.PID => PID;
    IIntField TID_O.TID => TID;
    OneOf<BAMStringField, IField<string>> OT_O.OT => OT;
    OneOf<IIntField, IField<Gender>> OT_Gender_O.OT_Gender => OT_Gender;
    IIntField Met_Level_O.Met_Level => Met_Level;
    IIntField Met_Location_O.Met_Location => Met_Location;
    OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> Met_Date_O.Met_Date => Met_Date;
    IIntField Egg_Location_O.Egg_Location => Egg_Location;
    OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> Egg_Date_O.Egg_Date => Egg_Date;
    IIntField Egg_Steps_O.Egg_Steps => Egg_Steps;
    OneOf<IIntField, IField<string>> Language_O.Language => Language;
    IField<bool> Fateful_Encounter_O.Fateful_Encounter => Fateful_Encounter;
}