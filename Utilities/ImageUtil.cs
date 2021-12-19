using Newtonsoft.Json.Linq;
using pkuManager.Common;
using pkuManager.pku;
using System;
using System.Linq;
using System.Collections.Generic;

namespace pkuManager.Utilities;

public static class ImageUtil
{
    /// <summary>
    /// URL to the <see href="https://github.com/project-pku/pkuSprite">pkuSprite</see> repository on Github.<br/>
    /// This is where all sprites used in pkuManager are sourced from.
    /// </summary>
    private const string SPRITE_DEX_URL = "https://raw.githubusercontent.com/project-pku/pkuSprite/main/masterSpriteDex.json";

    /// <summary>
    /// The master SpriteDex containing all Pokemon species sprites used in pkuManager.<br/>
    /// </summary>
    private static readonly JObject SPRITE_DEX = DataUtil.DownloadJson(SPRITE_DEX_URL, "SpriteDex");

    /// <summary>
    /// The different types of sprites listed in the <see cref="SPRITE_DEX"/>.
    /// </summary>
    public enum Sprite_Type
    {
        Box,
        Front,
        Back
    }

    /// <summary>
    /// Gets the url and author of the requested sprite type of the given pku, according to <see href="https://github.com/project-pku/pkuSprite">pkuSprite</see>.
    /// </summary>
    /// <param name="pku">The pku whose sprite is to be returned.</param>
    /// <param name="s_type">The type of sprite being requested (i.e. Front, Back, Box)</param>
    /// <returns>A tuple of the url of the reuested sprite, and its author.</returns>
    public static (string url, string author) GetSprite(pkuObject pku, Sprite_Type s_type)
    {
        // --------------
        // Get pku params
        // --------------
        List<string> base_keys = new() { "Sprites" }; //Sprites

        // Block type
        if (pku.IsEgg())
            base_keys.Add("Egg");
        else if (pku.IsShadow()) //egg takes priority over shadow
            base_keys.Add("Shadow");
        else
            base_keys.Add("Default");

        // Shiny
        string shinyString = pku.IsShiny() ? "Shiny" : "Regular";
        base_keys.Add(shinyString);

        // Female
        if (pku.Gender.ToEnum<Gender>() is Gender.Female)
            base_keys.Add("Female");

        // Sprite type
        string typeString = s_type switch
        {
            Sprite_Type.Box => "Box",
            Sprite_Type.Front => "Front",
            Sprite_Type.Back => "Back",
            _ => throw new NotImplementedException(),
        };
        base_keys.Add(typeString);


        // --------------
        // Request sprite & fallback if needed
        // --------------
        (string u, string a) = (null, null);

        (string, string) readURLBlock(List<string> keys)
        {
            keys.Add("URL");
            string url = SPRITE_DEX.ReadSpeciesDex<string>(pku, keys.ToArray());
            keys.Remove("URL");

            keys.Add("Author");
            string author = SPRITE_DEX.ReadSpeciesDex<string>(pku, keys.ToArray());
            keys.Remove("Author");

            return (url, author);
        }
        (string, string) trySub(string a, string b)
        {
            List<string> keys = new(base_keys);
            if (b is not null)
            {
                var index = keys.FindIndex(c => c == a);
                keys[index] = b;
            }
            else
                keys.Remove(a);
                
            return readURLBlock(keys);
        }

        //try as is
        (u, a) = readURLBlock(base_keys);
        if (u is not null) goto Finish;
                
        //shadow + female -> shadow
        if(base_keys.Contains("Shadow") && base_keys.Contains("Female"))
        {
            (u, a) = trySub("Female", null);
            if (u is not null) goto Finish;
        }

        //shadow -> default
        if (base_keys.Contains("Shadow"))
        {
            (u, a) = trySub("Shadow", "Default");
            if (u is not null) goto Finish;
        }

        //default + female -> default
        if (base_keys.Contains("Default"))
        {
            (u, a) = trySub("Female", null);
            if (u is not null) goto Finish;
        }


        // --------------
        // Return, using default if needed
        // --------------
    Finish:

        //no sprite found, use default sprites
        if (u is null)
        {
            List<string> def_keys = new() { "", "Forms", "", "Sprites", pku.IsEgg() ? "Egg" : "Default", shinyString, typeString };
            return (SPRITE_DEX.ReadDataDex<string>(def_keys.Append("URL").ToArray()),
                SPRITE_DEX.ReadDataDex<string>(def_keys.Append("Author").ToArray()));
        }

        return (u, a); // sprite found
    }
}