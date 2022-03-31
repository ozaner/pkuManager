using pkuManager.Alerts;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.showdown;

/// <summary>
/// Exports a <see cref="pkuObject"/> to a <see cref="ShowdownObject"/>.
/// </summary>
public class ShowdownExporter : Exporter, BattleStatOverride_E, FormCasting_E, Species_E,
                                Form_E, Shiny_E, Nickname_E, Level_E, Gender_E, Ability_E, Moves_E,
                                Item_E, Nature_E, Friendship_E, IVs_E, EVs_E, Gigantamax_Factor_E
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
     * Custom Processing Methods
     * ------------------------------------
    */
    // Nickname
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessNickname()
    {
        // Notes:
        //  - Practically no character limit
        //  - Can use parenthesis (only checks at the end of first line)
        //  - Empty nickname interpreted as no nickname
        //  - Leading spaces are ignored

        (this as Nickname_E).ProcessNicknameBase();
        if (Data.Nickname.Value?.Length > 0 && Data.Nickname.Value[0] is ' ') //if first character is a space
            Warnings.Add(GetNicknameAlert());
    }


    /* ------------------------------------
     * Custom Alerts
     * ------------------------------------
    */
    public static Alert GetNicknameAlert()
        => new("Nickname", $"Showdown does not recoginize leading spaces in nicknames.");

    public Alert GetNatureAlert(AlertType at, string val, string defaultVal)
    {
        Alert a = (this as Nature_E).GetNatureAlertBase(at, val, defaultVal);
        if (at.HasFlag(AlertType.UNSPECIFIED))
            a.Message += " (Showdown treats no nature as Serious).";
        return a;
    }


    /* ------------------------------------
     * Module Parameters 
     * ------------------------------------
    */
    public Species_O Species_Field => Data;
    public Form_O Form_Field => Data;
    public Shiny_O Shiny_Field => Data;
    public Gender_O Gender_Field => Data;

    public Ability_O Ability_Field => Data;
    public string Ability_Default => "None (Showdown will pick one)";

    public Nickname_O Nickname_Field => Data;
    public Level_O Level_Field => Data;

    public Moves_O Moves_Field => Data;
    public int[] Moves_Indices { set { } } //don't need these

    public Item_O Item_Field => Data;
    public Nature_O Nature_Field => Data;

    public Friendship_O Friendship_Field => Data;
    public int Friendship_Default => 255;

    public IVs_O IVs_Field => Data;
    public int IVs_Default => 31;
    public bool IVs_AlertIfUnspecified => false;
    
    public EVs_O EVs_Field => Data;
    public Gigantamax_Factor_O Gigantamax_Factor_Field => Data;
}