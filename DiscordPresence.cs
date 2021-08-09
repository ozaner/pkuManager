using DiscordRPC;
using System;
using System.Collections.Generic;

namespace pkuManager
{
    public class DiscordPresence
    {
        private static List<string> VALID_BALLS = new List<string>()
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

        DiscordRpcClient client;
        public string box, nickname, collection;

        private string _ball;
        public string ball
        {
            set
            {
                if (value == "pc")
                    _ball = "pc";
                else if (value?.Length > 5)
                {
                    string temp = value.Substring(0, value.Length - 5).ToLowerInvariant(); //strip " ball" from value
                    if (VALID_BALLS.Contains(temp))
                        _ball = temp;
                    else
                        _ball = "poke";
                }
                else
                    _ball = "poke";
            }
            get
            {
                return _ball;
            }
        }

        public DiscordPresence()
        {
            ball = "pc";
            client = new DiscordRpcClient("810636398016069653");

            //Set the logger
            //client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            //client.OnPresenceUpdate += (sender, e) =>
            //{
            //	Console.WriteLine("Received Update! {0}", e.Presence);
            //};

            //Connect to the RPC
            client.Initialize();

            setPresence();
        }

        public void setPresence()
        {
            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = collection != null ? "Collection: " + collection : null,
                State = box != null ? "Box: " + box : null,
                Assets = new Assets()
                {
                    LargeImageKey = ball,
                    LargeImageText = nickname,
                    //SmallImageKey = "poke"
                }
            });
        }

        // called when onClose event occurs (presumably when the form closes)
        public void Deinitialize(Object sender, EventArgs e)
        {
            client.Dispose();
        }
    }
}
