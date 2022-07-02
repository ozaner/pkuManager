using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules;
using pkuManager.WinForms.Formats.Modules.Tags;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.WinForms.Formats.PorterDirective;
using pkuManager.WinForms.Formats.Modules.MetaTags;

namespace pkuManager.WinForms.Formats.pkx.pk3;

/// <summary>
/// Exports a <see cref="pkuObject"/> to a <see cref="pk3Object"/>.
/// </summary>
public class pk3Exporter : Exporter, BattleStatOverride_E, FormCasting_E, SFA_E, Shiny_E,
                           Gender_E, Nickname_E, Experience_E, Moves_E, PP_Ups_E, PP_E, Item_E,
                           Nature_E, Ability_Slot_E, Friendship_E, PID_E, TID_E, IVs_E, EVs_E,
                           Contest_Stats_E, Ball_E, OT_E, Origin_Game_E, Met_Location_E,
                           Met_Level_E, OT_Gender_E, Language_E, Fateful_Encounter_E, Markings_E,
                           Ribbons_E, Is_Egg_E, Egg_Steps_E, Pokerus_E, Trash_Bytes_E, ByteOverride_E
{
    public override string FormatName => "pk3";

    /// <summary>
    /// Creates an exporter that will attempt to export <paramref name="pku"/>
    /// to a .pk3 file with the given <paramref name="globalFlags"/>.
    /// </summary>
    /// <inheritdoc cref="Exporter(pkuObject, GlobalFlags, FormatObject)"/>
    public pk3Exporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags)
    {
        // Screen Species & Form
        if (DexUtil.FirstFormInFormat(this.pku, FormatName, true, GlobalFlags.Default_Form_Override) is null)
            Reason = "Must be a species & form that exists in Gen 3.";
        
        // Screen Shadow Pokemon
        else if (this.pku.IsShadow())
            Reason = "This format doesn't support Shadow Pokémon.";
    }

    public override pk3Object Data { get; } = new();


    /* ------------------------------------
     * Working Variables
     * ------------------------------------
    */
    public bool legalGen3Egg;
    public string[] Moves_Indices { get; set; }
    public ChoiceAlert PID_DependencyError { get; set; }
    public Dictionary<string, object> PID_DependencyDigest { get; set; }
    public ChoiceAlert Language_DependencyError { get; set; }


    /* ------------------------------------
     * Exporting Parameters
     * ------------------------------------
    */
    public bool UseOfficialValues => true;


    /* ------------------------------------
     * Custom Processing Methods
     * ------------------------------------
    */
    // SFA
    [PorterDirective(ProcessingPhase.FirstPass, nameof(PID_E.ExportPID))]
    public void ExportSFA()
    {
        (this as SFA_E).ExportSFABase();
        Data.HasSpecies.ValueAsBool = true;

        //deal with Unown form pid dependence
        if (pku.Species.Value is "Unown")
        {
            string pkuForm = pku.Forms.Value.JoinLexical();
            string pidForm = TagUtil.GetUnownFormName(Data.Form.GetAs<int>());

            int? exportedID = TagUtil.GetUnownFormIDFromName(pkuForm);
            PID_DependencyDigest["Unown Form"] = exportedID;

            //add to pid dep error if necessary
            //pkuForm should be valid at this point
            if (PID_DependencyError is not null && pkuForm != pidForm)
            {
                var x = PID_DependencyError.Choices;
                x[0].Message = x[0].Message.AddNewLine($"Unown Form: {pidForm}");
                x[1].Message = x[1].Message.AddNewLine($"Unown Form: {pkuForm}");
            }
        }
    }

    // Egg
    public void ExportIs_Egg()
    {
        (this as Is_Egg_E).ExportIs_EggBase();

        //Deal with "Legal Gen 3 eggs"
        if (pku.IsEgg())
        {
            //To be seen as legal must have no nickname or a defined language + matching "Egg" nickname.
            bool isValid = LANGUAGE_DEX.ExistsIn(FormatName, pku.Game_Info.Language.Value);
            if (pku.Nickname.IsNull() || isValid && TagUtil.EGG_NICKNAME[pku.Game_Info.Language.Value] == pku.Nickname.Value)
            {
                Data.UseEggName.ValueAsBool = true;
                legalGen3Egg = true;
            }
        }
    }

    // Language
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ExportIs_Egg))]
    public void ExportLanguage()
    {
        (this as Language_E).ExportLanguageBase();
        
        if (legalGen3Egg) // Sets language to JPN if legal egg
            Language_Resolver = new(null, Data.Language, 1); //JPN = 1
    }

    // Nickname
    public void ExportNickname()
    {
        if (legalGen3Egg)
            Data.Nickname.Value = DexUtil.CharEncoding.Encode(TagUtil.EGG_NICKNAME["Japanese"],
                pk3Object.MAX_NICKNAME_CHARS, FormatName, "Japanese").encodedStr;
        else
            (this as Nickname_E).ExportNicknameBase();
    }

    // Ribbons
    public void ExportRibbons()
    {
        HashSet<Ribbon> ribbons = pku.Ribbons.ToEnumSet<Ribbon>(); //get ribbon list

        //In other words, if the pku has a contest ribbon at level x, but not at level x-1 (when x-1 exists).
        bool ribbonAlert =
            ribbons.Contains(Ribbon.Cool_Super_G3) && !ribbons.Contains(Ribbon.Cool_G3) ||
            ribbons.Contains(Ribbon.Cool_Hyper_G3) && !ribbons.Contains(Ribbon.Cool_Super_G3) ||
            ribbons.Contains(Ribbon.Cool_Master_G3) && !ribbons.Contains(Ribbon.Cool_Hyper_G3) ||
            ribbons.Contains(Ribbon.Beauty_Super_G3) && !ribbons.Contains(Ribbon.Beauty_G3) ||
            ribbons.Contains(Ribbon.Beauty_Hyper_G3) && !ribbons.Contains(Ribbon.Beauty_Super_G3) ||
            ribbons.Contains(Ribbon.Beauty_Master_G3) && !ribbons.Contains(Ribbon.Beauty_Hyper_G3) ||
            ribbons.Contains(Ribbon.Cute_Super_G3) && !ribbons.Contains(Ribbon.Cute_G3) ||
            ribbons.Contains(Ribbon.Cute_Hyper_G3) && !ribbons.Contains(Ribbon.Cute_Super_G3) ||
            ribbons.Contains(Ribbon.Cute_Master_G3) && !ribbons.Contains(Ribbon.Cute_Hyper_G3) ||
            ribbons.Contains(Ribbon.Smart_Super_G3) && !ribbons.Contains(Ribbon.Smart_G3) ||
            ribbons.Contains(Ribbon.Smart_Hyper_G3) && !ribbons.Contains(Ribbon.Smart_Super_G3) ||
            ribbons.Contains(Ribbon.Smart_Master_G3) && !ribbons.Contains(Ribbon.Smart_Hyper_G3) ||
            ribbons.Contains(Ribbon.Tough_Super_G3) && !ribbons.Contains(Ribbon.Tough_G3) ||
            ribbons.Contains(Ribbon.Tough_Hyper_G3) && !ribbons.Contains(Ribbon.Tough_Super_G3) ||
            ribbons.Contains(Ribbon.Tough_Master_G3) && !ribbons.Contains(Ribbon.Tough_Hyper_G3);
        
        if (ribbonAlert)
            Warnings.Add(GetContestRibbonAlert());

        //Add contest ribbons
        Data.Cool_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Cool_G3, ribbons));
        Data.Beauty_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Beauty_G3, ribbons));
        Data.Cute_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Cute_G3, ribbons));
        Data.Smart_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Smart_G3, ribbons));
        Data.Tough_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Tough_G3, ribbons));

        (this as Ribbons_E).ExportRibbonsBase(); //Process other ribbons
    }

    // Fateful Encounter
    [PorterDirective(ProcessingPhase.FirstPass, nameof(SFA_E.ExportSFA))]
    public void ExportFateful_Encounter()
    {
        //Mew or Deoxys w/ no fateful encounter
        if (pku.Species.Value is "Mew" or "Deoxys" && pku.Catch_Info.Fateful_Encounter.Value is not true)
        {
            Alert alert = GetObedienceAlert(pku.Species.Value is "Mew");
            Fateful_EncounterResolver = new(alert, Data.Fateful_Encounter, new[] { false, true });
            Errors.Add(alert);
        }
        else
            (this as Fateful_Encounter_E).ExportFateful_EncounterBase();
    }


    /* ------------------------------------
     * Error Resolvers
     * ------------------------------------
    */
    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<bool> Fateful_EncounterResolver { get; set; }

    public ErrorResolver<BigInteger> Experience_Resolver { get; set; }
    public ErrorResolver<BigInteger> PID_Resolver { get; set; }
    public ErrorResolver<BigInteger> Language_Resolver { get; set; }
    public ErrorResolver<BigInteger[]> Nickname_Resolver { get; set; }
    public ErrorResolver<BigInteger[]> OT_Resolver { get; set; }
    public Action ByteOverride_Action { get; set; }


    /* ------------------------------------
     * Custom Alerts
     * ------------------------------------
    */
    public Alert GetSFAAlert(DexUtil.SFA sfa)
    {
        if (sfa.Species == "Deoxys" && sfa.Form != "Normal") //must be another valid deoxys form to get here
            return new("Form", "Note that in generation 3, Deoxys' form depends on what game it is currently in.");
        else if (sfa.Species == "Castform" && sfa.Form != "Normal")
            return new("Form", "Note that in generation 3, Castform can only be in its 'Normal' form out of battle.");
        else
            return null;
    }

    public static Alert GetContestRibbonAlert()
        => new("Ribbons", "This pku has a Gen 3 contest ribbon of some category with rank super or higher, " +
            "but doesn't have the ribbons below that rank. This is impossible in this format, adding those ribbons.");

    public static Alert GetObedienceAlert(bool isMew)
    {
        string pkmn = isMew ? "Mew" : "Deoxys";
        string msg = $"This {pkmn} was not met in a fateful encounter. " +
            $"Note that, in the Gen 3 games, {pkmn} will only obey the player if it was met in a fateful encounter.";

        ChoiceAlert.SingleChoice[] choices =
        {
            new("Keep Fateful Encounter",$"Fateful Encounter: false\n{pkmn} won't obey."),
            new("Set Fateful Encounter",$"Fateful Encounter: true\n{pkmn} will obey.")
        };

        return new ChoiceAlert("Fateful Encounter", msg, choices, true);
    }
}