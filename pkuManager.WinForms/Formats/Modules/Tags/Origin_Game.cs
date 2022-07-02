using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Utilities;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Origin_Game_O
{
    public IIntField Origin_Game { get; }
}

public interface Origin_Game_E : Tag
{
    public bool UseOfficialValues => false;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportOrigin_Game()
    {
        Origin_Game_O originGameObj = Data as Origin_Game_O;

        AlertType at = AlertType.NONE;
        string game = pku.GameField(UseOfficialValues).Value;

        if (game is null) //unspecified
            at = AlertType.UNSPECIFIED;
        else //specified
        {
            if (!GAME_DEX.ExistsIn(FormatName, game)) //invalid
                at = AlertType.INVALID;
            else // success
                //if game exists and format has a field, indices must exist
                originGameObj.Origin_Game.Value = GAME_DEX.GetIndexedValue<int?>(FormatName, game, "Indices").Value;
        }
        Warnings.Add(GetOrigin_GameAlert(at, game));
    }

    protected static Alert GetOrigin_GameAlert(AlertType at, string game)
    {
        if (at is AlertType.NONE)
            return null;

        string msg;
        if (at.HasFlag(AlertType.UNSPECIFIED))
            msg = "The origin game was unspecified.";
        else if (at.HasFlag(AlertType.INVALID))
            msg = $"The origin game '{game}' does not exist in this format.";
        else
            throw InvalidAlertType(at);

        return new("Origin Game", msg + "Using the default of None.");
    }
}