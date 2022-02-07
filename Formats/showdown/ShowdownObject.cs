using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pkuManager.Formats.showdown;

/// <summary>
/// An implementation of the .txt (Showdown!) format used by Pokémon Showdown!.
/// </summary>
public class ShowdownObject : FormatObject, Species_O, Form_O, Item_O, Nature_O,
                              Friendship_O, IVs_O, EVs_O
{
    public override string FormatName => "Showdown";

    /* ------------------------------------
     * Attributes
     * ------------------------------------
    */
    public BackedField<string> Species { get; } = new();
    public BackedField<string> Form { get; } = new();
    public string Nickname { get; set; }
    public BackedField<string> Item { get; } = new();
    public string Ability { get; set; }
    public string[] Moves { get; set; }
    public byte Level { get; set; }
    public BackedIntegralField Friendship { get; } = new(255, 0);
    public BackedIntegralArrayField IVs { get; } = new(6, 31, 0);
    public BackedIntegralArrayField EVs { get; } = new(6, 255, 0);
    public Gender? Gender { get; set; }
    public BackedField<Nature?> Nature { get; } = new();
    public bool Shiny { get; set; }
    public bool Gigantamax_Factor { get; set; }
    // PP Ups not used in Showdown, by default always max PP
    // Found one thread regarding it but nothing seems to have come of it:
    // https://www.smogon.com/forums/threads/allow-moves-to-have-non-max-pp.3653621/

    /// <summary>
    /// A list of strings that will be added to the final .txt file upon calling <see cref="ToFile"/>.
    /// </summary>
    protected List<string> Lines = new();
        
    /// <summary>
    /// Compiles each of the object's attributes to strings which are added to <see cref="Lines"/>.
    /// </summary>
    protected virtual void CompileLines()
    {
        //Full Showdown Name
        string showdownName = Species.Value;
        if (!Form.IsNull())
            showdownName += $"-{Form.Value}";

        // Nickname
        string introLine = "";
        if (Nickname is null || Nickname == showdownName)
            introLine += showdownName;
        else
            introLine += $"{Nickname} ({showdownName})";

        // Gender
        introLine += Gender switch
        {
            Modules.Gender.Male => " (M)",
            Modules.Gender.Female => " (F)",
            _ => ""
        };

        // Item
        if (!Item.IsNull())
            introLine += $" @ {Item.Value}";

        Lines.Add(introLine);

        // Ability
        if (Ability is not null)
            Lines.Add($"Ability: {Ability}");

        // Level
        if (Level is not 100)
            Lines.Add($"Level: {Level}");

        // Shiny (no preprocessing)
        if (Shiny)
            Lines.Add("Shiny: true");

        // Friendship
        if (Friendship.Value != 255)
            Lines.Add($"Happiness: {Friendship.Value}");

        // IVs
        if (!IVs.Value.All(x => x == 31))
        {
            string ivs = $"IVs: {(IVs.Value[0] != 31 ? $"{IVs.Value[0]} HP / " : "")}{(IVs.Value[1] != 31 ? $"{IVs.Value[1]} Atk / " : "")}" +
                            $"{(IVs.Value[2] != 31 ? $"{IVs.Value[2]} Def / " : "")}{(IVs.Value[3] != 31 ? $"{IVs.Value[3]} SpA / " : "")}" +
                            $"{(IVs.Value[4] != 31 ? $"{IVs.Value[4]} SpD / " : "")}{(IVs.Value[5] != 31 ? $"{IVs.Value[5]} Spe / " : "")}";
            ivs = ivs[0..^3]; //remove extra " / "
            Lines.Add(ivs);
        }

        // EVs
        if (!EVs.Value.All(x => x == 0))
        {
            string evs = $"EVs: {(EVs.Value[0] != 0 ? $"{EVs.Value[0]} HP / " : "")}{(EVs.Value[1] != 0 ? $"{EVs.Value[1]} Atk / " : "")}" +
                            $"{(EVs.Value[2] != 0 ? $"{EVs.Value[2]} Def / " : "")}{(EVs.Value[3] != 0 ? $"{EVs.Value[3]} SpA / " : "")}" +
                            $"{(EVs.Value[4] != 0 ? $"{EVs.Value[4]} SpD / " : "")}{(EVs.Value[5] != 0 ? $"{EVs.Value[5]} Spe / " : "")}";
            evs = evs[0..^3]; //remove extra " / "
            Lines.Add(evs);
        }

        // Nature
        if (!Nature.IsNull())
            Lines.Add($"{Nature.Value} Nature");

        // Gigantamax
        if (Gigantamax_Factor)
            Lines.Add("Gigantamax: Yes");

        // Moves
        foreach (string move in Moves)
            Lines.Add($"- {move}");
    }

    public override byte[] ToFile()
    {
        CompileLines();
        string txt = string.Join("\n", Lines);
        return Encoding.UTF8.GetBytes(txt);
    }

    public override void FromFile(byte[] file)
    {
        throw new NotImplementedException();
    }


    /* ------------------------------------
     * Duct Tape
     * ------------------------------------
    */
    OneOf<IIntegralField, IField<string>> Species_O.Species => Species;
    OneOf<IIntegralField, IField<string>> Form_O.Form => Form;
    OneOf<IIntegralField, IField<string>> Item_O.Item => Item;
    OneOf<IIntegralField, IField<Nature>, IField<Nature?>> Nature_O.Nature => Nature;
    IIntegralField Friendship_O.Friendship => Friendship;
    IIntegralArrayField IVs_O.IVs => IVs;
    IIntegralArrayField EVs_O.EVs => EVs;
}