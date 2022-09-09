using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using pkuManager.WinForms.Formats.pku;
using pkuManager.Data;

namespace pkuManager.WinForms.Utilities;

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
    /// Gets the url and author of the requested sprite type with the given parameters,
    /// from the <see href="https://github.com/project-pku/pkuSprite">pkuSprite</see> repo.
    /// </summary>
    /// <param name="s_type">The type of sprite being requested (i.e. Front, Back, Box)</param>
    /// <param name="sfam">The SFAM corresponding to the desired sprite.</param>
    /// <param name="isShiny">Whether the shiny sprite should be returned.</param>
    /// <param name="isEgg">Whether the egg sprite should be returned.</param>
    /// <param name="isShadow">Whether the shadow sprite should be returned (if available).</param>
    /// <returns>A tuple of the url of the reuested sprite, and its author.</returns>
    public static (string url, string author) GetSprite(Sprite_Type s_type, SFAM sfam,
        bool isShiny, bool isEgg = false, bool isShadow = false)
    {
        // --------------
        // Get pku params
        // --------------
        List<string> base_keys = new() { "Sprites" }; //Sprites

        // Block type
        if (isEgg)
            base_keys.Add("Egg");
        else if (isShadow) //egg takes priority over shadow
            base_keys.Add("Shadow");
        else
            base_keys.Add("Default");

        // Shiny
        string shinyString = isShiny ? "Shiny" : "Regular";
        base_keys.Add(shinyString);

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
        (string, string) readURLBlock(List<string> keys)
        {
            keys.Add("URL");
            string url = SPRITE_DEX.ReadSpeciesDex<string>(sfam, keys.ToArray());
            keys.Remove("URL");

            keys.Add("Author");
            string author = SPRITE_DEX.ReadSpeciesDex<string>(sfam, keys.ToArray());
            keys.Remove("Author");

            return (url, author);
        }

        (string u, string a) = readURLBlock(base_keys); //try as is
        if (u is null && base_keys.Contains("Shadow")) //failed, try shadow -> default
        {
            List<string> keys = new(base_keys);
            var index = keys.FindIndex(c => c == "Shadow");
            keys[index] = "Default";
            (u, a) = readURLBlock(keys);
        }
        if (u is null) //failed, use default
        {
            List<string> def_keys = new() { "", "Forms", "", "Sprites", isEgg ? "Egg" : "Default", shinyString, typeString };
            return (SPRITE_DEX.ReadDataDex<string>(def_keys.Append("URL").ToArray()),
                SPRITE_DEX.ReadDataDex<string>(def_keys.Append("Author").ToArray()));
        }
        return (u, a); // sprite found
    }

    /// <param name="pku">The pku whose sprite is to be returned.</param>
    /// <inheritdoc cref="GetSprite(Sprite_Type, SFAM, bool, bool, bool)"/>
    public static (string url, string author) GetSprite(Sprite_Type s_type, pkuObject pku)
        => GetSprite(s_type, pku, pku.IsShiny(), pku.IsEgg(), pku.IsShadow());

    /// <summary>
    /// Gets the url and author of the box, front, and back sprites with the given parameters,
    /// from the <see href="https://github.com/project-pku/pkuSprite">pkuSprite</see> repo.
    /// </summary>
    /// <inheritdoc cref="GetSprite(Sprite_Type, SFAM, bool, bool, bool)"/>
    public static (string url, string author)[] GetSprites(SFAM sfam,
        bool isShiny, bool isEgg = false, bool isShadow = false) => new[]
    {
        GetSprite(Sprite_Type.Box, sfam, isShiny, isEgg, isShadow),
        GetSprite(Sprite_Type.Front, sfam, isShiny, isEgg, isShadow),
        GetSprite(Sprite_Type.Back, sfam, isShiny, isEgg, isShadow)
    };

    /// <param name="pku">The pku whose sprites are to be returned.</param>
    /// <inheritdoc cref="GetSprites(SFAM, bool, bool, bool)"/>
    public static (string url, string author)[] GetSprites(pkuObject pku) => new[]
    {
        GetSprite(Sprite_Type.Box, pku),
        GetSprite(Sprite_Type.Front, pku),
        GetSprite(Sprite_Type.Back, pku)
    };
}