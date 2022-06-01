using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

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
        
        (int? id, string game) helper(string game)
        {
            if (GAME_DEX.ExistsIn(FormatName, game))
                return (GAME_DEX.GetIndexedValue<int?>(FormatName, game, "Indices"), game);
            return (null, game);
        }

        // Init
        originGameObj.Origin_Game.Value = 0;
        Origin_Game_Name = null;

        // if both unspecified
        if (pku.Game_Info.Origin_Game.IsNull() && pku.Game_Info.Official_Origin_Game.IsNull())
            Warnings.Add(GetOrigin_GameAlert(AlertType.UNSPECIFIED));
        else // at least one specified
        {
            (int? id, string game) = helper(pku.Game_Info.Origin_Game.Value);

            if (id is null) //origin game failed, try official origin game
                (id, game) = helper(pku.Game_Info.Official_Origin_Game.Value);

            if (id is null) // if neither have an id in this format
                Warnings.Add(GetOrigin_GameAlert(AlertType.INVALID, pku.Game_Info.Origin_Game.Value, pku.Game_Info.Official_Origin_Game.Value));
            else // success
            {
                originGameObj.Origin_Game.Value = id.Value;
                Origin_Game_Name = game;
            }
        }
    }

    protected static Alert GetOrigin_GameAlert(AlertType at, string originGame = null, string officialOriginGame = null) => new("Origin Game", at switch
    {
        AlertType.UNSPECIFIED => "The origin game was unspecified. Using the default of None.",
        AlertType.INVALID => (originGame, officialOriginGame) switch
        {
            (not null, not null) => $"Neither the specified origin game '{originGame}' nor the official origin game '{officialOriginGame}'",
            (not null, null) => $"The specified origin game '{originGame}' doesn't",
            (null, not null) => $"The specified official origin game '{officialOriginGame}' doesn't",
            _ => throw InvalidAlertType(at) //should be unspecified
        } + " exist in this format. Using the default of None.",
        _ => throw InvalidAlertType(at)
    });
}