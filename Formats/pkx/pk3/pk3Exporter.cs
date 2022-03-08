using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.Modules;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.pkx.pk3;

/// <summary>
/// Exports a <see cref="pkuObject"/> to a <see cref="pk3Object"/>.
/// </summary>
public class pk3Exporter : Exporter, BattleStatOverride_E, FormCasting_E, Species_E, Form_E,
                           Item_E, Nature_E, Friendship_E, TID_E, IVs_E, EVs_E, Contest_Stats_E,
                           Ball_E, Origin_Game_E, Met_Level_E, OT_Gender_E, Language_E
{
    public override string FormatName => "pk3";

    /// <summary>
    /// Creates an exporter that will attempt to export <paramref name="pku"/>
    /// to a .pk3 file with the given <paramref name="globalFlags"/>.
    /// </summary>
    /// <inheritdoc cref="Exporter(pkuObject, GlobalFlags, FormatObject)"/>
    public pk3Exporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

    protected override pk3Object Data { get; } = new();

    public override (bool canPort, string reason) CanPort()
    {
        // Screen Species & Form
        if (DexUtil.FirstFormInFormat(pku, FormatName, true, GlobalFlags.Default_Form_Override) is null)
            return (false, "Must be a species & form that exists in Gen 3.");

        // Screen Shadow Pokemon
        if (pku.IsShadow())
            return (false, "Cannot be a Shadow Pokémon.");

        return (true, null); //compatible with .pk3
    }

    // Module Parameters
    public Nature? Nature_Default => null;
    public Func<AlertType, string, string, Alert> Nature_Alert_Func => GetNatureAlert;
    public Predicate<Language> Language_IsValid => pk3Object.IsValidLang;

    // Working variables
    protected WorkingVariables workingVars = new();
    protected partial class WorkingVariables
    {
        public BackedBoundableField<BigInteger> Form { get; } = new(); // Form [Implicit]
        public BackedField<Nature?> Nature { get; } = new(); // Nature [Implicit]
    }
    public string Origin_Game_Name { get; set; } // Game Name (string form of Origin Game)
    protected int[] moveIndices; //indices of the chosen moves in the pku
    protected Gender? gender;
    protected bool legalGen3Egg;


    /* ------------------------------------
     * Tag Processing Methods
     * ------------------------------------
    */
    // Species
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessSpecies()
    {
        (this as Species_E).ProcessSpeciesBase();
        Data.HasSpecies.ValueAsBool = true;
    }

    // Gender [Implicit]
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessGender()
    {
        GenderRatio gr = pkxUtil.GetGenderRatio(pku);
        bool onlyOneGender = gr is GenderRatio.All_Genderless or GenderRatio.All_Female or GenderRatio.All_Male;

        Alert alert;
        if (pku.Gender is null && !onlyOneGender) //unspecified and has more than one possible gender
            alert = GetGenderAlert(AlertType.UNSPECIFIED);
        else if (pku.Gender.ToEnum<Gender>() is null && !onlyOneGender)
            alert = GetGenderAlert(AlertType.INVALID, null, pku.Gender);
        else
            (gender, alert) = pkxUtil.ExportTags.ProcessGender(pku);

        Warnings.Add(alert);
    }

    // Form [Implicit]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessSpecies))]
    protected virtual void ProcessForm()
    {
        (this as Form_E).ProcessFormBase();

        Alert alert = null; //to return
        BigInteger speciesIndex = Data.Species.Value;
        BigInteger formIndex = workingVars.Form.Value;
        if (speciesIndex == 410) //deoxys
            alert = GetFormAlert(AlertType.NONE, null, true);
        else if (speciesIndex == 385 && formIndex != 0) //castform
            alert = GetFormAlert(AlertType.IN_BATTLE, pku.Forms.Value);
        Warnings.Add(alert);
    }

    // PID [Requires: Gender, Form, Nature, TID] [ErrorResolver]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessGender), nameof(ProcessForm),
                                                "ProcessNature", "ProcessTID")]
    protected virtual void ProcessPID()
    {
        int? unownForm = Data.Species.Value == 201 ? workingVars.Form.GetAs<int>() : null;
        var (pids, alert) = pkxUtil.ExportTags.ProcessPID(pku, Data.TID.GetAs<uint>(), false,
            gender, workingVars.Nature.Value, unownForm);
        BigInteger[] castedPids = Array.ConvertAll(pids, x => x.ToBigInteger());
        PIDResolver = new(alert, Data.PID, castedPids);
        if (alert is RadioButtonAlert)
            Errors.Add(alert);
        else
            Warnings.Add(alert);
    }

    // Met Location [Requires: Origin Game]
    [PorterDirective(ProcessingPhase.FirstPass, "ProcessOrigin_Game")]
    protected virtual void ProcessMetLocation()
    {
        var (location, alert) = pkxUtil.ExportTags.ProcessMetLocation(pku, Origin_Game_Name);
        Data.Met_Location.SetAs(location);
        Warnings.Add(alert);
    }

    // Egg [Requires: Origin Game]
    [PorterDirective(ProcessingPhase.FirstPass, "ProcessOrigin_Game")]
    protected virtual void ProcessEgg()
    {
        Data.Is_Egg.ValueAsBool = pku.IsEgg();

        //Deal with "Legal Gen 3 eggs"
        if (pku.IsEgg() && Data.Origin_Game.Value != 0)
        {
            Language? lang = DataUtil.ToEnum<Language>(pku.Game_Info?.Language);
            if (lang is not null && pkxUtil.EGG_NICKNAME[lang.Value] == pku.Nickname)
            {
                Data.UseEggName.ValueAsBool = true;
                legalGen3Egg = true;
            }
        }
    }

    // Language [Requires: Egg]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessEgg))]
    protected virtual void ProcessLanguage()
    {
        if (legalGen3Egg)
            Data.Language.SetAs(Language.Japanese);
        else
            (this as Language_E).ProcessLanguageBase();
    }

    // Nickname [Requires: Language]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessLanguage))]
    protected virtual void ProcessNickname()
    {
        if (legalGen3Egg)
            Data.Nickname.SetAs(DexUtil.CharEncoding<byte>.Encode
                (pkxUtil.EGG_NICKNAME[Data.Language.GetAs<Language>()], pk3Object.MAX_NICKNAME_CHARS, FormatName, Data.Language.GetAs<Language>()).encodedStr);
        else
        {
            var (nick, alert, _, _) = pkxUtil.ExportTags.ProcessNickname<byte>(pku, 3, pk3Object.MAX_NICKNAME_CHARS, FormatName, Data.Language.GetAs<Language>());
            Data.Nickname.SetAs(nick);
            Warnings.Add(alert);
        }
    }

    // OT [Requires: Language]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessLanguage))]
    protected virtual void ProcessOT()
    {
        if (legalGen3Egg)
            Data.OT.SetAs(DexUtil.CharEncoding<byte>.Encode
                (pku.Game_Info?.OT, pk3Object.MAX_OT_CHARS, FormatName, Data.Language.GetAs<Language>()).encodedStr);
        else
        {
            var (ot, alert) = pkxUtil.ExportTags.ProcessOT<byte>(pku, pk3Object.MAX_OT_CHARS, FormatName, Data.Language.GetAs<Language>());
            Data.OT.SetAs(ot);
            Warnings.Add(alert);
        }
    }

    // Trash Bytes [Requires: Nickname, OT]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessNickname), nameof(ProcessOT))]
    protected virtual void ProcessTrashBytes()
    {
        pkuObject.Trash_Bytes_Class tb = pku?.Trash_Bytes;
        if (tb is not null)
        {
            ushort[] nicknameTrash = tb?.Nickname?.Length > 0 ? tb.Nickname : null;
            ushort[] otTrash = tb?.OT?.Length > 0 ? tb.OT : null;
            var (nick, ot, alert) = pkxUtil.ExportTags.ProcessTrash(Data.Nickname.GetAs<byte>(), nicknameTrash, Data.OT.GetAs<byte>(), otTrash, FormatName, Data.Language.GetAs<Language>());
            Data.Nickname.SetAs(nick);
            Data.OT.SetAs(ot);
            Warnings.Add(alert);
        }
    }

    // Markings
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessMarkings()
    {
        HashSet<Marking> markings = pku.Markings.ToEnumSet<Marking>();
        Data.MarkingCircle.ValueAsBool = markings.Contains(Marking.Blue_Circle);
        Data.MarkingSquare.ValueAsBool = markings.Contains(Marking.Blue_Square);
        Data.MarkingTriangle.ValueAsBool = markings.Contains(Marking.Blue_Triangle);
        Data.MarkingHeart.ValueAsBool = markings.Contains(Marking.Blue_Heart);
    }

    // Experience [ErrorResolver]
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessExperience()
    {
        var (options, alert) = pkxUtil.ExportTags.ProcessEXP(pku);
        BigInteger[] castedOptions = Array.ConvertAll(options, x => x.ToBigInteger());
        ExperienceResolver = new(alert, Data.Experience, castedOptions);
        if (alert is RadioButtonAlert)
            Errors.Add(alert);
        else
            Warnings.Add(alert);
    }

    // Moves
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessMoves()
    {
        (int[] moves, moveIndices, Alert alert) = pkxUtil.ExportTags.ProcessMoves(pku, FormatName);
        Data.Moves.SetAs(moves);
        Warnings.Add(alert);
    }

    // PP-Ups [Requires: Moves]
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessPPUps()
    {
        var (ppups, alert) = pkxUtil.ExportTags.ProcessPPUps(pku, moveIndices);
        Data.PP_Ups.SetAs(ppups);
        Warnings.Add(alert);
    }

    // PP [Requires: Moves, PP-Ups]
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessPP()
    {
        int[] pp = new int[4];
        for (int i = 0; i < moveIndices.Length; i++)
            pp[i] = pkxUtil.CalculatePP(pku.Moves[moveIndices[i]].Name, Data.PP_Ups.GetAs<byte>(i), FormatName);
        Data.PP.SetAs(pp);
    }

    // Pokérus
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessPokerus()
    {
        var (strain, days, alert) = pkxUtil.ExportTags.ProcessPokerus(pku);
        Data.PKRS_Strain.SetAs(strain);
        Data.PKRS_Days.SetAs(days);
        Warnings.Add(alert);
    }

    // Ability Slot
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessAbilitySlot()
    {
        Alert alert = null;
        string[] abilitySlots = SPECIES_DEX.ReadDataDex<string[]>(pku.Species, "Gen 3 Ability Slots");
        if (pku.Ability is null) //ability unspecified
        {
            Data.Ability_Slot.ValueAsBool = false;
            alert = pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.UNSPECIFIED, pku.Ability, abilitySlots[0]);
        }
        else //ability specified
        {
            int? abilityID = PokeAPIUtil.GetAbilityIndex(pku.Ability);
            if (abilityID is null or > 76) //unofficial ability OR gen4+ ability
            {
                Data.Ability_Slot.ValueAsBool = false;
                alert = pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.INVALID, pku.Ability, abilitySlots[0]);
            }
            else //gen 3- ability
            {
                bool isSlot1 = abilityID == PokeAPIUtil.GetAbilityIndex(abilitySlots[0]);
                bool isSlot2 = abilitySlots.Length > 1 && abilityID == PokeAPIUtil.GetAbilityIndex(abilitySlots[1]);
                Data.Ability_Slot.ValueAsBool = isSlot2; //else false (i.e. slot 1, or invalid)

                if (!isSlot1 && !isSlot2) //ability is impossible on this species, alert
                    alert = pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.MISMATCH, pku.Ability, abilitySlots[0]);
            }
        }
        Warnings.Add(alert);
    }

    // Ribbons
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessRibbons()
    {
        (HashSet<Ribbon> ribbons, Alert a) = pkxUtil.ExportTags.ProcessRibbons(pku, pk3Object.IsValidRibbon);

        //In other words, if the pku has a contest ribbon at level x, but not at level x-1 (when x-1 exists).
        if (ribbons.Contains(Ribbon.Cool_Super_G3) && !ribbons.Contains(Ribbon.Cool_G3) ||
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
            ribbons.Contains(Ribbon.Tough_Master_G3) && !ribbons.Contains(Ribbon.Tough_Hyper_G3))
            a = AddContestRibbonAlert(a);

        //Add contest ribbons
        Data.Cool_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Cool_G3, ribbons));
        Data.Beauty_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Beauty_G3, ribbons));
        Data.Cute_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Cute_G3, ribbons));
        Data.Smart_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Smart_G3, ribbons));
        Data.Tough_Ribbon_Rank.SetAs(pk3Object.GetRibbonRank(Ribbon.Tough_G3, ribbons));

        //Add other ribbons
        Data.Champion_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Champion);
        Data.Winning_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Winning);
        Data.Victory_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Victory);
        Data.Artist_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Artist);
        Data.Effort_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Effort);
        Data.Battle_Champion_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Battle_Champion);
        Data.Regional_Champion_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Regional_Champion);
        Data.National_Champion_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.National_Champion);
        Data.Country_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Country);
        Data.National_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.National);
        Data.Earth_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.Earth);
        Data.World_Ribbon.ValueAsBool = ribbons.Contains(Ribbon.World);

        Warnings.Add(a);
    }

    // Fateful Encounter [ErrorResolver]
    [PorterDirective(ProcessingPhase.FirstPass, "ProcessSpecies")]
    protected virtual void ProcessFatefulEncounter()
    {
        Alert alert = null;
        bool[] options;
        int speciesIndex = Data.Species.GetAs<int>();
        if (speciesIndex is 151 or 410 && pku.Catch_Info?.Fateful_Encounter is not true) //Mew or Deoxys w/ no fateful encounter
        {
            options = new bool[] { false, true };
            alert = GetFatefulEncounterAlert(speciesIndex is 151);
        }
        else
            options = new bool[] { pku.Catch_Info?.Fateful_Encounter is true };

        FatefulEncounterResolver = new(alert, Data.Fateful_Encounter, options);
        if (alert is RadioButtonAlert)
            Errors.Add(alert);
        else
            Warnings.Add(alert);
    }

    // Byte Override [ErrorResolver]
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessByteOverride()
    {
        (Alert a, ByteOverrideAction) = pkxUtil.MetaTags.ApplyByteOverride(pku, Data.NonSubData, Data.G, Data.A, Data.E, Data.M);
        Warnings.Add(a);
    }


    /* ------------------------------------
     * Error Resolvers
     * ------------------------------------
    */
    // PID ErrorResolver
    [PorterDirective(ProcessingPhase.SecondPass)]
    protected virtual ErrorResolver<BigInteger> PIDResolver { get; set; }

    // Experience ErrorResolver
    [PorterDirective(ProcessingPhase.SecondPass)]
    protected virtual ErrorResolver<BigInteger> ExperienceResolver { get; set; }

    // Fateful Encounter ErrorResolver
    [PorterDirective(ProcessingPhase.SecondPass)]
    protected virtual ErrorResolver<bool> FatefulEncounterResolver { get; set; }


    /* ------------------------------------
     * Post-Processing Methods
     * ------------------------------------
    */
    // Byte Override Action
    [PorterDirective(ProcessingPhase.PostProcessing)]
    protected virtual Action ByteOverrideAction { get; set; }


    /* ------------------------------------
     * pk3 Exporting Alerts
     * ------------------------------------
    */
    // Adds gen 3 contest ribbon alert to an existing pkxUtil ribbon alert (or null), if needed.
    public static Alert AddContestRibbonAlert(Alert ribbonAlert)
    {
        string msg = "This pku has a Gen 3 contest ribbon of some category with rank super or higher, " +
            "but doesn't have the ribbons below that rank. This is impossible in this format, adding those ribbons.";
        if (ribbonAlert is not null)
            ribbonAlert.Message += DataUtil.Newline(2) + msg;
        else
            ribbonAlert = new Alert("Ribbons", msg);
        return ribbonAlert;
    }

    public static Alert GetNatureAlert(AlertType at, string val, string defaultVal) => at switch
    {
        AlertType.UNSPECIFIED => new Alert("Nature", "No nature specified, using the nature decided by the PID."),
        AlertType.INVALID => new Alert("Nature", $"The nature \"{val}\" is not valid in this format. Using the nature decided by the PID."),
        _ => EnumTag_E.GetEnumAlert("Nature", at, val, defaultVal)
    };

    // Changes the UNSPECIFIED & INVALID AlertTypes from the pkxUtil method to account for PID-Nature dependence.
    public static Alert GetGenderAlert(AlertType at, Gender? correctGender = null, string invalidGender = null) => at switch
    {
        AlertType.UNSPECIFIED => new Alert("Gender", "No gender specified, using the gender decided by the PID."),
        AlertType.INVALID => new Alert("Gender", $"The gender \"{invalidGender}\" is not valid in this format. Using the gender decided by the PID."),
        _ => pkxUtil.ExportAlerts.GetGenderAlert(at, correctGender, invalidGender)
    };

    public static RadioButtonAlert GetFatefulEncounterAlert(bool isMew)
    {
        string pkmn = isMew ? "Mew" : "Deoxys";
        string msg = $"This {pkmn} was not met in a fateful encounter. " +
            $"Note that, in the Gen 3 games, {pkmn} will only obey the player if it was met in a fateful encounter.";

        RadioButtonAlert.RBAChoice[] choices =
        {
            new("Keep Fateful Encounter",$"Fateful Encounter: false\n{pkmn} won't obey."),
            new("Set Fateful Encounter",$"Fateful Encounter: true\n{pkmn} will obey.")
        };

        return new RadioButtonAlert("Fateful Encounter", msg, choices);
    }

    public static Alert GetFormAlert(AlertType at, string[] invalidForm = null, bool isDeoxys = false)
        => isDeoxys ? new Alert("Form", "Note that in generation 3, Deoxys' form depends on what game it is currently in.")
                    : pkxUtil.ExportAlerts.GetFormAlert(at, invalidForm);


    /* ------------------------------------
     * Duct Tape
     * ------------------------------------
    */
    Species_O Species_E.Data => Data;
    Form_O Form_E.Data => workingVars;
    Item_O Item_E.Data => Data;
    Nature_O Nature_E.Data => workingVars;
    Friendship_O Friendship_E.Data => Data;
    TID_O TID_E.Data => Data;
    IVs_O IVs_E.Data => Data;
    EVs_O EVs_E.Data => Data;
    Contest_Stats_O Contest_Stats_E.Data => Data;
    Ball_O Ball_E.Data => Data;
    Origin_Game_O Origin_Game_E.Data => Data;
    Met_Level_O Met_Level_E.Data => Data;
    OT_Gender_O OT_Gender_E.Data => Data;
    Language_O Language_E.Data => Data;
    protected partial class WorkingVariables : Nature_O, Form_O
    {
        OneOf<IField<BigInteger>, IField<string>> Form_O.Form => Form;
        OneOf<IField<BigInteger>, IField<Nature>, IField<Nature?>> Nature_O.Nature => Nature;
    }
}