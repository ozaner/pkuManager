using pkuManager.Common;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pkuManager.Formats.showdown;

/// <summary>
/// An implementation of the .txt (Showdown!) format used by Pokémon Showdown!.
/// </summary>
public class ShowdownObject : FormatObject
{
    /* ------------------------------------
     * Attributes
     * ------------------------------------
    */
    public string ShowdownName { get; set; }
    public string Nickname { get; set; }
    public string Item { get; set; }
    public string Ability { get; set; }
    public string[] Moves { get; set; }
    public byte Level { get; set; }
    public byte Friendship { get; set; }
    public BackedIntegralArrayField IVs { get; } = new(31, 0, 6);
    public BackedIntegralArrayField EVs { get; } = new(31, 0, 6);
    public Gender? Gender { get; set; }
    public Nature? Nature { get; set; }
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
        string introLine = "";

        // Species/Nickname
        if (Nickname is null || Nickname == ShowdownName)
            introLine += ShowdownName;
        else
            introLine += $"{Nickname} ({ShowdownName})";

        // Gender
        introLine += Gender switch
        {
            Common.Gender.Male => " (M)",
            Common.Gender.Female => " (F)",
            _ => ""
        };

        // Item
        if (Item is not null)
            introLine += $" @ {Item}";

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
        if (Friendship is not 255)
            Lines.Add($"Happiness: {Friendship}");

        // IVs
        if (!IVs.Get().All(x => x == 31))
        {
            string ivs = $"IVs: {(IVs[0] != 31 ? $"{IVs[0]} HP / " : "")}{(IVs[1] != 31 ? $"{IVs[1]} Atk / " : "")}" +
                            $"{(IVs[2] != 31 ? $"{IVs[2]} Def / " : "")}{(IVs[3] != 31 ? $"{IVs[3]} SpA / " : "")}" +
                            $"{(IVs[4] != 31 ? $"{IVs[4]} SpD / " : "")}{(IVs[5] != 31 ? $"{IVs[5]} Spe / " : "")}";
            ivs = ivs[0..^3]; //remove extra " / "
            Lines.Add(ivs);
        }

        // EVs
        if (!EVs.Get().All(x => x == 0))
        {
            string evs = $"EVs: {(EVs[0] != 0 ? $"{EVs[0]} HP / " : "")}{(EVs[1] != 0 ? $"{EVs[1]} Atk / " : "")}" +
                            $"{(EVs[2] != 0 ? $"{EVs[2]} Def / " : "")}{(EVs[3] != 0 ? $"{EVs[3]} SpA / " : "")}" +
                            $"{(EVs[4] != 0 ? $"{EVs[4]} SpD / " : "")}{(EVs[5] != 0 ? $"{EVs[5]} Spe / " : "")}";
            evs = evs[0..^3]; //remove extra " / "
            Lines.Add(evs);
        }

        // Nature
        if (Nature.HasValue)
            Lines.Add($"{Nature} Nature");

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
     * Showdown Utility
     * ------------------------------------
    */
    /// <summary>
    /// Searches the <see cref="SPECIES_DEX"/> for the
    /// Showdown name of a given pku's species/form/appearance.
    /// </summary>
    /// <param name="pku">The pku whose Showdown name is to be determined.</param>
    /// <returns><paramref name="pku"/>'s Showdown name.</returns>
    public static string GetShowdownName(pkuObject pku, bool ignoreCasting = false)
    {
        string searchStr = "Showdown Name";

        //Check for gender split
        bool? genderSplit = SPECIES_DEX.ReadSpeciesDex<bool?>(pku, ignoreCasting, "Showdown Gender Split");
        if(genderSplit is true)
        {
            Gender? gender = pku.Gender.ToEnum<Gender>();
            string genderStr = gender is Common.Gender.Female ? "Female" : "Male"; //Default is male

            //this could be bad iff a "Showdown Name {genderStr}" appears in a earlier apperance w/o Showdown Gender Split
            //that would be a data error though...
            searchStr += $" {genderStr}";
        }
        return SPECIES_DEX.ReadSpeciesDex<string>(pku, ignoreCasting, searchStr);
    }
}