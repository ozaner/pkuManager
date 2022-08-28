using pkuManager.Data.Dexes;
using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.pku;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Origin_Game_O
{
    public IIntField Origin_Game { get; }
}

public interface Origin_Game_E : Tag
{
    public string Origin_Game_Name { set; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportOrigin_Game()
    {
        Origin_Game_O originGameObj = Data as Origin_Game_O;

        AlertType at = AlertType.NONE;
        string game = pkuObject.ChooseField(true, pku.Game_Info.Origin_Game, pku.Game_Info.Official_Origin_Game).Value;

        if (game is null) //unspecified
            at = AlertType.UNSPECIFIED;
        else //specified
        {
            //Only supports int type game indices
            if (!DDM.TryGetGameID(FormatName, game, out int ID)) //invalid
                at = AlertType.INVALID;
            else // success
                //if game exists and format has a game field, indices must exist
                originGameObj.Origin_Game.Value = ID;
        }

        //set origin game name
        if (at is AlertType.NONE)
            Origin_Game_Name = game;

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