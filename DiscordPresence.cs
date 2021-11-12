using DiscordRPC;
using System;
using System.Linq;

namespace pkuManager;

public class DiscordPresence
{
    private const string DISCORD_APP_ID = "810636398016069653";
    private static readonly string[] VALID_BALLS = new[]
    {
        "pc", //when no pku is selected
		"beast",
        "cherish",
        "dive",
        "dream",
        "dusk",
        "fast",
        "friend",
        "great",
        "heal",
        "heavy",
        "level",
        "love",
        "lure",
        "luxury",
        "master",
        "moon",
        "nest",
        "net",
        "park",
        "poke",
        "premier",
        "quick",
        "repeat",
        "safari",
        "sport",
        "timer",
        "ultra"
    };

    private readonly DiscordRpcClient client;
    
    public string Box, Nickname, Collection;
    private string _ball;
    public string Ball
    {
        set
        {
            if (value is "pc") //pc icon
                _ball = "pc";
            else if (value?.ToLowerInvariant().EndsWith(" ball") is true)
            {
                string temp = value[0..^5].ToLowerInvariant(); //strip " ball" from value
                _ball = VALID_BALLS.Contains(temp) ? temp : "poke";
            }
            else //all other strings
                _ball = "poke";
        }
        get => _ball;
    }

    public DiscordPresence()
    {
        Ball = "pc";
        client = new DiscordRpcClient(DISCORD_APP_ID);
        client.Initialize();
        UpdatePresence();
    }

    public void UpdatePresence()
    {
        //Set the rich presence
        if (!Properties.Settings.Default.Hide_Discord_Presence)
        {
            client.SetPresence(new RichPresence()
            {
                Details = Collection is not null ? "Collection: " + Collection : null,
                State = Box is not null ? "Box: " + Box : null,
                Assets = new Assets()
                {
                    LargeImageKey = Ball,
                    LargeImageText = Nickname,
                    //SmallImageKey = "poke"
                }
            });
        }
        else
            client.ClearPresence();
    }

    // called when onClose event occurs (presumably when the form closes)
    public void Deinitialize(object sender, EventArgs e)
        => client.Dispose();
}