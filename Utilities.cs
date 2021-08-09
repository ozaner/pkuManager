using APNGLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pkuManager.APNGViewer;
using pkuManager.Exporters;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace pkuManager
{
    static class Utilities
    {
        private static JObject SPRITE_DATA = getJson("spriteData");
        private static int WEB_TIMEOUT = 3000; // 3 second wait time before image method gives up and uses ?
        private static JObject pkSpriteData = getJson("pokespriteData"); //load sprite url info

        // Imports a pku from a fileinfo object.
        // Includes all tags used in the supported formats.
        public static PKUObject ImportPKU(FileInfo pkuFile)
        {
            string pkuString = File.ReadAllText(pkuFile.FullName);
            PKUObject pku = JsonConvert.DeserializeObject<PKUObject>(pkuString);
            return pku;
        }

        // Gets the given pku's box sprite from the pokesprite github repo
        public static Image getPKUIcon(PKUObject pku)
        {
            if (pku.Species == null)
                return Properties.Resources.unknown; // no species

            int? dex = pkCommons.GetNationalDex(pku);
            // deal with eggs
            if (pkCommons.isAnEgg(pku))
            {
                if (dex == 490)
                    return Properties.Resources.egg_manaphy; //Manaphy egg
                else
                    return Properties.Resources.egg; //Any other egg
            }

            // get species json name
            string pkName = null;
            if (dex.HasValue)
                pkName = "/" + (string)pkSpriteData[dex.Value.ToString("D3")]["slug"]["eng"]; //get species name from json

            if (pkName == null)
                return Properties.Resources.unknown; //if species is not official (aka not in json)

            // Get form
            string form = "";
            if (pku.Form != null)
            {
                if (pku.Form == "?")
                    form = "question";
                else if (pku.Form == "!")
                    form = "exclamation";
                else
                    form = pku.Form.ToLower();

                object req = pkSpriteData[dex.Value.ToString("D3")]["gen-8"]["forms"][form];
                if (req == null)
                    form = "";
            }

            //shinyness
            string shiny;
            if (pku.Shiny)
                shiny = "/shiny";
            else
                shiny = "/regular";

            // is female unique? (for the form recoginized above)
            string female = "";
            if (pku.Gender != null)
            {
                object hasFemale;
                if (form == "") //default form
                    hasFemale = pkSpriteData[dex.Value.ToString("D3")]["gen-8"]["forms"]["$"]["has_female"];
                else //for a specific form
                {
                    try
                    {
                        hasFemale = pkSpriteData[dex.Value.ToString("D3")]["gen-8"]["forms"][form]["has_female"];
                    }
                    catch
                    {
                        hasFemale = null;
                    }
                }

                if (hasFemale != null && pkCommons.GetGenderID(pku.Gender, false) == Gender.Female)
                    female = "/female";
            }

            //Fix form alias
            if (form != "")
            {
                string alias = null;
                try
                {
                    alias = (string)pkSpriteData[dex.Value.ToString("D3")]["gen-8"]["forms"][form]["is_alias_of"];
                }
                catch { }
                if (alias == "$")
                    form = "";
                else if (alias != null)
                    form = alias;
            }

            //put together url
            string iconUrl = "https://raw.githubusercontent.com/msikma/pokesprite/master/pokemon-gen8";
            iconUrl += shiny + female + pkName + (form == "" ? "" : "-") + form + ".png";

            // Request icon
            System.Net.WebRequest request = System.Net.WebRequest.Create(iconUrl);
            try
            {
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                return new Bitmap(responseStream);
            }
            catch //if request fails use default icon
            {
                return Properties.Resources.unknown; //TODO: make failure time shorter...? just download sprite sheet
            }
        }

        // Gets the given pku's origin game sprite, and returns it as either an Image or APNG object.
        // Supports Pokestar sprites, Uses HOME for everything else.
        // TODO: Support backsprites when available, add functionality to doubleclick of apng box.
        //       Support DP + Pt + RS + E + FRLG + XD + Colo
        public static object getPKUSprite(PKUObject pku)
        {
            object image = null;
            int? dex = pkCommons.GetNationalDex(pku);
            int? gameID = pkCommons.GetGameID(pku);
            if (pkCommons.GetPokestarID(pku).Item1.HasValue)
                image = getPokestarSprite(pku);
            else if (dex.HasValue && dex <= 649 && gameID.HasValue)
            {
                if (gameID.Value <= 15) //gen 1-4
                    image = getHGSSSprite(pku);
                else if (gameID.Value <= 23)
                    image = getB2W2Sprite(pku);
            }
            else if (pkCommons.GetNationalDex(pku) != -1)
                image = getHOMESprite(pku);

            if (image == null)
                return Properties.Resources.unknown2; //default return is unknown2.png
                                                      //return getAPNGFromURL("https://archives.bulbagarden.net/media/upload/a/a8/Box_XD_003.png");

            return image;
        }

        // Gets the given pku's Pokemon Home sprite as a png, null otherwise
        // Source: https://pokemondb.net/sprites
        public static Image getHOMESprite(PKUObject pku)
        {
            string spriteUrl = "https://img.pokemondb.net/sprites/home";

            int? dex = pkCommons.GetNationalDex(pku);
            if (dex.HasValue)
            {
                if (pkCommons.isAnEgg(pku))
                {
                    spriteUrl += "/normal/egg.png";
                    goto Request;
                }
            }
            else
                return null;

            string pkName;
            if (dex.HasValue && dex <= 890) //up to eternatus w/ sprites
                pkName = (string)pkSpriteData[dex.Value.ToString("D3")]["slug"]["eng"];
            else
                return null;

            // Get form
            string form = "";
            if (pku.Form != null)
            {
                if (pku.Form == "?")
                    form = "question";
                else if (pku.Form == "!")
                    form = "exclamation";
                else
                    form = pku.Form.ToLower();

                object req = pkSpriteData[dex.Value.ToString("D3")]["gen-8"]["forms"][form];
                if (req == null)
                    form = "";
                else
                    form = "-" + form;
            }

            //int versionID;
            //if (pku.Game_InfoSpecified && pku.Game_Info.Origin_GameSpecified)
            //    versionID = Utilities.GetGameID(pku.Game_Info.Origin_Game, 8, "ENG");
            //else
            //    versionID = 0;

            //string version = versionID switch
            //{
            //    1 => "/ruby-sapphire",
            //    2 => "/ruby-sapphire",
            //    3 => "/emerald",
            //    4 => "/firered-leafgreen",
            //    5 => "/firered-leafgreen",
            //    15 => "/colo-xd",
            //    10 => "/diamond-pearl",
            //    11 => "/diamond-pearl",
            //    12 => "/platinum",
            //    7 => "/heartgold-soulsilver",
            //    8 => "/heartgold-soulsilver",
            //    20 => "/black-white",
            //    21 => "/black-white",
            //    22 => "/black-white",
            //    23 => "/black-white",
            //    24 => "/x-y",
            //    25 => "/x-y",
            //    26 => "/x-y", //AS-OR sprites are bad
            //    27 => "/x-y",
            //    30 => "/sun-moon",
            //    31 => "/sun-moon",
            //    32 => "/ultra-sun-ultra-moon",
            //    33 => "/sun-moon",
            //    _ => "/home"
            //};

            //// Check for species that don't exist in that game
            //if (((versionID <= 5 || versionID == 15) && dex > 386) ||
            //     (versionID <= 8 && dex > 493) ||
            //     (versionID <= 23 && dex > 649))
            //{
            //    version = "/home";
            //}

            //spriteUrl += version;

            if (pku.Shiny)
                spriteUrl += "/shiny";
            else
                spriteUrl += "/normal";
            spriteUrl += "/" + pkName + form + ".png";

        Request:
            return getImageFromURL(spriteUrl);
        }

        // Gets the given pku's Pokestar sprite (as a gif) from B2W2 if it has one, null otherwise.
        // Source: https://projectpokemon.org/home/docs/spriteindex_148/nds-sprites-pokestar-studios-related-animated-images-r97/
        public static Image getPokestarSprite(PKUObject pku)
        {
            string url = "https://projectpokemon.org/images/sprites-models/pokestar/";
            int[] shiny = { 660, 682, 684 }; //id #s that have shiny sprites

            (int? gen5id, _) = pkCommons.GetPokestarID(pku);
            if (!gen5id.HasValue)
                return null; //No pokestar image

            // This will be null if form does not match
            JToken checkedForm = null;
            if (pku.Form != null)
                checkedForm = pkCommons.POKESTAR_DATA[pku.Species]?["Forms"]?[pku.Form];

            // Check if its a prop
            bool isProp = checkedForm != null && (pku.Form.ToLower() == "prop 1" ||
                                                  pku.Form.ToLower() == "prop 2" ||
                                                  pku.Form.ToLower() == "prop");
            if (isProp)
            {
                bool? notGrounded = (bool?)pkCommons.POKESTAR_DATA[pku.Species]?["Forms"]?[pku.Form]?["Not Grounded"];
                if (notGrounded.HasValue && notGrounded.Value) //if its suspended by cables
                    url += "cable.gif";
                else
                    url += "ground.gif";

                return getImageFromURL(url);
            }

            // At this point only valid, non-prop pokemon are left (invalid forms are left too but gen5ID already accounts for this)
            url += gen5id.Value;
            if (pku.Shiny && shiny.Contains(gen5id.Value))
            {
                url += "-shiny";
            }
            return getImageFromURL(url + ".gif");
        }

        //TODO: this uses gen 4 suffix, also includes pichu thing...
        // Gets the given pku's B2W2 sprite as an APNG, null if not a gen 5 species
        // Source: https://archives.bulbagarden.net/wiki/Category:Black_2_and_White_2_sprites
        public static APNG getB2W2Sprite(PKUObject pku)
        {
            int? dex = pkCommons.GetNationalDex(pku);

            // Deal with eggs
            if (pkCommons.isAnEgg(pku))
            {
                if (dex == 490) //if manaphy
                    return getAPNGFromURL("https://bulbapedia.bulbagarden.net/wiki/Special:FilePath/Spr_5b_ManaphyEgg.png");
                else
                    return getAPNGFromURL("https://bulbapedia.bulbagarden.net/wiki/Special:FilePath/Spr_5b_Egg.png");
            }

            string url = "https://bulbapedia.bulbagarden.net/wiki/Special:FilePath/Spr_5b_";

            if (dex.HasValue && dex <= 493) //Bulbasaur to Arceus
                url += dex.Value.ToString("D3");
            else // Anything else
                return null;

            if (pku.Form != null) // must be a valid gen 4 species at this point
            {
                string suffix = (string)SPRITE_DATA[pku.Species]?["Forms"]?[pku.Form]?["Bulbapedia Suffix"];
                if (suffix != null && suffix != "N") //N is for spiky pichu
                    url += suffix;
            }

            if (pkCommons.hasGenderDifferences(pku, 4))
            {
                Gender? gender = pkCommons.GetGenderID(pku.Gender, false);
                if (gender == Gender.Female)
                    url += " f";
                else // if gender is male or anything else
                    url += " m";
            }

            if (pku.Shiny)
                url += "_s";

            return getAPNGFromURL(url + ".png");
        }

        // Gets the given pku's HGSS sprite as an APNG, null if not a gen 4 species
        // Source: https://archives.bulbagarden.net/wiki/Category:HeartGold_and_SoulSilver_sprites
        public static APNG getHGSSSprite(PKUObject pku)
        {
            int? dex = pkCommons.GetNationalDex(pku);

            // Deal with eggs
            if (pkCommons.isAnEgg(pku))
            {
                if (dex == 490) //if manaphy
                    return getAPNGFromURL("https://cdn.bulbagarden.net/upload/2/23/Spr_5b_ManaphyEgg.png"); //actually gen 5 sprite but it is identical to gen 4 just animated
                else
                    return getAPNGFromURL("https://cdn.bulbagarden.net/upload/4/45/Spr_4d_Egg.png");
            }

            string url = "https://bulbapedia.bulbagarden.net/wiki/Special:FilePath/Spr_4h_";

            if (dex.HasValue && dex <= 493) //Bulbasaur to Arceus
                url += dex.Value.ToString("D3");
            else // Anything else
                return null;

            if (pku.Form != null) // must be a valid gen 4 species at this point
            {
                string suffix = (string)SPRITE_DATA[pku.Species]?["Forms"]?[pku.Form]?["Bulbapedia Suffix"];
                if (suffix != null)
                    url += suffix;
            }

            if (pkCommons.hasGenderDifferences(pku, 4))
            {
                Gender? gender = pkCommons.GetGenderID(pku.Gender, false);
                if (gender == Gender.Female)
                    url += " f";
                else // if gender is male or anything else
                    url += " m";
            }

            if (pku.Shiny)
                url += "_s";

            APNG2 apng = (APNG2)getAPNGFromURL(url + ".png");
            if (apng != null)
                apng.MaxPlays = 1; //These animations don't loop
            return apng;
        }

        // Returns an APNG from the given URL, returns null on failure.
        private static APNG getAPNGFromURL(string url)
        {
            APNG2 apng;
            if (url != null)
            {
                try
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                    request.Timeout = WEB_TIMEOUT;
                    System.Net.WebResponse response = request.GetResponse();
                    MemoryStream memStream;
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        memStream = new MemoryStream();

                        byte[] buffer = new byte[1024];
                        int byteCount;
                        do
                        {
                            byteCount = responseStream.Read(buffer, 0, buffer.Length);
                            memStream.Write(buffer, 0, byteCount);
                        } while (byteCount > 0);
                    }

                    // If you're going to be reading from the stream afterwords you're going to want to seek back to the beginning.
                    memStream.Seek(0, SeekOrigin.Begin);

                    apng = new APNG2();
                    apng.Load(memStream);
                    return apng;
                }
                catch { }
            }

            // If above fails or url is unknown
            //MemoryStream stream = new MemoryStream();
            //Properties.Resources.unknown2.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            //stream.Position = 0;
            //apng = new APNG2();
            //apng.Load(stream);
            //return apng;
            return null;
        }

        // Retrives an Image object from the given URL, returns null on failure.
        private static Image getImageFromURL(string url)
        {
            if (url != null)
            {
                try
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                    request.Timeout = WEB_TIMEOUT;
                    System.Net.WebResponse response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    return new Bitmap(responseStream);
                }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// Gets a JObject from the given json file located in the resources.resx file
        /// </summary>
        /// <param name="filename">The filename (sans the .json) of the desired json.</param>
        /// <returns></returns>
        public static JObject getJson(string filename)
        {
            return JObject.Parse(Properties.Resources.ResourceManager.GetString(filename));
        }

        /// <summary>
        /// Returns a merged JObject with the mergings taking place one after the other.
        /// </summary>
        /// <param name="jobjs">The JObjects to be merged, in order.</param>
        /// <returns></returns>
        public static JObject getCombinedJson(JObject[] jobjs)
        {
            if (jobjs == null || jobjs.Length == 0)
                return null;

            JObject combined = jobjs[0];
            foreach (JObject s in jobjs.Skip(1))
                combined.Merge(s);

            return combined;
        }

        /// <summary>
        ///  Given a pku and some species data (in the form of a json),
        ///  will return the default form of the given pku's species
        /// </summary>
        /// <param name="pku">The pku whose species will be checked.</param>
        /// <param name="data">The json data of all the relevant species.</param>
        /// <returns></returns>
        public static string getDefaultForm(PKUObject pku, JObject data)
        {
            if (pku.Species == null)
                return null;

            JObject forms = (JObject)data?[pku.Species]?["Forms"];
            if (forms == null)
                return null; // No listed forms, no default

            foreach (var j in forms)
            {
                bool? isDefault = (bool?)j.Value["Default"];
                if (isDefault.HasValue && isDefault.Value)
                    return j.Key;
            }
            throw new Exception("This data file has forms listed for this species, but no default!");
            //return null; // There are forms, but no default. Shouldn't happen.
        }
    }
}