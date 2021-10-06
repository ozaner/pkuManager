using Newtonsoft.Json.Linq;
using pkuManager.Common;
using pkuManager.pku;
using System;
using System.Collections.Generic;

namespace pkuManager.Utilities
{
    public static class ImageUtil
    {
        // -----------------------
        // Sprite Index Stuff
        // -----------------------

        /// <summary>
        /// URL to the <see href="https://github.com/project-pku/pkuSprite">pkuSprite</see> repository on Github.<br/>
        /// This is where all sprites used in pkuManager are sourced from.
        /// </summary>
        private static readonly string SPRITE_INDICES_URL = "https://raw.githubusercontent.com/project-pku/pkuSprite/main/sprite-indices.json";

        /// <summary>
        /// A JSON index of all Pokemon species sprites used in pkuManager.<br/>
        /// Compiled from the <see cref="SPRITE_INDICES_URL">sprite-indicies.json</see> file on the pkuSprite repo.
        /// </summary>
        private static readonly JObject MASTER_SPRITE_INDEX = DexUtil.GetMasterDatadex(SPRITE_INDICES_URL);

        /// <summary>
        /// The different types of sprites listed in the <see cref="MASTER_SPRITE_INDEX"/>.
        /// </summary>
        public enum Sprite_Type
        {
            Box,
            Front,
            Back
        }

        /// <summary>
        /// The different types of sprite blocks listed in the <see cref="MASTER_SPRITE_INDEX"/>.
        /// </summary>
        public enum Block_Type
        {
            Default,
            Egg,
            Shadow
        }

        /// <summary>
        /// Gets the url and author of the requested sprite type of the given pku, according to <see href="https://github.com/project-pku/pkuSprite">pkuSprite</see>.
        /// </summary>
        /// <param name="pku">The pku whose sprite is to be returned.</param>
        /// <param name="s_type">The type of sprite being requested (i.e. Front, Back, Box)</param>
        /// <returns>A tuple of the url of the reuested sprite, and its author.</returns>
        public static (string url, string author) GetSprite(pkuObject pku, Sprite_Type s_type)
        {
            // Determine Block type
            Block_Type b_type;
            if (pku.IsEgg())
                b_type = Block_Type.Egg;
            else if (pku.Shadow_Info?.Shadow == true) //egg takes priority over shadow
                b_type = Block_Type.Shadow;
            else
                b_type = Block_Type.Default;

            bool isFemale = pku.Gender.ToEnum<Gender>() is Gender.Female;
            return GetSprite(pku.Species, DexUtil.GetCastableForms(pku), pku.Appearance ?? Array.Empty<string>(), b_type, pku.IsShiny(), isFemale, s_type);
        }

        //request -> sprite
        public static (string url, string author) GetSprite(string species, List<string> castableForms, string[] appearance, Block_Type b_type, bool isShiny, bool isFemale, Sprite_Type s_type)
        {
            (string url, string author) = GetSpriteHelper(species, castableForms, appearance, b_type, isShiny, isFemale, s_type);

            if(url != null)
                return (url, author);
            else
            {
                // no shadow sprite found, try default
                if (b_type == Block_Type.Shadow)
                {
                    (url, author) = GetSpriteHelper(species, castableForms, appearance, Block_Type.Default, isShiny, isFemale, s_type);
                    
                    if (url != null) //default found
                        return (url, author);

                    // no shiny sprite found, try default regular
                    if (isShiny)
                        (url, author) = GetSpriteHelper(species, castableForms, appearance, Block_Type.Default, false, isFemale, s_type);

                    if (url != null) //default regular found
                        return (url, author);
                }

                // no shiny sprite found, try regular
                if (isShiny)
                    (url, author) = GetSpriteHelper(species, castableForms, appearance, b_type, false, isFemale, s_type);

                if (url != null) //regular found
                    return (url, author);

                //no sprites found, use default sprites
                string shinyString = isShiny ? "Shiny" : "Regular";
                string typeString = s_type switch
                {
                    Sprite_Type.Box => "Box",
                    Sprite_Type.Front => "Front",
                    Sprite_Type.Back => "Back",
                    _ => throw new NotImplementedException(),
                };
                JToken block = MASTER_SPRITE_INDEX.TraverseJTokenCaseInsensitive("", "Forms", "", "Sprites", b_type == Block_Type.Egg ? "Egg" : "Default", shinyString, typeString);
                return ((string)block.TraverseJTokenCaseInsensitive("URL"), (string)block.TraverseJTokenCaseInsensitive("Author"));
            }
        }

        //given a request, trys to find a matching sprite of the species in the same block. null on failure
        public static (string url, string author) GetSpriteHelper(string species, List<string> castableForms, string[] appearance, Block_Type b_type, bool isShiny, bool isFemale, Sprite_Type s_type)
        {
            static JToken CheckFemale(JToken block, bool isFemale, string typeString)
            {
                if (isFemale)
                {
                    JToken blockA = block.TraverseJTokenCaseInsensitive("Female", typeString);
                    if (blockA != null)
                        return blockA;
                }
                return block.TraverseJTokenCaseInsensitive(typeString);
            }

            JToken speciesBlock = MASTER_SPRITE_INDEX.TraverseJTokenCaseInsensitive(species, "Forms");
            string blockString = b_type switch
            {
                Block_Type.Default => "Default",
                Block_Type.Egg => "Egg",
                Block_Type.Shadow => "Shadow",
                _ => throw new NotImplementedException(),
            };
            string shinyString = isShiny ? "Shiny" : "Regular";
            string typeString = s_type switch
            {
                Sprite_Type.Box => "Box",
                Sprite_Type.Front => "Front",
                Sprite_Type.Back => "Back",
                _ => throw new NotImplementedException(),
            };

            JToken formBlock, check;
            foreach (string searchableForm in castableForms)
            {
                formBlock = speciesBlock.TraverseJTokenCaseInsensitive(searchableForm);

                // try each appearance, and return when one is found
                foreach (string app in appearance)
                {
                    check = formBlock.TraverseJTokenCaseInsensitive("Appearance", app, "Sprites", blockString, shinyString);
                    check = CheckFemale(check, isFemale, typeString);
                    if (check != null)
                        return ((string)check.TraverseJTokenCaseInsensitive("URL"), (string)check.TraverseJTokenCaseInsensitive("Author"));
                }

                //no matching appearance found, try null appearance
                check = formBlock.TraverseJTokenCaseInsensitive("Sprites", blockString, shinyString);
                check = CheckFemale(check, isFemale, typeString);
                if (check != null)
                    return ((string)check.TraverseJTokenCaseInsensitive("URL"), (string)check.TraverseJTokenCaseInsensitive("Author"));
            }

            return (null, null); //failed to find a sprite.
        }
    }
}

