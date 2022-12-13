using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.MetaTags;
using pkuManager.WinForms.Formats.Modules.Tags;
using pkuManager.WinForms.Formats.Modules.Templates;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;
using System.Numerics;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.essentials.uranium;

public class pkeUraniumExporter : Exporter, SFA_E, Nickname_E, Experience_E, Ability_Slot_E,
                                  Gender_E, Nature_E, Shiny_E, Friendship_E, Moves_E, PP_Ups_E,
                                  Relearn_Moves_E, Ball_E, Item_E, EVs_E, IVs_E, Pokerus_E,
                                  Markings_E, PID_E, TID_E, OT_E, OT_Gender_E, Met_Location_E,
                                  Met_Date_E, Met_Level_E, Egg_Location_E, Egg_Date_E,
                                  Egg_Steps_E, Language_E
{
    public override string FormatName => "pkeUranium";
    public override pkeUraniumObject Data { get; } = new();

    public pkeUraniumExporter(pkuObject pku, GlobalFlags globalFlags, bool checkMode) : base(pku, globalFlags, checkMode)
    {
        // Screen Species & Form
        if (FormCastingUtil.GetCastedForm(pku, FormatName,
            GlobalFlags.Default_Form_Override).fcs is FormCastingUtil.FormCastStatus.DNE)
            Reason = "Must be a species & form that exists in Uranium.";

        // Screen Shadow Pokemon
        else if (this.pku.IsShadow())
            Reason = "This format doesn't support Shadow Pokémon.";
    }


    /* ------------------------------------
     * Working Variables
     * ------------------------------------
    */
    public string[] Moves_Indices { get; set; }
    public string Origin_Game_Name => "Uranium"; //only supports Uranium
    protected bool Met_LocationOverrideOccured = false;


    /* ------------------------------------
     * Module Parameters
     * ------------------------------------
    */
    public bool Relearn_Moves_ExportWholeMovepool => true;


    /* ------------------------------------
     * Custom Processing Methods
     * ------------------------------------
    */
    //Primary Location Override
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Met_Location_E.ExportMet_Location),
                                                nameof(Egg_Location_E.ExportEgg_Location))]
    public void ExportPrimaryLocationOverride()
    {
        if (Data.Egg_Location.Value != 0) //egg/hatched and official used
        {
            if (!pku.Egg_Info.Official_Received_Location.IsNull())
                Data.Primary_Location_Override.Value = pku.Egg_Info.Received_Location.Value;
        }
        else //not egg/hatched and official used.
        {
            //Offical used (so regular can be overlayed) or met location invalid (use literal).
            if (!pku.Catch_Info.Official_Met_Location.IsNull() || Data.Met_Location.Value == 0)
            {
                Data.Primary_Location_Override.Value = pku.Catch_Info.Met_Location.Value;
                Met_LocationOverrideOccured = true; //to skip alerting
            }
        }

        //Special case for Day-Care Couple
        if (pku.Egg_Info.Received_Location.Value is "Day-Care Couple")
            Data.Primary_Location_Override.Value = "Day-Care Couple";
    }

    //Format Specific: "Global Language"
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ExportLanguage))]
    public void ExportGlobalLanguage()
    {
        (bool? globalLang, _) = FormatSpecificUtil.GetValue<bool?>(pku, FormatName, "Global Language");
        if (globalLang is true)
            Data.Language.Value = 6; //Global Lang Flag = 6
    }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportFeralNuclear()
        => BooleanTagUtil.ExportBooleanTag(pku.Feral_Nuclear, Data.Feral_Nuclear, false);


    /* ------------------------------------
     * Error Resolvers
     * ------------------------------------
    */
    public ErrorResolver<BigInteger> Experience_Resolver { get; set; }


    /* ------------------------------------
     * Custom Alerts
     * ------------------------------------
    */
    public Alert GetMet_LocationAlert(AlertType at, string defaultLoc, string invalidLoc)
        => Met_LocationOverrideOccured ? null : (this as Met_Location_E).GetMet_LocationAlertBase(at, defaultLoc, invalidLoc);
}
