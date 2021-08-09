using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pkuManager.Common;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

namespace pkuManager.Utilities
{
    public static class ImageUtil
    {
        public enum SpriteGroup
        {
            RED_BLUE,
            YELLOW,
            GOLD,
            SILVER,
            CRYSTAL,
            RUBY_SAPPHIRE,
            FRLG,
            EMERALD,
            DP,
            PLATINUM,
            HGSS,
            BW
        }

        private static readonly int WEB_TIMEOUT = 600;

        // Retrives an Image object from the given URL, returns null on failure.
        private static Image GetImageFromURL(string url)
        {
            if (url != null)
            {
                try
                {
                    WebRequest request = WebRequest.Create(url);
                    request.Timeout = WEB_TIMEOUT;
                    WebResponse response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    return new Bitmap(responseStream);
                }
                catch
                {
                    Console.WriteLine($"Error in retreiving the image at: {url}");
                }
            }
            return null;
        }

        // Gets the given pku's Pokestar sprite (as a gif) from B2W2 if it has one, null otherwise.
        // Source: https://projectpokemon.org/home/docs/spriteindex_148/nds-sprites-pokestar-studios-related-animated-images-r97/
        public static string getPokestarSpriteURL(PKUObject pku)
        {
            string url = "https://projectpokemon.org/images/sprites-models/pokestar/";
            int[] shiny = { 660, 682, 684 }; //id #s that have shiny sprites

            int? gen5id = pkxUtil.GetPokestarID(pku);
            if (!gen5id.HasValue)
                return null; //No pokestar image

            // This will be null if form does not match
            JToken checkedForm = null;
            if (pku.Form != null)
                checkedForm = pkxUtil.POKESTAR_DATA[pku.Species]?["Forms"]?[pku.Form];

            // Check if its a prop
            bool isProp = checkedForm != null && (pku.Form.ToLower() == "prop 1" ||
                                                  pku.Form.ToLower() == "prop 2" ||
                                                  pku.Form.ToLower() == "prop");
            if (isProp)
            {
                bool? notGrounded = (bool?)pkxUtil.POKESTAR_DATA[pku.Species]?["Forms"]?[pku.Form]?["Suspended"];
                if (notGrounded.HasValue && notGrounded.Value) //if its suspended by cables
                    url += "cable.gif";
                else
                    url += "ground.gif";

                return url;
            }

            // At this point only valid, non-prop pokemon are left (invalid forms are left too but gen5ID already accounts for this)
            url += gen5id.Value;
            if (pku.Shiny && shiny.Contains(gen5id.Value))
            {
                url += "-shiny";
            }
            return $"{url}.gif";
        }

        // -----------------------
        // Sprite Index Stuff
        // -----------------------

        private static readonly string SPRITE_INDICES_URL = "https://raw.githubusercontent.com/project-pku/pkuSprite/main/sprite-indices.json";
        public static readonly JObject MASTER_SPRITE_INDEX = GetMasterSpriteIndex();

        public enum SpriteTypes
        {
            Box,
            Front,
            Back
        }

        public enum SpriteBlock
        {
            Default,
            Egg,
            Shadow
        }

        private static JObject GetMasterSpriteIndex()
        {
            JObject masterSpriteIndex = new JObject(); //init with empty sprite index
            WebClient client = new WebClient();
            try
            {
                //Download sprite-indices.json
                string spriteIndiciesStr = client.DownloadString(SPRITE_INDICES_URL);
                JObject spriteIndiciesJSON = JObject.Parse(spriteIndiciesStr);

                foreach (var kvp in spriteIndiciesJSON)
                {
                    try
                    {
                        //Download <sprite-index>.json
                        string spriteIndexStr = client.DownloadString((string)kvp.Value);
                        JObject spriteIndexJSON = JObject.Parse(spriteIndexStr);

                        masterSpriteIndex = DataUtil.getCombinedJson(masterSpriteIndex, spriteIndexJSON);
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to read {kvp.Key} sprite index...");
                    }
                }
            }
            catch
            {
                Console.WriteLine("Failed to read sprite-indices.json...");
            }
            return masterSpriteIndex;
        }

        public static (string url, string author) GetSpriteURL(PKUObject pku, SpriteTypes st)
        {
            string species = pku.Species;
            string form = pku.Form == null ? pkuUtil.getDefaultForm(species, Registry.MASTER_DEX) : pku.Form;

            bool isEgg = pkuUtil.IsAnEgg(pku);
            bool isShadow = pku.Shadow_Info?.Shadow == true;
            SpriteBlock sb;
            if (isEgg)
                sb = SpriteBlock.Egg;
            else if (isShadow) //egg takes priority over shadow
                sb = SpriteBlock.Shadow;
            else
                sb = SpriteBlock.Default;
            bool isShiny = pku.Shiny;
            bool isFemale = pkxUtil.GetGender(pku.Gender, false) == Gender.Female;
            return GetSpriteURL(species, form, sb, isShiny, isFemale, st);
        }

        public static (string url, string author) GetSpriteURL(string species, string form, SpriteBlock sb, bool isShiny, bool isFemale, SpriteTypes st)
        {
            JToken speciesBlock = DataUtil.TraverseJTokenCaseInsensitive(MASTER_SPRITE_INDEX, species);
            string defaultForm = pkuUtil.getDefaultForm(species, Registry.MASTER_DEX);
            string blockString = sb switch
            {
                SpriteBlock.Default => "Default",
                SpriteBlock.Egg => "Egg",
                SpriteBlock.Shadow => "Shadow",
                _ => throw new NotImplementedException(),
            };
            string shinyString = isShiny ? "Shiny" : "Regular";
            string typeString = st switch
            {
                SpriteTypes.Box => "Box",
                SpriteTypes.Front => "Front",
                SpriteTypes.Back => "Back",
                _ => throw new NotImplementedException(),
            };

            // Try getting given sprite
            JToken spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(speciesBlock, "Forms", form, "Sprites", blockString, shinyString);
            if (isFemale)
                spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "Female");
            spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, typeString);
            string url = (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "URL");

            if (url != null) // sprite exists, return it
                return (url, (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "Author"));
            else //sprite doesn't exist, find a fallback
            {
                if (isFemale) //try default gender, if no female sprites
                {
                    spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(speciesBlock, "Forms", form, "Sprites", blockString, shinyString, typeString);
                    url = (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "URL");
                    if (url != null) // default gender sprite exists, return it
                        return (url, (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "Author"));
                }
                if (isShiny) //try regular, if no shiny sprites
                {
                    if (isFemale)
                    {
                        spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(speciesBlock, "Forms", form, "Sprites", blockString, "Regular", "Female", typeString);
                        url = (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "URL");
                        if (url != null)
                            return (url, (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "Author"));
                    }
                    else
                    {
                        spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(speciesBlock, "Forms", form, "Sprites", blockString, "Regular", typeString);
                        url = (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "URL");
                        if (url != null)
                            return (url, (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "Author"));
                    }
                }
                if (sb == SpriteBlock.Shadow) // try default block, if no shadow sprites
                    return GetSpriteURL(species, form, SpriteBlock.Default, isShiny, isFemale, st);
                if (!DataUtil.stringEqualsCaseInsensitive(form, defaultForm)) //try default form, if no given form sprites
                    return GetSpriteURL(species, defaultForm, sb, isShiny, isFemale, st);

                //no sprites found, use default sprites
                if (sb == SpriteBlock.Egg) //default egg sprites
                {
                    spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(MASTER_SPRITE_INDEX, "", "Forms", "", "Sprites", "Egg", shinyString, typeString);
                    return ((string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "URL"), (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "Author"));
                }
                else //unkown species (i.e. ?)
                {
                    spriteAuthorBlock = DataUtil.TraverseJTokenCaseInsensitive(MASTER_SPRITE_INDEX, "", "Forms", "", "Sprites", "Default", shinyString, typeString);
                    return ((string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "URL"), (string)DataUtil.TraverseJTokenCaseInsensitive(spriteAuthorBlock, "Author"));
                }
            }
        }


        // -----------------------
        // Sprite Index Work
        // -----------------------

        private static Dictionary<string, SpriteIndexStructure.SpeciesBlock> GetSpriteIndexFromText(string jsontext)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, SpriteIndexStructure.SpeciesBlock>>(jsontext);
        }

        public class SpriteIndexStructure
        {
            public partial class SpeciesBlock
            {
                [JsonProperty("Forms")]
                public Dictionary<string, FormBlock> Forms;
            }

            public partial class FormBlock
            {
                [JsonProperty("Sprites")]
                public SpritesBlock Sprites;
            }

            public partial class SpritesBlock
            {
                [JsonProperty("Default")]
                public ShinyBlock Default;

                [JsonProperty("Egg")]
                public ShinyBlock Egg;

                [JsonProperty("Shadow")]
                public ShinyBlock Shadow;
            }

            public partial class ShinyBlock
            {
                [JsonProperty("Regular")]
                public SpriteTypeBlock Regular;

                [JsonProperty("Shiny")]
                public SpriteTypeBlock Shiny;
            }

            public partial class SpriteTypeBlock
            {
                //Sprites for default gender
                [JsonProperty("Box")]
                public SpriteAuthorBlock Box;

                [JsonProperty("Front")]
                public SpriteAuthorBlock Front;

                [JsonProperty("Back")]
                public SpriteAuthorBlock Back;

                //Sprites for female if species has multiple genders.
                [JsonProperty("Female")]
                public FemaleBlock Female;
            }

            public partial class FemaleBlock
            {
                [JsonProperty("Box")]
                public SpriteAuthorBlock Box;

                [JsonProperty("Front")]
                public SpriteAuthorBlock Front;

                [JsonProperty("Back")]
                public SpriteAuthorBlock Back;
            }

            public partial class SpriteAuthorBlock
            {
                [JsonProperty("URL")]
                public string URL;

                [JsonProperty("Author")]
                public string Author;
            }
        }
    }
}

