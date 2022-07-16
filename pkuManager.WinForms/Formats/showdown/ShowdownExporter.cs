using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Modules.MetaTags;
using pkuManager.WinForms.Formats.Modules.Tags;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.showdown;

/// <summary>
/// Exports a <see cref="pkuObject"/> to a <see cref="ShowdownObject"/>.
/// </summary>
public class ShowdownExporter : Exporter, BattleStatOverride_E, FormCasting_E, SFA_E, Shiny_E,
                                Nickname_E, Level_E, Gender_E, Ability_E, Moves_E, Item_E,
                                Nature_E, Friendship_E, IVs_E, EVs_E, Gigantamax_Factor_E
{
    public override string FormatName => "Showdown";
    public override ShowdownObject Data { get; } = new();

    /// <summary>
    /// Creates an exporter that will attempt to export <paramref name="pku"/>
    /// to a .txt (Showdown!) file, encoded in UTF-8, with the given <paramref name="globalFlags"/>.
    /// </summary>
    /// <inheritdoc cref="Exporter(pkuObject, GlobalFlags, FormatObject)"/>
    public ShowdownExporter(pkuObject pku, GlobalFlags globalFlags, bool checkMode) : base(pku, globalFlags, checkMode)
    {
        // Screen Species & Form
        if (DexUtil.FirstFormInFormat(pku, FormatName, true, GlobalFlags.Default_Form_Override) is null)
            Reason = "Must be a species & form that exists in Showdown.";

        // Screen Shadow Pokemon
        else if (pku.IsShadow())
            Reason = "This format doesn't support Shadow Pokémon.";

        //Showdown doesn't support eggs (they can't exactly battle...)
        else if (pku.IsEgg())
            Reason = "Cannot be an Egg.";
    }


    /* ------------------------------------
     * Working Variables
     * ------------------------------------
    */
    public string[] Moves_Indices { get; set; }


    /* ------------------------------------
     * Exporting Parameters
     * ------------------------------------
    */
    public bool IVs_AlertIfUnspecified => false;


    /* ------------------------------------
     * Custom Processing Methods
     * ------------------------------------
    */
    // Nickname
    public void ExportNickname()
    {
        // Notes:
        //  - Practically no character limit
        //  - Can use parenthesis (only checks at the end of first line)
        //  - Empty nickname interpreted as no nickname
        //  - Leading spaces are ignored

        (this as Nickname_E).ExportNicknameBase();
        if (Data.Nickname.Value?.Length > 0 && Data.Nickname.Value[0] is ' ') //if first character is a space
            Warnings.Add(GetNicknameAlert());
    }

    // PP Ups
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Moves_E.ExportMoves))]
    public void ExportPP_Ups()
    {
        bool invalid = false;
        foreach(var id in Moves_Indices)
        {
            var pp = pku.Moves[id].PP_Ups.Value;
            invalid |= (pp != null && pp != 3);
        }
        Warnings.Add(GetPP_UpAlert());
    }


    /* ------------------------------------
     * Custom Alerts
     * ------------------------------------
    */
    public static Alert GetNicknameAlert()
        => new("Nickname", $"Showdown does not recoginize leading spaces in nicknames.");

    public static Alert GetPP_UpAlert()
        => new("PP Ups", "Note that, even though one or more moves does not have exactly 3 PP Ups," +
            "the Showdown format treats all moves as having 3 PP Ups.");

    public Alert GetAbilityAlert(AlertType at)
    {
        Alert a = (this as Ability_E).GetAbilityAlertBase(at);
        if (a is not null)
            a.Message += " (Showdown will pick a legal one).";
        return a;
    }
}