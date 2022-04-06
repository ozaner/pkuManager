using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.Modules;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Formats.Modules.Templates;

namespace pkuManager.Formats.pkx.pk3;

/// <summary>
/// Exports a <see cref="pkuObject"/> to a <see cref="pk3Object"/>.
/// </summary>
public class pk3Exporter : Exporter, BattleStatOverride_E, FormCasting_E, Species_E, Form_E,
                           Gender_E, Nickname_E, Experience_E, Moves_E, PP_Ups_E, PP_E, Item_E,
                           Nature_E, Friendship_E, PID_E, TID_E, IVs_E, EVs_E, Contest_Stats_E,
                           Ball_E, Encoded_OT_E, Origin_Game_E, Met_Location_E, Met_Level_E,
                           OT_Gender_E, Language_E, Fateful_Encounter_E, Markings_E, Ribbons_E,
                           Is_Egg_E, Pokerus_E, Trash_Bytes_E, ByteOverride_E
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


    /* ------------------------------------
     * Working Variables
     * ------------------------------------
    */
    protected bool legalGen3Egg;
    protected ImplicitFields implicitFields = new();
    protected partial class ImplicitFields
    {
        public BackedBoundableField<BigInteger> Form { get; } = new(); // Form [Implicit]
        public BackedField<Gender?> Gender { get; } = new(); //Gender [Implicit]
        public BackedField<Nature?> Nature { get; } = new(); // Nature [Implicit]
    }


    /* ------------------------------------
     * Custom Processing Methods
     * ------------------------------------
    */
    // Species
    [PorterDirective(ProcessingPhase.FirstPass)]
    public virtual void ProcessSpecies()
    {
        (this as Species_E).ProcessSpeciesBase();
        Data.HasSpecies.ValueAsBool = true;
    }

    // Egg [Requires: Origin Game]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Origin_Game_E.ProcessOrigin_Game))]
    public virtual void ProcessIs_Egg()
    {
        (this as Is_Egg_E).ProcessIs_EggBase();

        //Deal with "Legal Gen 3 eggs"
        if (pku.IsEgg())
        {
            //To be seen as legal must have no nickname or a defined language + matching "Egg" nickname.
            Language_Field.AsString = pku.Game_Info.Language.Value;
            if (pku.Nickname.IsNull() || Language_Field.IsValid && TagUtil.EGG_NICKNAME[Language_Field.AsString] == pku.Nickname.Value)
            {
                Data.UseEggName.ValueAsBool = true;
                legalGen3Egg = true;
            }
        }
    }

    // Language [Requires: Egg]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessIs_Egg))]
    public virtual void ProcessLanguage()
    {
        if (legalGen3Egg)
            Language_Field.AsString = "Japanese";
        else
            (this as Language_E).ProcessLanguageBase();
    }

    // Nickname [Requires: Language]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessLanguage))]
    public virtual void ProcessNickname()
    {
        if (legalGen3Egg)
            Data.Nickname.Value = DexUtil.CharEncoding.Encode(TagUtil.EGG_NICKNAME["Japanese"],
                pk3Object.MAX_NICKNAME_CHARS, FormatName, "Japanese").encodedStr;
        else
            (this as Nickname_E).ProcessNicknameBase();
    }

    // OT [Requires: Language]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessLanguage))]
    public virtual void ProcessOT()
    {
        if (legalGen3Egg) //encode legal egg OTs in their proper lang
            Language_Field.AsString = pku.Game_Info.Language.Value;

        (this as Encoded_OT_E).ProcessOTBase();
        
        if (legalGen3Egg) //return legal egg lang back to japanese
            Language_Field.AsString = "Japanese";
    }

    // Ability Slot
    [PorterDirective(ProcessingPhase.FirstPass)]
    public virtual void ProcessAbilitySlot()
    {
        int getGen3AbilityID(string name)
            => ABILITY_DEX.GetIndexedValue<int?>(FormatName, name, "Indices").Value; //must have an ID

        Alert alert = null;
        string[] abilitySlots = SPECIES_DEX.ReadDataDex<string[]>(pku.Species.Value, "Gen 3 Ability Slots"); //must exist
        if (pku.Ability.IsNull()) //ability unspecified
        {
            Data.Ability_Slot.ValueAsBool = false;
            alert = GetAbilitySlotAlert(AlertType.UNSPECIFIED, null, abilitySlots[0]);
        }
        else //ability specified
        {
            if (ABILITY_DEX.ExistsIn(FormatName, pku.Ability.Value)) //gen 3 ability
            {
                int abilityID = getGen3AbilityID(pku.Ability.Value);
                bool isSlot1 = abilityID == getGen3AbilityID(abilitySlots[0]);
                bool isSlot2 = abilitySlots.Length > 1 && abilityID == getGen3AbilityID(abilitySlots[1]);
                Data.Ability_Slot.ValueAsBool = isSlot2; //else false (i.e. slot 1, or invalid)

                if (!isSlot1 && !isSlot2) //ability is impossible on this species, alert
                    alert = GetAbilitySlotAlert(AlertType.MISMATCH, pku.Ability.Value, abilitySlots[0]);
            }
            else //unofficial ability/gen 4+ ability
            {
                Data.Ability_Slot.ValueAsBool = false;
                alert = GetAbilitySlotAlert(AlertType.INVALID, pku.Ability.Value, abilitySlots[0]);
            }
        }
        Warnings.Add(alert);
    }

    // Ribbons
    [PorterDirective(ProcessingPhase.FirstPass)]
    public virtual void ProcessRibbons()
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

        (this as Ribbons_E).ProcessRibbonsBase(); //Process other ribbons
    }

    // Fateful Encounter [ErrorResolver]
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessSpecies))]
    public virtual void ProcessFateful_Encounter()
    {
        int speciesIndex = Data.Species.GetAs<int>();

        //Mew or Deoxys w/ no fateful encounter
        if (speciesIndex is 151 or 410 && pku.Catch_Info.Fateful_Encounter.Value is not true)
        {
            Alert alert = GetObedienceAlert(speciesIndex is 151);
            Fateful_EncounterResolver = new(alert, Data.Fateful_Encounter, new[] { false, true });
            Errors.Add(alert);
        }
        else
            (this as Fateful_Encounter_E).ProcessFateful_EncounterBase();
    }


    /* ------------------------------------
     * Error Resolvers
     * ------------------------------------
    */
    [PorterDirective(ProcessingPhase.SecondPass)]
    protected virtual ErrorResolver<bool> Fateful_EncounterResolver { get; set; }

    public ErrorResolver<BigInteger> Experience_Resolver { get; set; }
    public ErrorResolver<BigInteger> PID_Resolver { get; set; }
    public Action ByteOverride_Action { get; set; }


    /* ------------------------------------
     * Custom Alerts
     * ------------------------------------
    */
    public Alert GetFormAlert(DexUtil.SFA sfa)
    {
        if (sfa.Species == "Deoxys" && sfa.Form != "Normal") //must be another valid deoxys form to get here
            return new("Form", "Note that in generation 3, Deoxys' form depends on what game it is currently in.");
        else if (sfa.Species == "Castform" && sfa.Form != "Normal")
            return new("Form", "Note that in generation 3, Castform can only be in its 'Normal' form out of battle.");
        else
            return null;
    }

    public Alert GetNatureAlert(AlertType at, string val, string defaultVal) => at switch
    {
        AlertType.UNSPECIFIED => new Alert("Nature", "No nature specified, using the nature decided by the PID."),
        AlertType.INVALID => new Alert("Nature", $"The nature \"{val}\" is not valid in this format. Using the nature decided by the PID."),
        _ => (this as Nature_E).GetNatureAlertBase(at, val, defaultVal)
    };

    public static Alert GetAbilitySlotAlert(AlertType at, string val, string defaultVal)
    {
        if (at.HasFlag(AlertType.MISMATCH))
            return new("Ability", $"This species cannot have the ability '{val}' in this format. Using the default ability: '{defaultVal}'.");
        else
            return IndexTag_E.GetIndexAlert("Ability", at, val, defaultVal);
    }

    public static Alert GetContestRibbonAlert()
        => new("Ribbons", "This pku has a Gen 3 contest ribbon of some category with rank super or higher, " +
            "but doesn't have the ribbons below that rank. This is impossible in this format, adding those ribbons.");

    public static Alert GetObedienceAlert(bool isMew)
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


    /* ------------------------------------
     * Module Parameters
     * ------------------------------------
    */
    public Species_O Species_Field => Data;
    public Form_O Form_Field => implicitFields;
    public Experience_O Experience_Field => Data;

    public Gender_O Gender_Field => implicitFields;
    public bool Gender_DisallowImpossibleGenders => true;
    public bool Gender_PIDDependent => true;

    public Nickname_O Nickname_Field => Data;
    public bool Nickname_CapitalizeDefault => true;

    public Moves_O Moves_Field => Data;
    public int[] Moves_Indices { get; set; } //indices of the chosen moves in the pku

    public PP_Ups_O PP_Ups_Field => Data;
    public PP_O PP_Field => Data;
    public Item_O Item_Field => Data;
    public Nature_O Nature_Field => implicitFields;
    public Friendship_O Friendship_Field => Data;

    public PID_O PID_Field => Data;
    public bool PID_GenderDependent => true;
    public bool PID_UnownFormDependent => true;
    public bool PID_NatureDependent => true;
    public bool PID_Gen6ShinyOdds => false;

    public TID_O TID_Field => Data;
    public IVs_O IVs_Field => Data;
    public EVs_O EVs_Field => Data;
    public Contest_Stats_O Contest_Stats_Field => Data;
    public Ball_O Ball_Field => Data;

    public Origin_Game_O Origin_Game_Field => Data;
    public string Origin_Game_Name { get; set; } // Game Name (string form of Origin Game)

    public Encoded_OT_O OT_Field => Data;
    public Met_Location_O Met_Location_Field => Data;
    public Met_Level_O Met_Level_Field => Data;
    public OT_Gender_O OT_Gender_Field => Data;
    public Language_O Language_Field => Data;

    public Fateful_Encounter_O Fateful_Encounter_Field => Data;
    public Markings_O Markings_Field => Data;
    public Ribbons_O Ribbons_Field => Data;
    public Is_Egg_O Is_Egg_Field => Data;
    public Pokerus_O Pokerus_Field => Data;
    public ByteOverride_O ByteOverride_Field => Data;


    /* ------------------------------------
     * Duct Tape
     * ------------------------------------
    */
    protected partial class ImplicitFields : Form_O, Gender_O, Nature_O
    {
        OneOf<IField<BigInteger>, IField<string>> Form_O.Form => Form;
        OneOf<IField<BigInteger>, IField<Gender>, IField<Gender?>> Gender_O.Gender => Gender;
        OneOf<IField<BigInteger>, IField<Nature>, IField<Nature?>> Nature_O.Nature => Nature;
    }
}