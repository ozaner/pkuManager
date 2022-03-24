using pkuManager.Alerts;
using pkuManager.Formats.Modules;
using pkuManager.Formats.pku;
using pkuManager.Formats.pkx;
using pkuManager.Utilities;
using System;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.Modules.Nature_Util;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.showdown;

/// <summary>
/// Exports a <see cref="pkuObject"/> to a <see cref="ShowdownObject"/>.
/// </summary>
public class ShowdownExporter : Exporter, BattleStatOverride_E, FormCasting_E, Species_E, Form_E, 
                                Nickname_E, Gender_E, Moves_E, Item_E, Nature_E, Friendship_E,
                                IVs_E, EVs_E
{
    public override string FormatName => "Showdown";
    protected override ShowdownObject Data { get; } = new();

    /// <summary>
    /// Creates an exporter that will attempt to export <paramref name="pku"/>
    /// to a .txt (Showdown!) file, encoded in UTF-8, with the given <paramref name="globalFlags"/>.
    /// </summary>
    /// <inheritdoc cref="Exporter(pkuObject, GlobalFlags, FormatObject)"/>
    public ShowdownExporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

    public override (bool, string) CanPort()
    {
        // Screen Species & Form
        if (DexUtil.FirstFormInFormat(pku, FormatName, true, GlobalFlags.Default_Form_Override) is null)
            return (false, "Must be a species & form that exists in Showdown.");

        //Showdown doesn't support eggs (they can't exactly battle...).
        if (pku.IsEgg())
            return (false, "Cannot be an Egg.");

        return (true, null);
    }


    /* ------------------------------------
     * Tag Processing Methods
     * ------------------------------------
    */
    // Nickname
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessNickname()
    {
        // Notes:
        //  - Practically no character limit
        //  - Can use parenthesis (only checks at the end of first line)
        //  - Empty nickname interpreted as no nickname
        //  - Leading spaces are ignored

        (this as Nickname_E).ProcessNicknameBase();
        if (Data.Nickname.Value?.Length > 0 && Data.Nickname.Value[0] is ' ') //if first character is a space
            Warnings.Add(GetNicknameAlert(AlertType.INVALID));
    }

    // Ability
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessAbility()
    {
        bool abilityValid = ABILITY_DEX.ExistsIn(FormatName, pku.Ability);
        if (pku.Ability is not null && !abilityValid) //check for invalid alert
            Warnings.Add(pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.INVALID, pku.Ability, "None (Showdown will pick one)"));
        else
            Data.Ability = pku.Ability;
    }

    // Level
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessLevel()
    {
        var (level, alert) = pkxUtil.ExportTags.ProcessNumericTag(pku.Level, pkxUtil.ExportAlerts.GetLevelAlert, false, 100, 1, 100);
        Data.Level = (byte)level;
        Warnings.Add(alert);
    }

    // Gigantamax Factor
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessGigantamaxFactor()
        => Data.Gigantamax_Factor = pku.Gigantamax_Factor is true;


    /* ------------------------------------
     * Showdown Exporting Alerts
     * ------------------------------------
    */
    public Alert GetNicknameAlert(AlertType at, int? maxCharacters = null)
    {
        if (at.HasFlag(AlertType.INVALID))
            return new Alert("Nickname", $"Showdown does not recoginize leading spaces in nicknames.");
        throw InvalidAlertType(at);
    }

    public static Alert GetLevelAlert(AlertType at) => at switch
    {
        //override pkx's unspecified level of 1 to 100
        AlertType.UNSPECIFIED => new Alert("Level", "No level specified, using the default: 100."),
        _ => pkxUtil.ExportAlerts.GetLevelAlert(at)
    };

    public static Alert GetNatureAlert(AlertType at, string invalidNature = null)
    {
        Alert a = new("Nature", $"Using the default: None (Showdown uses Serious when no nature is specified.)");
        if (at is AlertType.INVALID)
        {
            if (invalidNature is null)
                throw new ArgumentException("If INVALID AlertType given, invalidNature must also be given.");
            a.Message = $"The Nature \"{invalidNature}\" is not valid in this format. " + a.Message;
        }
        else if (at is AlertType.UNSPECIFIED)
            a.Message = $"No nature was specified. " + a.Message;
        else
            throw InvalidAlertType(at);
        return a;
    }


    /* ------------------------------------
     * Module Parameters 
     * ------------------------------------
    */
    public Species_O Species_Field => Data;
    public Form_O Form_Field => Data;
    public Gender_O Gender_Field => Data;
    public Nickname_O Nickname_Field => Data;

    public Moves_O Moves_Field => Data;
    public int[] Moves_Indices { set { } } //don't need these

    public Item_O Item_Field => Data;

    public Nature_O Nature_Field => Data;
    public Nature? Nature_Default => null;

    public Friendship_O Friendship_Field => Data;
    public int Friendship_Default => 255;

    public IVs_O IVs_Field => Data;
    public int IVs_Default => 31;
    public bool IVs_SilentUnspecified => true;
    
    public EVs_O EVs_Field => Data;
}