using DiscordRPC;
using DiscordRPC.Helper;
using System;
using System.Linq;
using System.Text;

namespace pkuManager.WinForms;

public class DiscordPresence
{
    /* ------------------------------------
     * Constants
     * ------------------------------------
    */
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
        "ultra",
        "nuclear",
        "atom"
    };

    private const int TEXT_BYTE_SIZE = 128;
    private const int IMAGE_KEY_BYTE_SIZE = 256; //- 12; //-12 accounts for "mp:external/"


    /* ------------------------------------
     * State
     * ------------------------------------
    */
    private string _collection, _box, _nickname, _spriteurl, _ball;

    private static string ValidateString(string str, int bytes)
    {
        if (str is null)
            return null;
        if (str.WithinLength(bytes, Encoding.UTF8))
            return str;
        return null;
    }

    public void ClearState()
        => _collection = _box = _nickname = _spriteurl = _ball = null;

    public string Box
    {
        get => _box;
        set => _box = ValidateString(value, TEXT_BYTE_SIZE);
    }

    public string Nickname
    {
        get => _nickname;
        set => _nickname = ValidateString(value, TEXT_BYTE_SIZE);
    }

    public string Collection
    {
        get => _collection;
        set => _collection = ValidateString(value, TEXT_BYTE_SIZE);
    }

    public string SpriteURL
    {
        get => _spriteurl;
        set => _spriteurl = ValidateString(value, IMAGE_KEY_BYTE_SIZE);
    }

    //Legacy - still using preuploaded images
    public string Ball
    {
        set
        {
            if (value?.ToLowerInvariant().EndsWith(" ball") is true)
            {
                string temp = value[0..^5].ToLowerInvariant().Replace('é', 'e'); //strip " ball" from value
                _ball = VALID_BALLS.Contains(temp) ? temp : "poke";
            }
            else //all other strings
                _ball = "poke";
        }
        get => _ball;
    }


    /* ------------------------------------
     * Init + Set
     * ------------------------------------
    */
    private readonly DiscordRpcClient client;

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
                    LargeImageKey = SpriteURL,
                    LargeImageText = Nickname,
                    SmallImageKey = SpriteURL == "pc" ? null : Ball
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